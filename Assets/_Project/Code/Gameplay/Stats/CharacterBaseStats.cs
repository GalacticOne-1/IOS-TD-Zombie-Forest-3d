using System.Collections.Generic;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Units.Stats
{
    public abstract class CharacterStatsBase : ScriptableObject
    {

        public abstract Dictionary<StatId, float> GetBaseStats();

        // ! для разных сущностей наслодоватся от этого класса и добавлять конфиги оружия и защиты !
    }
}