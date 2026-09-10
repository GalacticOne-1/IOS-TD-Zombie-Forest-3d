using Galactic1.Game.Meta.Items;

namespace Galactic1.Code.Gameplay.Tasks
{
    /// <summary>
    /// Read-only снэпшот одной награды задачи — для отображения в ScenarioTaskRewardView.
    /// НЕ отвечает за выдачу: источник задачи (Tutorial и т.п.) сам решает, когда и через
    /// какой сервис (например InboxService) награда реально вручается — см. IScenarioTaskService
    /// class docstring, раздел "Completion and reward are separate concerns".
    /// Хранение ItemConfig — уже установленный в проекте паттерн runtime-представления
    /// предмета (см. InventorySlotRuntime.Item).
    /// </summary>
    public readonly struct ScenarioTaskReward
    {
        public readonly ItemConfig Item;
        public readonly int Amount;
        public readonly int Durability;
        public readonly int AmmoInMagazine;

        public ScenarioTaskReward(ItemConfig item, int amount, int durability, int ammoInMagazine)
        {
            Item = item;
            Amount = amount;
            Durability = durability;
            AmmoInMagazine = ammoInMagazine;
        }
    }
}