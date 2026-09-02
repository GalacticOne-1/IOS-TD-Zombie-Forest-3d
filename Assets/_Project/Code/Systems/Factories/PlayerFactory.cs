using System.Collections.Generic;
using Galactic1.AbstractFactory;
using Galactic1.Code.Gameplay.Animation;
using Galactic1.Code.Gameplay.Survivors.Repositories;
using Galactic1.Code.Gameplay.Units;
using Galactic1.Code.Gameplay.Units.Repositories;
using Galactic1.Code.Systems.Raid;
using Galactic1.Core.Enums;
using Galactic1.Gameplay.Player;
using UnityEngine;
using Galactic1.Core.GameSession;
using Galactic1.Gameplay.Locations;
using Galactic1.Meta.Configs.Recruitment;

namespace Galactic1.Core.Systems.Factories
{
    /*
     * PlayerFactory — аналог HeroFactory в LDoE.
     *
     * Получает PlayerSpawnPreset и собирает полноценного игрока:
     *  - создаёт игрового аватара
     *  - применяет статы
     *  - ставит экипировку
     *  - наполняет инвентарь
     *  - ставит в нужную позицию
     */
    public sealed class PlayerFactory : IGameService
    {

        public PlayerFactory()
        {
        }

        public SurvivorInstance Create(
            int index,
            SceneSessionDefinition context,
            WeaponAnimLibrary animLibrary,
            IUnitRuntime runtimeSource,
            (string prefabPath, UnitIdentityPoolConfig.ArchetypePrefabEntry variant) survEntry)
        {
            // var playerData = context.PlayerSpawnPreset.GetData();
            // playerData.UnitRuntime = runtimeSource;
            // playerData.InventoryPort = context.InventoryPort;
            // playerData.AnimLibrary = animLibrary;

            // Создаем юнит
            // var instance = playerData.CharacterConfig.Prefab
            //     .CreateGO(ServiceLocator.Current.Get<Environment>().playerUnits)
            //     .GetComponent<SurvivorInstance>();

            var instance = $"{AppConstants.PATH_PLAYER}{survEntry.prefabPath}"
                .CreateGO(ServiceLocator.Current.Get<Environment>().playerUnits)
                .GetComponent<SurvivorInstance>();
            instance.GetComponent<CharacterAppearanceController>().Apply(survEntry.variant.AppearanceId);

            var uniqueId = runtimeSource.Id;
            instance.UniqueId = uniqueId;
            instance.Id = index;
            instance.name = "Srv_"+runtimeSource.DisplayName;

            // ── Регистрация ──────────────────────────────────────
            // 1. Canonical scene registry — для Damage, AoE, Perception и т.д.
            ServiceLocator.Current.Get<UnitSceneRepository>().Register(uniqueId, instance);
            // 2. Filtered view — для Squad UI, Player Commands, Recruitment
            ServiceLocator.Current.Get<SurvivorRepository>().Register(uniqueId);

            instance.Tr.position = ResolveSpawnPosition(context.LocationContext, runtimeSource.IsCampDefender);

            // Передаём контекст
            context.Survivors.Add(instance);

            // создаём адаптер для сцены
            //var adapter = new SceneUnitAdapter(runtimeSource);
            //instance.Entity_Initialize(adapter);

            // toast над юнитом
            //runtimeSource.BindScene(instance);


            // 2 — статы
            //ApplyStats(playerData, instance, context.PlayerSpawnPreset);


            // 3 — экипировка
            //ApplyEquipment(playerData, instance, context.PlayerSpawnPreset);

            // 4 — инвентарь
            //ApplyInventory(playerData, instance, context.PlayerSpawnPreset);


            // Пока игрок не активирован (не принимает управление)
            //player.DisableInput();


            // * регистрация для очистки
            EventBus<SceneClearEvent>.Register(new EventBinding<SceneClearEvent>(() => Clear(uniqueId)));

            return instance;
        }

        public void Clear(string uniqueId)
        {
            var sceneRepo = ServiceLocator.Current.Get<UnitSceneRepository>();
            var survivorRepo = ServiceLocator.Current.Get<SurvivorRepository>();

            var result = sceneRepo.TryGet(uniqueId);
            if (result.done)
            {
                sceneRepo.Unregister(uniqueId, result.instance);
                survivorRepo.Unregister(uniqueId);
                result.instance.Entity_Destroy();
            }
        }


        private void ApplyStats(PlayerLoadData playerData, SurvivorInstance instance, PlayerSpawnPreset preset)
        {
            instance.Entity_Setup(playerData);
            //survGO.GetComponent<SurvivalController>().Initialize();
        }

        private void ApplyEquipment(PlayerLoadData playerData, SurvivorInstance instance, PlayerSpawnPreset preset)
        {
            instance.GetComponent<UnitAnimationController>().SetWeapon(WeaponType.Unarmed);
            ServiceLocator.Current.Get<CoroutineController>()
                .Coroutine_wait(.2f, () => PlayerEquipmentApplier.Apply(playerData, instance));
        }

        private void ApplyInventory(PlayerLoadData playerData, SurvivorInstance instance, PlayerSpawnPreset preset)
        {

        }

        /// <summary>
        /// Находит позицию для спавна юнита отряда в прямоугольной области вокруг LocationContext.PlayerSpawnPosition
        /// (ширина по X = SquadSpawnWidth, глубина по Z = SquadSpawnDepth), избегая наложения на уже заспавненных
        /// юнитов (минимальная дистанция SquadSpawnMinDistance).
        /// Найденная позиция запоминается в LocationContext.OccupiedSpawnPositions.
        /// </summary>
        private Vector3 ResolveSpawnPosition(LocationContext locationContext, bool isCampDefender)
        {
            // #1 если есть точки спавна для лагеря, используем их
            if (isCampDefender && locationContext.CampUnitSpawnPosition?.Length > 0)
            {
                return locationContext.CampUnitSpawnPosition[++locationContext.LastIdCampSpawnPoint];
            }
            
            
            // #2 обычный спавн в зоне для рейда
            var center = locationContext.PlayerSpawnPosition;
            float halfWidth = locationContext.SquadSpawnWidth * 0.5f;
            float halfDepth = locationContext.SquadSpawnDepth * 0.5f;
            float minDistance = locationContext.SquadSpawnMinDistance;

            const int maxAttempts = 30;

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                var offsetX = Random.Range(-halfWidth, halfWidth);
                var offsetZ = Random.Range(-halfDepth, halfDepth);
                var candidate = center + new Vector3(offsetX, 0f, offsetZ);

                if (IsFarEnough(candidate, locationContext.OccupiedSpawnPositions, minDistance))
                {
                    locationContext.OccupiedSpawnPositions.Add(candidate);
                    return candidate;
                }
            }

            // Fallback: место не нашлось за maxAttempts (область слишком мала для отряда) —
            // спавним в центре с небольшим смещением, чтобы юниты хотя бы не встали в одну точку.
            Debug.LogWarning("[PlayerFactory] Не удалось найти свободную точку спавна за " +
                             $"{maxAttempts} попыток. Width={locationContext.SquadSpawnWidth}, " +
                             $"Depth={locationContext.SquadSpawnDepth}, MinDistance={minDistance}. " +
                             "Используется fallback-позиция.");

            var fallback = center + new Vector3(Random.Range(-0.3f, 0.3f), 0f, Random.Range(-0.3f, 0.3f));
            locationContext.OccupiedSpawnPositions.Add(fallback);
            return fallback;
        }

        private static bool IsFarEnough(Vector3 candidate, List<Vector3> occupied, float minDistance)
        {
            foreach (var pos in occupied)
            {
                if (Vector3.Distance(candidate, pos) < minDistance)
                    return false;
            }

            return true;
        }
    }
}