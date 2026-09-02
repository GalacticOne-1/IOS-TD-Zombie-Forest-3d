using Galactic1.Game.Meta.Items;
using Galactic1.Localisation;
using Galactic1.Systems.Inventory;
using UnityEngine;

namespace Galactic1.Gameplay.Interaction.Objects
{
    public class LootItemInteractable : InteractableBase, IActionInteractable, ILoot
    {
        [field: Header("Resource BasicSettings")]
        [field: Tooltip("Добавить конфиг вручную")]
        [field: SerializeField] public ItemConfig DropItem { get; private set; }
        [field: SerializeField] public int DropAmount { get; private set; }
        
        public override ActionType ActionType => ActionType.LootPickup;

        
        public override void Interact(Transform interactor)
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
                
            OnDepleted();
        }
        
        protected virtual void OnDepleted()
        {
            // TODO: дроп + FX
            gameObject.SetActive(false);
        }
    }

}