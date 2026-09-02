using UnityEngine;
using Galactic1.Gameplay.Interaction;
using System.Collections;
using Galactic1.Code.GameDatabase;
using Galactic1.Configs;
using Galactic1.Core.Enums;
using Galactic1.Gameplay.UI;
using Galactic1.Items;
using Galactic1.Localisation;
using Galactic1.Repository;
using Galactic1.Code.UI.Inventory;
using Galactic1.Game.Meta.Items;
using Galactic1.UI.Core;
using Gameplay.Gameplay.Interaction;
using Gameplay.Inventory;

namespace Galactic1.Gameplay.Player.StateMachine
{
    /// <summary>
    /// Контроллер действий игрока: запускает ActionJob, управляет прогресс-UI и интегрируется с WorldInputDispatcher.
    /// Он является компонентом на игроке и используется PlayerStateMachine.
    /// </summary>
    [RequireComponent(typeof(PlayerStateMachine))]
    public class PlayerActionController : MonoBehaviour
    {
        private PlayerStateMachine _machine;
        private ActionTimeBarUI _actionTimeBarUI;
        private PlayerInventoryContainer _inventoryContainer;
        private EquipmentContainer_old _equipmentContainer;
        private ToolUserInventoryAdapter _toolAdapter;
        
        public ActionJob CurrentJob { get; private set; }

        private Coroutine _jobCoroutine;
        
        private bool gatherRepeating;
        private int? indexSlotTool;
        private bool toolInEquipment;
        

        private void Awake()
        {
            _machine = GetComponent<PlayerStateMachine>();

            // подключаем инвентари игрока
            _inventoryContainer = GetComponent<PlayerInventoryContainer>();
            _equipmentContainer = GetComponent<PlayerEquipmentContainer>();
            _toolAdapter = new ToolUserInventoryAdapter(_inventoryContainer, _equipmentContainer);

            
            // loading UI timer bar
            _actionTimeBarUI = "Prefabs/UI/Gameplay/ActionTimerBar"
                .CreateGO(ServiceLocator.Current.Get<UIManager>().TransformRoot.hudRoot)
                .GetComponent<ActionTimeBarUI>();
            
            EventBus<SceneClearEvent>.Register(new EventBinding<SceneClearEvent>(() =>
            {
                _actionTimeBarUI?.gameObject.DestroyGO();
            }));
            
        }

        /// <summary>
        /// Запустить Job для интеракта (сундук/труп/ящик). Решение о типе иконки/прогресса UI берёт UI-код.
        /// </summary>
        public void StartActionForInteractable(IInteractable interactable, Transform interactor)
        {
            if (interactable == null) return;
            
            // Если уже есть активный Job и это тот же интеракт — игнорируем повторное нажатие
            if (CurrentJob != null && interactable.Equals(CurrentJob.TargetInteractable))
                return;

            var typed = interactable as IActionInteractable;
            var type = typed?.ActionType ?? ActionType.None;

            switch (type)
            {
                case ActionType.OpenContainer:
                    StartOpenContainer(interactable, interactor);
                    return;

                case ActionType.LootPickup:
                    StartLootPickup(interactable, interactor, "isPickup");
                    return;

                case ActionType.GatherTree:
                    StartGather(interactable, interactor, "isMiningWood");
                    return;

                case ActionType.GatherOre:
                    StartGather(interactable, interactor, "isMiningOre");
                    return;

                default:
                    // fallback — старое универсальное поведение
                    //StartGenericAction(interactable, interactor);
                    return;
            }
        }
        
        public void OnActionButtonUp()
        {
            gatherRepeating = false;
        }
        
        // Быстрый доступ к инструменту
        ItemConfig GetTool(ItemEquipType itemEquipType)
        {
            return GameContent.Items.GetItemTool(itemEquipType);
        }



        private void StartOpenContainer(IInteractable interactable, Transform interactor)
        {
            var longInteract = interactable as ILongInteractable;
            if (longInteract == null || longInteract.IsFinished)
            {
                interactable.Interact(interactor);
                return;
            }

            var job = new ActionJob("OpenContainer", longInteract.RequiredTime, true, interactable);
            
            job.OnStarted += () =>
            {
                // старт анимации
                ServiceLocator.Current.Get<PlayerRepository>().GetController.Animation.animator.SetTrigger("isAction");
                ServiceLocator.Current.Get<PlayerRepository>().GetController.Animation.animator.SetBool("isOpening", true);
                
                _equipmentContainer.SwitchVisual(EquipSlotType.Weapon);
                // shield ??
            };
            
            job.OnCancelled += () =>
            {
                ServiceLocator.Current.Get<PlayerRepository>().GetController.Animation.animator.SetBool("isOpening", false);
                _equipmentContainer.SwitchVisual(EquipSlotType.Weapon);
            };

            job.OnFinished += () =>
            {
                ServiceLocator.Current.Get<PlayerRepository>().GetController.Animation.animator.SetBool("isOpening", false);
                interactable.Interact(interactor);
                _equipmentContainer.SwitchVisual(EquipSlotType.Weapon);
            };


            if (longInteract.RequiresProgressBar)
                _actionTimeBarUI.StartAction(interactable.IObjectContext.PivotCenter(), job);
            
            StartJob(job);
        }
        
        private void StartLootPickup(IInteractable interactable, Transform interactor, string animTrigger)
        {
            // пропускаем только предметы для инвентаря
            if(interactable is not ILoot loot) return;
            
            // 🔥 Проверка на наличие свободного места
            if (!HasInventorySpaceFor(loot))
            {
                EventBus<RequirementFailedEvent>.Raise(new RequirementFailedEvent(
                    ServiceLocator.Current.Get<LocalisationService>().Data.inventory_full, Vector3.one));
                return;
            }
            
            
            var job = new ActionJob("Gather", 1, false);
            
            job.OnStarted += () =>
            {
                // анимация
                ServiceLocator.Current.Get<PlayerRepository>().GetController.Animation.animator.SetTrigger(animTrigger);
                _equipmentContainer.SwitchVisual(EquipSlotType.Weapon);
            };
            
            job.OnFinished += () =>
            {
                _equipmentContainer.SwitchVisual(EquipSlotType.Weapon);
                _machine.ClearState();
                
                // наносим "урон ресурсу"
                interactable.Interact(interactor);
            };

            
            StartJob(job, true);
        }

        
        private void StartGather(IInteractable interactable, Transform interactor, string animTrigger)
        {
            // пропускаем только предметы для инвентаря
            if(interactable is not ILoot loot) return;
            
            if (interactable is not ILongInteractable longInteract)
            {
                interactable.Interact(interactor);
                return;
            }
            
            // 🔥 Проверка на наличие свободного места
            if (!HasInventorySpaceFor(loot))
            {
                EventBus<RequirementFailedEvent>.Raise(new RequirementFailedEvent(
                    ServiceLocator.Current.Get<LocalisationService>().Data.inventory_full, Vector3.one));
                return;
            }
            
            // 🔥 Проверка инструмента
            if (interactable is IToolRequirement toolRequirement)
            {
                (indexSlotTool, toolInEquipment) = _toolAdapter.GetBestToolFor(toolRequirement);
                if (!indexSlotTool.HasValue)
                {
                    EventBus<RequirementFailedEvent>.Raise(new RequirementFailedEvent(
                        $"{GetTool(toolRequirement.RequiredToolType[0]).Header.titleLid} required", Vector3.one));
                    return;
                }
                _toolAdapter.BindVisual(
                    toolInEquipment ? _equipmentContainer.Inventory : _inventoryContainer.Inventory,
                    indexSlotTool.Value);
            }

            gatherRepeating = true; // при удержании повторяем
            
            RunGatherCycle(interactable, interactor, longInteract, animTrigger);
        }

        private void RunGatherCycle(
            IInteractable interactable,
            Transform interactor,
            ILongInteractable longInteract,
            string animTrigger)
        {
            var job = new ActionJob("Gather", longInteract.RequiredTime, false);
            
            job.OnStarted += () =>
            {
                // анимация
                ServiceLocator.Current.Get<PlayerRepository>().GetController.Animation.animator.SetTrigger(animTrigger);
            };

            job.OnFinished += () =>
            {
                _machine.ClearState();
                
                // наносим "урон ресурсу"
                interactable.Interact(interactor);
                
                // #1 если ресурс не собран и кнопка удерживается
                var longInteract = interactable as ILongInteractable;
                var newCycle = !longInteract.IsFinished && gatherRepeating;
                
                // #2 Проверяем наличие места в инвентаре
                if (newCycle)
                {
                    var available = HasInventorySpaceFor(interactable as ILoot);

                    if (!available)
                    {
                        DLog.Alert("Нет места в инвентаре — цикл добычи остановлен.");
                        newCycle = false;     // блокируем повтор
                    }
                }

                
                // #3 🔥 Наносим урон инструменту
                var requiresTool = false;
                
                if (interactable is IToolRequirement toolRequirement && indexSlotTool.HasValue)
                {
                    var inventory = toolInEquipment ? _equipmentContainer.Inventory : _inventoryContainer.Inventory;
                    
                    // Наносим урон инструменту
                    requiresTool = _toolAdapter.OnItemUsed(inventory, indexSlotTool.Value);
                    
                    if(requiresTool)
                    {
                        EventBus<RequirementFailedEvent>.Raise(new RequirementFailedEvent(
                            ServiceLocator.Current.Get<LocalisationService>().Data.tool_broken, Vector3.one));
                    }

                    // Если инструмент сломан — ищем новый
                    if (newCycle && requiresTool)
                    {
                        (indexSlotTool, toolInEquipment) = _toolAdapter.GetBestToolFor(toolRequirement);
                        if (indexSlotTool.HasValue)
                        {
                            requiresTool = false;
                            inventory = toolInEquipment ? _equipmentContainer.Inventory : _inventoryContainer.Inventory;
                            _toolAdapter.BindVisual(inventory, indexSlotTool.Value);
                            DLog.Alert("Инструмент сломан. Авто-замена на новый.");
                        }
                        // нового тулза нет, возвращаем предмет из слота
                        else
                        {
                            _toolAdapter.ClearVisual();
                            EventBus<RequirementFailedEvent>.Raise(new RequirementFailedEvent(
                                $"{GetTool(toolRequirement.RequiredToolType[0]).Header.titleLid} required", Vector3.one));
                        }
                    }
                    // нового цикла нет, возвращаем предмет из слота
                    else if (!newCycle)
                    {
                        _toolAdapter.ClearVisual();
                    }
                }

                
                if (newCycle && !requiresTool)
                {
                    RunGatherCycle(interactable, interactor, longInteract, animTrigger);
                    return;
                }

                // если дошли сюда → добыча завершена
                gatherRepeating = false;
            };

            if (longInteract.RequiresProgressBar)
                _actionTimeBarUI.StartAction(interactable.IObjectContext.PivotCenter(), job);
            
            StartJob(job, true);
        }
        
        /// <summary>
        /// Проверяет, сможет ли инвентарь принять хотя бы единицу ресурса,
        /// которую выдаёт ILongInteractable.Interact()
        /// </summary>
        private bool HasInventorySpaceFor(ILoot loot)
        {
            var item = loot.DropItem;
            if (item == null) return true;

            return _inventoryContainer.Inventory.HasFreeSpaceFor(item).HasValue;
        }



        /// <summary>
        /// Начинаем атаку (если цель поддерживает IAttackable).
        /// </summary>
        public void StartAttack(ITargetable targetable, Transform attacker)
        {
            if (targetable == null) return;

            // пример: атака длится 0.6s
            var job = new ActionJob("Attack", 0.6f, false);
            job.OnStarted += () =>
            {
                // проиграть анимацию атаки
            };
            job.OnFinished += () =>
            {
                targetable.ReceiveAttack(attacker);
            };
            StartJob(job);
        }

        private void StartJob(ActionJob job, bool forcedStop = false)
        {
            // если уже есть текущий джоб — попытаться отменить или ждать
            if (CurrentJob != null)
            {
                // отменяем предыдущий, если он отменяем
                if (CurrentJob.CanBeInterrupted || forcedStop)
                    CurrentJob.Cancel("New job started");
                else
                    return; // нельзя прервать -> игнорируем новый
            }

            CurrentJob = job;
            CurrentJob.Start();

            // переключаем состояние машины в Interact/Attack в зависимости от job name
            if (job.Name.StartsWith("Attack"))
                _machine.ChangeState(_machine.GetAttackState());
            
            else if (job.Name.StartsWith("Gather"))
                _machine.ChangeState(_machine.GetGatheringState());
            
            else
                _machine.ChangeState(_machine.GetInteractState());
            
            // запускаем корутину тикера
            _jobCoroutine = StartCoroutine(JobTickCoroutine(job));
        }

        private IEnumerator JobTickCoroutine(ActionJob job)
        {
            while (!job.IsCompleted && !job.IsCancelled)
            {
                job.Tick(Time.deltaTime);
                yield return null;
            }

            // завершение/отмена
            // дождёмся, пока состояние обработает завершение
            yield return null;
        }

        public void FinishCurrentJob()
        {
            if (_jobCoroutine != null)
            {
                StopCoroutine(_jobCoroutine);
                _jobCoroutine = null;
            }
            DLog.Alert("FinishCurrentJob", EDlogColor.ORANGE);
            CurrentJob = null;
        }

        public void CancelCurrentJob()
        {
            if (CurrentJob != null)
            {
                CurrentJob.Cancel("Cancelled by controller");
            }

            if (_jobCoroutine != null)
            {
                StopCoroutine(_jobCoroutine);
                _jobCoroutine = null;
            }

            CurrentJob = null;
            _actionTimeBarUI.CancelAction();
        }



        
    }
}
