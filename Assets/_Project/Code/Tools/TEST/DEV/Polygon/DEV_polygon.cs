
using Galactic1;
using Galactic1.AbstractFactory;
using Galactic1.Code.Gameplay.Enemies.Spawning;
using Galactic1.Code.Gameplay.Enemies.Spawning.Requests;
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Configs;
using Galactic1.Core.Systems.GameLoopSession;
using UnityEngine;

namespace DEV
{
    /// <summary>
    /// DEV-инструмент для тестирования спавна врагов на полигоне.
    ///
    /// ПРАВИЛО: DEV-инструменты используют те же пути спавна что и реальный геймплей.
    ///   НЕ создаём runtime напрямую.
    ///   НЕ вызываем ZombieRuntimeFactory / EnemyRuntimeFactory напрямую.
    ///   Используем EnemySpawnSystem.Enqueue() — единственный вход в пайплайн.
    /// </summary>
    public class DEV_polygon : Singleton<DEV_polygon>
    {
        private DIContainer _container;

        [SerializeField] private DevPolygonConfig _polygonConfig;
        [SerializeField] private Vector3 startCoord;

        public void LoadPolygon(DIContainer container)
        {
            if (!DeveloperConsole.I.core.dev_polygon) return;

            _container = container;

            DLog.Alert(
                "Активирован полигон, для спавна новой цели используй клавишу E",
                EDlogColor.YELLOW);

            var l = _polygonConfig.realEnemy
                ? _polygonConfig.realList.Length
                : _polygonConfig.devList.Length;

            for (int i = 0; i < l; i++)
            {
                if (_polygonConfig.realEnemy)
                    SpawnCreature(i, startCoord, _polygonConfig.realList[i]);
                else
                    SpawnCreatureDev(i, startCoord, _polygonConfig.devList[i]);

                startCoord.x += 4;
            }
        }

        /// <summary>
        /// Спавн девелоперского (упрощённого) зомби-объекта напрямую.
        /// Используется только для отладочных non-gameplay объектов.
        /// </summary>
        private void SpawnCreatureDev(int index, Vector3 coord, DevPolygonConfig.CData data)
        {
            var unit = data.prefab.gameObject
                .CreateGO(ServiceLocator.Current.Get<Environment>().enemies)
                .GetComponent<DevZombieInstance>();

            unit.transform.position = coord;
            unit.name = "dev_zombie_" + index;
        }

        /// <summary>
        /// Спавн реального игрового врага через канонический пайплайн.
        ///
        /// Создаём EnemySpawnRequest и передаём в EnemySpawnSystem.Enqueue().
        /// Это ИДЕНТИЧНО поведению WaveSystem — DEV-инструмент не нарушает архитектуру.
        /// </summary>
        private void SpawnCreature(int index, Vector3 coord, DevPolygonConfig.CDataReal data)
        {
            var spawnSystem = _container.Resolve<EnemySpawnSystem>();

            var request = new EnemySpawnRequest(
                data.configId,
                coord,
                string.Empty, // случайный вариант
                null, // без модификаторов
                0);

            spawnSystem.Enqueue(request);

            // Немедленно тикаем чтобы запрос обработался в тот же кадр на полигоне
            spawnSystem.Tick(0f);
        }
        
        public void SpawnTarget() 
            => SpawnCreature(0, startCoord, _polygonConfig.realList[0]); 

    }
}