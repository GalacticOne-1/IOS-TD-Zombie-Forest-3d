
using Galactic1.RaidLoot.Events;
using Galactic1.RaidLoot.Services;

namespace Galactic1.Code.Systems.Progression
{
    /// <summary>
    /// Gameplay Event → XP.
    ///
    /// Subscribes to gameplay-fact events published by other systems and
    /// resolves the XP reward via ProgressionXPDefinition, then hands the
    /// amount to ProgressionService. Never calculates gameplay logic, never
    /// touches Level directly.
    ///
    /// Bindings are registered once for the lifetime of the app (progression
    /// is global, not scene-scoped) — same lifetime as the WorldMapScene/
    /// LocationScene event bindings in CoreRegistrations.
    /// </summary>
    public sealed class ProgressionXPService
    {
        private readonly ProgressionService _progressionService;
        private readonly ProgressionXPDefinition _xpDefinition;

        private readonly EventBinding<EnemyKilledEvent> _enemyKilledBinding;
        private readonly EventBinding<ContainerOpenedEvent> _containerOpenedBinding;
        private readonly EventBinding<LocationDiscoveredEvent> _locationDiscoveredBinding;
        private readonly EventBinding<FacilityBuiltEvent> _facilityBuiltBinding;

        public ProgressionXPService(ProgressionService progressionService, ProgressionXPDefinition xpDefinition)
        {
            _progressionService = progressionService;
            _xpDefinition = xpDefinition;

            _enemyKilledBinding = new EventBinding<EnemyKilledEvent>(OnEnemyKilled);
            EventBus<EnemyKilledEvent>.Register(_enemyKilledBinding);

            _containerOpenedBinding = new EventBinding<ContainerOpenedEvent>(OnContainerOpened);
            EventBus<ContainerOpenedEvent>.Register(_containerOpenedBinding);

            _locationDiscoveredBinding = new EventBinding<LocationDiscoveredEvent>(OnLocationDiscovered);
            EventBus<LocationDiscoveredEvent>.Register(_locationDiscoveredBinding);

            _facilityBuiltBinding = new EventBinding<FacilityBuiltEvent>(OnFacilityBuilt);
            EventBus<FacilityBuiltEvent>.Register(_facilityBuiltBinding);
        }

        private void OnEnemyKilled(EnemyKilledEvent e)
        {
            int xp = _xpDefinition.GetZombieKillXP(e.Runtime.EnemyId);
            _progressionService.AddExperience(xp);
        }

        private void OnContainerOpened(ContainerOpenedEvent e)
        {
            int xp = _xpDefinition.GetLootXP(e.ContainerTagId);
            _progressionService.AddExperience(xp);
        }

        private void OnLocationDiscovered(LocationDiscoveredEvent e)
        {
            int xp = _xpDefinition.GetLocationXP(e.LocationId);
            _progressionService.AddExperience(xp);
        }

        private void OnFacilityBuilt(FacilityBuiltEvent e)
        {
            int xp = _xpDefinition.GetFacilityXP(e.FacilityItemId);
            _progressionService.AddExperience(xp);
        }
    }
}
