
using UnityEngine;

namespace Galactic1.UI.Core
{
    public class UITextAnimator : MonoBehaviour
    {
        private float lastValue;

        public void SetValue(float newValue)
        {
            ServiceLocator.Current.Get<UIAnimationService>().AnimateStatChange(gameObject.CMP_Text(), lastValue, newValue);
            lastValue = newValue;
        }
    }
}