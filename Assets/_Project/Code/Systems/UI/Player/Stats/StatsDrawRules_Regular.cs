
using UnityEngine;

namespace Galactic1.Core.UI
{
    [CreateAssetMenu(fileName = "StatsDrawRules_Regular", menuName = "Game Configs/UI/Stats Draw Rules_Regular")]
    public class StatsDrawRules_Regular : StatsDrawRules
    {
        
        public override void Apply(StatSlotParam param)
        {
            if (param.valueText)
            {
                if (!param.valueText.gameObject.activeInHierarchy || !param.textAnimation)
                    param.valueText.text = $"{param.currValue:0}";
                else if(param.textAnimation)
                    SetValue(param.valueText, param.currValue);
            }
            
            if(param.fillBar)
                param.fillBar.fillAmount = param.currValue / param.maxValue;
        }

        
    }
}