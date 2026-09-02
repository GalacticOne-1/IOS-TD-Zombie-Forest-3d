using System;
using Galactic1.Code.Gameplay.Combat.Cover;
using Galactic1.Code.Gameplay.Equipment;
using Galactic1.Code.Gameplay.Units;
using Galactic1.Code.Systems.Raid;
using Galactic1.Code.UI.Units.Presentation;

namespace Galactic1.Code.Systems.Runtime
{
    public sealed class SceneTransportAdapter : ISceneUnit
    {
        private readonly ITransportRuntime _runtime;

        public IUnitRuntime Runtime { get; }
        
        /// <summary>
        /// Общий runtime — используется UnitInstance.UpdateM() для тика.
        /// Кастуется к IUnitRuntimeBase: RaidUnitRuntime реализует его.
        /// </summary>
        public IUnitRuntimeBase RuntimeBase => Runtime;

        public string Id => _runtime.Id;

        public IUnitStatsScene Stats { get; }
        public UnitCoverState Cover { get; }

        public IEquipmentStatsProvider EquipmentStatsProvider { get; }
        //public IReadOnlyInventoryView Inventory { get; }
        //public IEquipmentPresentation Equipment => _runtime.EquipmentService as IEquipmentPresentation;

        public event Action OnDeath;

        public SceneTransportAdapter(ITransportRuntime runtime)
        {
            _runtime = runtime;
            

            //Stats = new UnitStatsSceneAdapter(runtime.Stats);
            //Inventory = new UnitInventoryPresentationAdapter(runtime.InventorySource);

            // death подписка
            // runtime.Stats.OnStatChanged += (evt, _) =>
            // {
            //     if (evt.Type == StatId.Health && evt.Current <= 0)
            //         OnDeath?.Invoke();
            // };
        }
        
        public void Dispose()
        {
            // TODO 
            // отписка от рантайм 
            // runtime.Stats.OnStatChanged += ...
        }
    }
}