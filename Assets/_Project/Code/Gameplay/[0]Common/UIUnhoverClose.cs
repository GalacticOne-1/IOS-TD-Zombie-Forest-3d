using UnityEngine;
using UnityEngine.EventSystems;

namespace Galactic1
{
    // простое закрытие панели при ее unhover
    public class UIUnhoverClose : MonoBehaviour, IPointerExitHandler
    {
        
        public void OnPointerExit(PointerEventData eventData)
        {
            gameObject.SetActive(false);
        }
    }
}