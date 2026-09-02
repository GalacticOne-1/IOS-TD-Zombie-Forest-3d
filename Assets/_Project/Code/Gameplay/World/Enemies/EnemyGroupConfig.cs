
using System;
using System.Collections.Generic;
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Game.Meta.Enemy;
using UnityEngine;

namespace Galactic1.Configs.Enemies
{
    /// <summary>
    /// Конфиг состава группы врагов.
    /// Отвечает только на вопрос КТО спавнится.
    /// ГДЕ спавнится — хранит AmbientSpawnPoint на сцене.
    /// </summary>
    [CreateAssetMenu(
        fileName = "AmbientGroupConfig",
        menuName = "Game Configs/Enemy/Ambient Group Config")]
    public sealed class EnemyGroupConfig : ScriptableObject
    {
        public AmbientGroupId GroupId;
        public List<AmbientEnemyEntry> Enemies = new();
    }

    [Serializable]
    public sealed class AmbientEnemyEntry
    {
        public EnemyArchetypeConfig Enemy;
        public int Count = 1;
    }
}