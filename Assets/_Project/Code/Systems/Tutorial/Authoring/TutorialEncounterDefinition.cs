using UnityEngine;
using Galactic1.Configs.Enemies;

namespace Galactic1.Code.Systems.Tutorial.Authoring
{
    /// <summary>
    /// Generic конфигурация "что заспавнить, когда encounter активирован" — раздел 19 ТЗ
    /// Chapter 2. НЕ знает про Chapter2/шаги/Molotov — просто состав группы + радиус
    /// разброса. Переиспользует существующий EnemyGroupConfig (тот же тип, что
    /// EnemySpawnPoint.Group уже использует для ambient-спавна) — не дублирует
    /// классификацию "кто спавнится".
    /// </summary>
    [CreateAssetMenu(fileName = "TutorialEncounter_",
        menuName = "Game Configs/Tutorial/Encounter Definition")]
    public sealed class TutorialEncounterDefinition : ScriptableObject
    {
        [Tooltip("Состав группы — сколько и каких врагов заспавнить.")]
        public EnemyGroupConfig group;

        [Tooltip("Радиус случайного разброса вокруг точки триггера — та же семантика, что " +
                 "EnemySpawnPoint.WanderRadius/EnemySpawnPointResolver.RandomRadius.")]
        public float spawnRadius = 3f;
    }
}
