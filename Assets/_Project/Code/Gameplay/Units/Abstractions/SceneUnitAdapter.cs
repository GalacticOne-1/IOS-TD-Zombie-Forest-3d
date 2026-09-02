
using System;
using Galactic1.Code.Gameplay.Combat.Cover;
using Galactic1.Code.Gameplay.Equipment;
using Galactic1.Code.Gameplay.Units;
using Galactic1.Code.Systems.Raid;
using Galactic1.Code.UI.Units.Presentation;

namespace Galactic1.Code.Systems.Runtime
{
    public sealed class SceneUnitAdapter : ISceneUnit
    {
        private readonly IUnitRuntime _runtime;

        public IUnitRuntime Runtime => _runtime;  // todo remove !!
        
        /// <summary>
        /// Общий runtime — используется UnitInstance.UpdateM() для тика.
        /// Кастуется к IUnitRuntimeBase: RaidUnitRuntime реализует его.
        /// </summary>
        public IUnitRuntimeBase RuntimeBase => _runtime;

        public string Id => _runtime.Id;

        public IUnitStatsScene Stats { get; }

        public IEquipmentStatsProvider EquipmentStatsProvider { get; }
        //public IReadOnlyInventoryView Inventory { get; }
        //public IEquipmentPresentation Equipment => _runtime.EquipmentService as IEquipmentPresentation;
        
        private UnitCoverState _cover;
 
        // <<< NEW — implements IUnitSceneContext.Cover
        public UnitCoverState Cover => _cover;

        public event Action OnDeath; 

        public SceneUnitAdapter(IUnitRuntime runtime)
        {
            _runtime = runtime;
            EquipmentStatsProvider = _runtime.EquipmentService;

            Stats = new UnitStatsSceneAdapter(runtime.Stats);
            //Inventory = new UnitInventoryPresentationAdapter(runtime.InventorySource);
            
            _cover = new UnitCoverState
            {
                CoverType = CoverType.None,
                CoverDirection = UnityEngine.Vector3.zero
            };

            // ✅ ПРАВИЛЬНО: прокидываем через метод
            Stats.OnDeath += HandleDeath;
        }
        
        public void SetCover(UnitCoverState cover)
        {
            _cover = cover;
        }

        private void HandleDeath() => OnDeath?.Invoke();

        public void Dispose()
        {
            if (Stats != null)
            {
                Stats.OnDeath -= HandleDeath;
                Stats.Dispose();
            }
        }
    }
}