using System;
using UnityEngine;

namespace Galactic1.Configs.UI
{
    [CreateAssetMenu(fileName = "InventoryButtonRules", menuName = "Game Configs/Rules/UI/Inventory Button Rules")]
    public class InventoryButtonRulesConfig : ScriptableObject
    {
        [Serializable]
        public class ButtonRule
        {
            public string buttonName; // "Use", "Split" и т.д.

            public ButtonRuleLogic isEnabledLogic;
            public ButtonRuleLogic isHighlightedLogic;
        }

        public ButtonRule[] rules;
    }
}