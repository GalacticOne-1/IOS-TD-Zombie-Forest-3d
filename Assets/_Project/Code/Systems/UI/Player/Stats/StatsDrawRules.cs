
using Galactic1.UI.Core;
using TMPro;
using UnityEngine;

namespace Galactic1.Core.UI
{
    public abstract class StatsDrawRules : ScriptableObject
    {
        private float lastValue;

        protected void SetValue(TMP_Text text, float newValue)
        {
            ServiceLocator.Current.Get<UIAnimationService>().AnimateStatChange(text, lastValue, newValue);
            lastValue = newValue;
        }
        
        public abstract void Apply(StatSlotParam param);
    }
}