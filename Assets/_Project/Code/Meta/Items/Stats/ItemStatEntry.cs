using System;
using Galactic1.Game.Meta.Items;
using UnityEngine;

namespace Galactic1.Game.Meta.Stats
{
    [Serializable]
    public class ItemStatEntry
    {
        public StatId StatId;
        public ModifierOperation Operation;
        public float Value;

        [Tooltip("Показывать в тултипе предмета")]
        public bool showInTooltip = true;

        [Tooltip("Применять как модификатор юнита")]
        public bool applyToUnit = true;
    }
}