using UnityEngine;
using UnityEngine.UI;
using Galactic1.Core.Input;

namespace Galactic1.Core.UI
{
    public class UIInventoryButton : MonoBehaviour
    {
        private void Start()
        {
            GetComponent<Button>().onClick.AddListener(() =>
                InputManager.Instance.InventoryPressed());
        }
    }
}