using System;
using System.Collections.Generic;
using System.Linq;
using Galactic1.Code.GameDatabase;
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Code.Systems.Runtime.Building;
using Galactic1.Code.Systems.GameTime;
using Galactic1.Game.Buildings.Proxy;
using Galactic1.Game.Meta.Items;
using Galactic1.Runtime.Production;

namespace Galactic1.Game.Runtime.Production
{
    /// <summary>
    /// Базовый runtime для всех production-станций.
    ///
    /// Отвечает за:
    /// • очередь
    /// • время
    /// • порядок слотов
    /// • запуск следующего
    /// • commit lifecycle
    ///
    /// Не знает:
    /// • про инвентарь
    /// • про UI
    /// • про конкретную механику станции
    /// </summary>
    public abstract class BaseProductionStationRuntime : 
        BaseCampFacilityRuntime,
        IProductionStationRuntime
    {
        
        protected abstract int MaxSlots { get; }

        public IReadOnlyList<ProductionJobProxy> GetQueue => Proxy.ProductionQueue;

        private EventBinding<ProductionOrderAutoCollectedEvent> _eventBinding;


        protected BaseProductionStationRuntime(
            FacilityProxy proxy, 
            FacilityModule config,
            GameTimeService timeService) 
            : base(proxy, config, timeService)
        {

            TimeService.HoursPassed += OnHoursPassed;

            _eventBinding = new EventBinding<ProductionOrderAutoCollectedEvent>(OnAutoCollected);
            EventBus<ProductionOrderAutoCollectedEvent>.Register(_eventBinding);

            TryStartNext();
            NormalizeQueue();
            ProcessProduction(false);
            MarkStateChanged();
        }

        /// <summary>
        /// ОБЯЗАТЕЛЬНО вызывать при удалении здания.
        /// Предотвращает утечки подписок.
        /// </summary>
        public override void Dispose()
        {
            TimeService.HoursPassed -= OnHoursPassed;
            EventBus<ProductionOrderAutoCollectedEvent>.Deregister(_eventBinding);
        }

        
        
        
        // =========================================================
        // PUBLIC API
        // =========================================================
        public IReadOnlyList<ProductionJobRuntimeDTO> GetQueueDTO()
        {
            var result = new List<ProductionJobRuntimeDTO>();

            foreach (var p in Proxy.ProductionQueue)
            {
                if (!TryResolveRecipe(p, out var item))
                    continue;

                var jobRuntime = new ProductionJobRuntimeDTO(
                    p.JobId.Value,
                    item.Id,
                    p.TotalHours.Value,
                    p.Amount.Value,
                    p.CurrentStack.Value,
                    p.CompletedStack.Value);

                jobRuntime.RestoreState(p.State.Value, p.StartWorldHour.Value);

                result.Add(jobRuntime);
            }

            return result;
        }

        public bool CanAddJob(RuntimeId recipeId)
        {
            if (Proxy.ProductionQueue.Count < MaxSlots)
                return true;

            return Proxy.ProductionQueue.Any(j =>
                j.RecipeId.Value == recipeId.Guid &&
                j.State.Value == ProductionJobState.Queued &&
                j.CurrentStack.Value < j.MaxStack.Value);
        }
        
        
        public bool TryAddJob(
            RuntimeId recipeId, 
            int durationHours, 
            int orders, 
            int stackLimit, 
            int amountPerOrder)
        {
            // 1. ищем последний слот где есть заказы (с конца)
            ProductionJobProxy targetSlot = null;
            for (int i = Proxy.ProductionQueue.Count - 1; i >= 0; i--)
            {
                var slot = Proxy.ProductionQueue[i];
                if (slot.CurrentStack.Value > 0)
                {
                    if(slot.RecipeId.Value == recipeId.Guid && slot.CurrentStack.Value < slot.MaxStack.Value)
                    {
                        targetSlot = slot;
                        
                        // *** первый слот может быть в статусе Completed
                        // поэтому меняем что бы он снова запустился
                        if (i == 0 && IsCompleted(targetSlot))
                            targetSlot.State.Value = ProductionJobState.Queued;
                    }
                    break;
                }
            }

            // если нет, ищем первый свободный слот
            if (targetSlot == null)
            {
                targetSlot = Proxy.ProductionQueue.FirstOrDefault(j => j.CurrentStack.Value == 0);
            }

            // 2. заполняем найденный слот
            if (targetSlot != null)
            {
                int free = targetSlot.MaxStack.Value - targetSlot.CurrentStack.Value;
                int add = Math.Min(free, orders);
                targetSlot.CurrentStack.Value += add;
                orders -= add;
            }

            // 3. создаем новые слоты, если остались заказы
            while (orders > 0)
            {
                if (Proxy.ProductionQueue.Count >= MaxSlots)
                    return false;

                int toAdd = Math.Min(orders, stackLimit);

                var data = new ProductionJobData
                {
                    JobId = Guid.NewGuid().ToString(),
                    RecipeId = recipeId.Guid,
                    
                    State = (byte)ProductionJobState.Queued,
                    TotalHours = durationHours,
                    StartWorldHour = 0,
                    
                    Amount = amountPerOrder,
                    CurrentStack = toAdd,
                    CompletedStack = 0,
                    MaxStack = stackLimit
                };

                Proxy.AddJob(data);
                orders -= toAdd;
            }

            TryStartNext();
            MarkStateChanged();
            return true;
        }

        public bool CancelJob(string jobId, int ordersToCancel = 1)
        {
            var job = Find(jobId);
            if (job == null)
                return false;

            // уменьшаем стек
            job.CurrentStack.Value = Math.Max(0, job.CurrentStack.Value - ordersToCancel);
            

            // если стек пуст, удаляем слот
            if (job.CurrentStack.Value <= 0)
            {
                bool wasActive = IsInProgress(job);
                Proxy.RemoveJob(job);

                // если был активный, запускаем следующий
                if (wasActive)
                    TryStartNext();
                
                NormalizeQueue();
            }
            
            MarkStateChanged();

            return true;
        }
        
        /// <summary>
        /// Забрать N готовых заказов из слота
        /// </summary>
        public void CollectCompletedOrders(string jobId, int orders)
        {
            var job = Find(jobId);
            if (job == null)
                return;

            int take = Math.Min(orders, job.CompletedStack.Value);
            if (take <= 0)
                return;

            // уменьшаем готовые
            job.CompletedStack.Value = Math.Max(0, job.CompletedStack.Value - take);
            // уменьшаем общее количество заказов
            job.CurrentStack.Value   = Math.Max(0, job.CurrentStack.Value - take);

            // если все заказы забраны и новых нет
            if (job.CompletedStack.Value == 0 && job.CurrentStack.Value == 0)
            {
                Proxy.RemoveJob(job);
                NormalizeQueue();
            }

            MarkStateChanged();
        }
        
        
        
        public void ReduceCompletedAmount(string jobId, int amount)
        {
            var job = Proxy.ProductionQueue.FirstOrDefault(j => j.JobId.Value == jobId);
            if (job == null)
                return;

            if (job.State.Value != ProductionJobState.Completed)
                return;

            job.Amount.Value -= amount;

            if (job.Amount.Value <= 0)
                Proxy.RemoveJob(job);

            MarkStateChanged();
        }
        
        public void SkipActive()
        {
            ProcessProduction(true);
        }

        protected void RemoveCompleted(string jobId)
        {
            var job = Find(jobId);
            if (job == null)
                return;

            if (!IsCompleted(job))
                return;

            Proxy.RemoveJob(job);

            NormalizeQueue();
            MarkStateChanged();
        }

        // =========================================================
        // TIME
        // =========================================================

        private void OnHoursPassed(int hours, TimeAdvanceReason reason)
        {
            if (Proxy.ProductionQueue.Count == 0)
                return;

            ProcessProduction(false);
            MarkStateChanged();
        }

        // =========================================================
        // CORE QUEUE LOGIC
        // =========================================================

        protected void ProcessProduction(bool forceCompleteActive)
        {
            if (Proxy.ProductionQueue.Count == 0)
                return;

            var active = Proxy.ProductionQueue.FirstOrDefault(IsInProgress);
            if (active == null)
                return;

            int duration = active.TotalHours.Value;
            int elapsed = TotalWorldHour - active.StartWorldHour.Value;

            if (!forceCompleteActive && elapsed < duration)
                return;

            int ordersToComplete;

            if (forceCompleteActive)
            {
                ordersToComplete = active.CurrentStack.Value - active.CompletedStack.Value;
            }
            else
            {
                ordersToComplete = elapsed / duration;
            }

            int remainingOrders = active.CurrentStack.Value - active.CompletedStack.Value;

            ordersToComplete = Math.Min(ordersToComplete, remainingOrders);

            if (ordersToComplete <= 0)
                return;
            
            
            if (!TryResolveRecipe(active, out var recipe))
                return;

            for (int i = 0; i < ordersToComplete; i++)
            {
                active.CompletedStack.Value++;

                EventBus<ProductionOrderCompletedEvent>.Raise(
                    new ProductionOrderCompletedEvent()
                    {
                        JobId = active.JobId.Value,
                        RecipeId = recipe.Id,
                        StationId = Proxy.UniqueId,
                        Orders = 1,
                        Amount = active.Amount.Value
                    });
            }

            bool slotWillComplete =
                active.CompletedStack.Value >= active.CurrentStack.Value;

            if (slotWillComplete)
            {
                MarkCompleted(active);
            }
            else
            {
                active.StartWorldHour.Value += ordersToComplete * duration;
            }

            if (!Proxy.ProductionQueue.Any(IsInProgress))
                TryStartNext();

            NormalizeQueue();
            MarkStateChanged();
        }
        
        private void OnAutoCollected(ProductionOrderAutoCollectedEvent e)
        {
            if (e.StationId != Proxy.UniqueId)
                return;

            var job = Find(e.JobId);

            if (job == null)
                return;

            CollectCompletedOrders(job.JobId.Value, e.Orders);
        }

        protected void TryStartNext()
        {
            if (Proxy.ProductionQueue.Any(IsInProgress))
                return;

            var next = Proxy.ProductionQueue.FirstOrDefault(IsQueued);
            if (next != null)
                StartJob(next, TotalWorldHour);
        }

        /// <summary>
        /// Приводит очередь к строгому порядку:
        /// 1. Один InProgress (если есть)
        /// 2. Все Queued
        /// 3. Все Completed
        /// Не изменяет состояние слотов — только порядок.
        /// </summary>
        protected void NormalizeQueue()
        {
            // if (Proxy.ProductionQueue.Count <= 1)
            //     return;
            //
            // var ordered = Proxy.ProductionQueue
            //     .OrderByDescending(IsInProgress)
            //     .ThenBy(j => IsQueued(j) ? 0 : 1)
            //     .ToList();
            //
            // if (!Proxy.ProductionQueue.SequenceEqual(ordered))
            //     Proxy.Reorder(ordered);
        }

        // =========================================================
        // PROTECTED HELPERS
        // =========================================================

        protected bool TryResolveRecipe(ProductionJobProxy job, out ItemConfig item)
            => GameContent.ResolveItem(job.RecipeId.Value, out item);

        protected ProductionJobProxy Find(string id)
            => Proxy.ProductionQueue.FirstOrDefault(j => j.JobId.Value == id);

        protected bool IsInProgress(ProductionJobProxy job)
            => job.State.Value == ProductionJobState.InProgress;

        protected bool IsQueued(ProductionJobProxy job)
            => job.State.Value == ProductionJobState.Queued;

        protected bool IsCompleted(ProductionJobProxy job)
            => job.State.Value == ProductionJobState.Completed;

        protected int GetFinishHour(ProductionJobProxy job)
            => job.StartWorldHour.Value + job.TotalHours.Value;

        protected void StartJob(ProductionJobProxy job, int startHour)
        {
            job.StartWorldHour.Value = startHour;
            job.State.Value = ProductionJobState.InProgress;
        }

        protected void MarkCompleted(ProductionJobProxy job)
        {
            job.State.Value = ProductionJobState.Completed;
        }

        protected void ReorderAfterCompletion(ProductionJobProxy completed)
        {
            var reordered = Proxy.ProductionQueue
                .Skip(1)
                .Concat(new[] { completed })
                .ToList();

            Proxy.Reorder(reordered);
        }

    }
}