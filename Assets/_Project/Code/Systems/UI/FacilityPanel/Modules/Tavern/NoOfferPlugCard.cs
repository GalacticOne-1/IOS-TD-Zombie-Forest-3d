
using Galactic1.Code.UI.Common.Effects;
using UnityEngine;

namespace Galactic1.Code.UI.Buildings
{
    /// <summary>
    /// Отображает кандидата, экипировку, статы и кнопку найма.
    /// </summary>
    public class NoOfferPlugCard : MonoBehaviour
    {
        
        public void Bind()
        {
            // эффект появления карточки
            var fadeComponent = GetComponent<UIFadeComponent>();
            fadeComponent.Setup();
            fadeComponent.SetInstant(0f); // карточка изначально скрыта
            gameObject.GetChild(0).SetActive(false);
            
            
            // добавляем появление
            fadeComponent.FadeIn(true,() => gameObject.GetChild(0).SetActive(true));
        }

        
    }
}