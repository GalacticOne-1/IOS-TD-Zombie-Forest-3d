using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Galactic1.Core.UI
{
    [Serializable]
    public class StatSlotUI
    {
        [field: SerializeField] public StatId StatId { get; private set; }
        [SerializeField] private TMP_Text ValueText;
        [SerializeField] private bool RequiresTextAnimation;
        [SerializeField] private Image FillBar;
        

        private StatsDrawRules drawRules;
        public StatsDrawRules DrawRules { set => drawRules = value; }


        public void Set(
            float currValue, 
            float maxValue, 
            bool excludeEffects = false)
        {
#if UNITY_EDITOR
            //DLog.Alert($"Setting {StatId} to {currValue}", EDlogColor.YELLOW);
#endif

            drawRules.Apply(new StatSlotParam()
            {
                StatId = StatId,
                valueText = ValueText,
                fillBar = FillBar,
                currValue = currValue,
                maxValue = maxValue,
                textAnimation = RequiresTextAnimation && !excludeEffects
            });
        }
    }
}