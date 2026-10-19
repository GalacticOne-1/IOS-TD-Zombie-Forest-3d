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
    /// Правила:
    /// • крафтит ТОЛЬКО слот 0, остальные слоты — очередь
    /// • если слот 0 Completed (ждёт сбора) — станок стоит
    /// • после освобождения слота 0 очередь сдвигается и новый слот 0 стартует
    /// • время почасовое: за один вызов может пройти много часов,
    ///   поэтому обработка идёт циклом по заказам с реальным часом готовности
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

        // час, на который "перемотано" моделирование (null = реальное время)
        private int? _simHour;

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

            // досчитываем прогресс, пока игра была выключена
            // (ProcessProduction сам вызывает TryStartNext)
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

        /// <summary>
        /// Должен повторять логику выбора слота в TryAddJob.
        /// </summary>
        public bool CanAddJob(RuntimeId recipeId)
        {
            var q = Proxy.ProductionQueue;

            if (q.Count < MaxSlots)
                return true;

            // очередь полна: подходит только дозаказ в последний слот с заказами
            var last = FindLastOccupiedSlot();
            return last != null && CanStackInto(last, recipeId);
        }

        public bool TryAddJob(
            RuntimeId recipeId,
            int durationHours,
            int orders,
            int stackLimit,
            int amountPerOrder)
        {
            if (orders <= 0 || stackLimit <= 0)
                return false;

            // 1. ищем слот для дозаказа: последний слот с заказами,
            //    если это тот же рецепт и в нём есть место
            ProductionJobProxy targetSlot = null;
            var last = FindLastOccupiedSlot();
            if (last != null && CanStackInto(last, recipeId))
                targetSlot = last;

            // 2. ПРОВЕРКА ДО ИЗМЕНЕНИЙ (атомарность):
            //    считаем, сколько заказов уйдёт в существующий слот
            //    и сколько новых слотов понадобится
            int remaining = orders;
            if (targetSlot != null)
            {
                int free = targetSlot.MaxStack.Value - targetSlot.CurrentStack.Value;
                remaining -= Math.Min(free, remaining);
            }

            int newSlotsNeeded = remaining > 0
                ? (remaining + stackLimit - 1) / stackLimit
                : 0;

            if (Proxy.ProductionQueue.Count + newSlotsNeeded > MaxSlots)
                return false;

            // 3. применяем изменения
            if (targetSlot != null)
            {
                // первый слот может быть Completed — перезапускаем его
                if (ReferenceEquals(targetSlot, Proxy.ProductionQueue[0]) && IsCompleted(targetSlot))
                    targetSlot.State.Value = ProductionJobState.Queued;

                int free = targetSlot.MaxStack.Value - targetSlot.CurrentStack.Value;
                int add = Math.Min(free, orders);
                targetSlot.CurrentStack.Value += add;
                orders -= add;
            }

            while (orders > 0)
            {
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

        /// <summary>
        /// Отмена НЕПРОИЗВЕДЁННЫХ заказов слота. Готовые отменить нельзя — их только забирают.
        /// </summary>
        public bool CancelJob(string jobId, int ordersToCancel = 1)
        {
            var job = Find(jobId);
            if (job == null)
                return false;

            int cancellable = job.CurrentStack.Value - job.CompletedStack.Value;
            ordersToCancel = Math.Min(ordersToCancel, cancellable);
            if (ordersToCancel <= 0)
                return false;

            job.CurrentStack.Value -= ordersToCancel;

            if (job.CurrentStack.Value <= 0)
            {
                Proxy.RemoveJob(job);
            }
            else if (IsInProgress(job) &&
                     job.CompletedStack.Value >= job.CurrentStack.Value)
            {
                // все оставшиеся заказы уже готовы -> станок ждёт сбора
                MarkCompleted(job);
            }

            TryStartNext();
            NormalizeQueue();
            MarkStateChanged();
            return true;
        }

        /// <summary>
        /// Забрать N готовых заказов из слота.
        /// </summary>
        public void CollectCompletedOrders(string jobId, int orders)
        {
            var job = Find(jobId);
            if (job == null)
                return;

            int take = Math.Min(orders, job.CompletedStack.Value);
            if (take <= 0)
                return;

            job.CompletedStack.Value = Math.Max(0, job.CompletedStack.Value - take);
            job.CurrentStack.Value = Math.Max(0, job.CurrentStack.Value - take);

            // все заказы забраны и новых нет -> слот освобождается, очередь сдвигается
            if (job.CompletedStack.Value == 0 && job.CurrentStack.Value == 0)
            {
                Proxy.RemoveJob(job);
                TryStartNext();
                NormalizeQueue();
            }

            MarkStateChanged();
        }

        public void ReduceCompletedAmount(string jobId, int amount)
        {
            var job = Find(jobId);
            if (job == null)
                return;

            if (!IsCompleted(job))
                return;

            job.Amount.Value -= amount;

            if (job.Amount.Value <= 0)
            {
                Proxy.RemoveJob(job);
                TryStartNext();
            }

            MarkStateChanged();
        }

        /// <summary>
        /// Мгновенно завершает весь активный слот (слот 0).
        /// </summary>
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
            TryStartNext();

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

        /// <summary>
        /// Обрабатывает производство по одному заказу за итерацию.
        /// Каждый заказ фиксируется с часом реальной готовности, поэтому
        /// если прошло много часов, а автосбор освобождает слоты, следующие
        /// слоты тоже успевают произвестись.
        /// </summary>
        protected void ProcessProduction(bool forceCompleteActive)
        {
            int now = TotalWorldHour;
            bool force = forceCompleteActive;

            // guard защищает от бесконечного цикла при кривых данных
            for (int guard = 0; guard < 10000; guard++)
            {
                if (Proxy.ProductionQueue.Count == 0)
                    break;

                TryStartNext();

                var active = Proxy.ProductionQueue[0];
                if (!IsInProgress(active))
                    break; // слот 0 Completed -> станок стоит

                int duration = active.TotalHours.Value;
                if (duration <= 0)
                    break;

                if (active.CompletedStack.Value >= active.CurrentStack.Value)
                {
                    MarkCompleted(active);
                    break;
                }

                int finishHour = active.StartWorldHour.Value + duration;
                if (!force && finishHour > now)
                    break; // следующий заказ ещё не готов

                if (!TryResolveRecipe(active, out var recipe))
                    break;

                // час, когда этот заказ реально завершился
                int completionHour = force ? now : finishHour;

                // 1. полностью обновляем состояние ДО события
                active.CompletedStack.Value++;
                active.StartWorldHour.Value = completionHour; // следующий заказ стека начнётся отсюда

                bool slotDone = active.CompletedStack.Value >= active.CurrentStack.Value;
                if (slotDone)
                    MarkCompleted(active);

                // 2. данные события берём заранее: автосбор может удалить active из очереди
                var evt = new ProductionOrderCompletedEvent
                {
                    JobId = active.JobId.Value,
                    RecipeId = recipe.Id,
                    StationId = Proxy.UniqueId,
                    Orders = 1,
                    Amount = active.Amount.Value
                };

                // 3. событие. Если сработает автосбор, слот освободится,
                //    а TryStartNext() возьмёт _simHour как час старта следующего слота
                _simHour = completionHour;
                try
                {
                    EventBus<ProductionOrderCompletedEvent>.Raise(evt);
                }
                finally
                {
                    _simHour = null;
                }

                // skip завершает только текущий слот
                if (force && slotDone)
                    break;
            }

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

        /// <summary>
        /// Запускает слот 0, если он ждёт в очереди. Другие слоты никогда не стартуют.
        /// </summary>
        protected void TryStartNext()
        {
            if (Proxy.ProductionQueue.Count == 0)
                return;

            var first = Proxy.ProductionQueue[0];
            if (IsQueued(first))
                StartJob(first, _simHour ?? TotalWorldHour);
        }

        /// <summary>
        /// Оставлен для совместимости с наследниками. Сдвиг очереди происходит
        /// сам при удалении слота из списка (Proxy.RemoveJob).
        /// </summary>
        protected void NormalizeQueue()
        {
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

        // =========================================================
        // PRIVATE HELPERS
        // =========================================================

        private ProductionJobProxy FindLastOccupiedSlot()
        {
            var q = Proxy.ProductionQueue;
            for (int i = q.Count - 1; i >= 0; i--)
            {
                if (q[i].CurrentStack.Value > 0)
                    return q[i];
            }

            return null;
        }

        private static bool CanStackInto(ProductionJobProxy slot, RuntimeId recipeId)
            => slot.RecipeId.Value == recipeId.Guid &&
               slot.CurrentStack.Value < slot.MaxStack.Value;
    }
}