using System.Collections.Generic;
using Galactic1.Core.Enums;
using Galactic1.Game.Meta.Items;
using UnityEngine;
using Galactic1.Gameplay.Player.StateMachine;
using Galactic1.Items;
using Galactic1.Localisation;
using Galactic1.Systems.Inventory;
using Gameplay.Gameplay.Interaction;

namespace Galactic1.Gameplay.Interaction.Objects
{
    /// <summary>
    /// Базовый класс добываемого ресурса (дерево/руда). 
    /// Работает через вашу ActionJob-систему: 
    ///   - реализует ILongInteractable (даёт ActionController длительность и отменяемость)
    ///   - запускается через PlayerActionController.StartActionForInteractable
    ///   - прогресс/отмена/получение урона уже в PlayerActionController + StateMachine
    /// </summary>
    public abstract class ResourceNodeBase : InteractableBase, ILongInteractable, IToolRequirement, ILoot
    {
        [field:Header("Resource BasicSettings")]
        [field:Tooltip("Добавить конфиг вручную")]
        [field: SerializeField] public ItemConfig DropItem { get; private set; }
        [field: SerializeField] public int DropAmount { get; private set; }
        
        
        [Space(20)]
        [SerializeField]
        protected int resourceHP = 3;

        [Tooltip("Сколько времени нужно на добычу одного хита.")] [SerializeField]
        protected float gatherDuration = 0.8f;

        [field: Tooltip("Инструмент обязателен?")]
        [field: SerializeField] public List<ItemEquipType> RequiredToolType { get; private set; }


        public bool RequiresProgressBar => false;


        // ILongInteractable
        public float RequiredTime => gatherDuration;
        
        public bool IsFinished { get; private set; } = false;




        public override bool CanInteract(Transform interactor, bool isDragon)
        {
            if (!base.CanInteract(interactor,isDragon))
                return false;
            if (resourceHP <= 0)
                return false;

            return true;
        }

        /// <summary>
        /// Тут мы НЕ начинаем добычу.
        /// PlayerActionController сам вызовет Interact() после завершения ActionJob.
        /// Для ресурсов Interact() = "нанести добывающий удар".
        /// </summary>
        public override void Interact(Transform interactor)
        {
            ApplyHit(1);
            OnGatherHit(interactor);
        }

        protected virtual void OnGatherHit(Transform interactor)
        {
            // TODO: звук, анимации, эффекты
        }

        /// <summary>
        /// Вызывается каждый удар добычи (каждый ActionJob Finish).
        /// </summary>
        public virtual void ApplyHit(int amount)
        {
            resourceHP -= amount;

            if (resourceHP <= 0)
            {
                var result = ServiceLocator.Current.Get<InventoryRepository>().PlayerInventory.Inventory.TryAdd(DropItem, DropAmount);
                
                // пускаем лог добвленного кол-ва
                if (result.Added > 0)
                    EventBus<ItemPickedEvent>.Raise(new ItemPickedEvent(DropItem, result.Added, Tr.position));

                // пускаем лог если что-то не вошло в инвентарь
                if (result.Remaining > 0)
                {
                    var notSpace= ServiceLocator.Current.Get<LocalisationService>().Data.not_space;
                    EventBus<RequirementFailedEvent>
                        .Raise(new RequirementFailedEvent($"{notSpace} {DropItem.Header.titleLid} ({result.Remaining})",
                            Tr.position));
                }
                
                IsFinished = true;
                OnDepleted();
            }
        }

        protected virtual void OnDepleted()
        {
            // TODO: дроп + FX
            gameObject.SetActive(false);
        }

    }
}
