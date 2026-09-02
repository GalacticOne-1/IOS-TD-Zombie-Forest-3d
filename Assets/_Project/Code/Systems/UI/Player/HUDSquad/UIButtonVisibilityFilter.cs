using UnityEngine;
using Galactic1.Core.Input;

namespace Galactic1.Core.UI.HUD
{
    public class UIButtonVisibilityFilter : MonoBehaviour
    {
        [SerializeField] private ControllableType[] allowedTypes;
        private GameObject targetObject;

        private void Awake()
        {
            targetObject = gameObject;
            InputManager.Instance.OnControllableChanged += OnControlChanged;
        }

        // private void Start()
        // {
        //     OnControlChanged(InputManager.Instance.ActiveControllable);
        // }

        private void OnControlChanged(ControllableType type)
        {
            bool visible = false;

            foreach (var t in allowedTypes)
            {
                if (t == type)
                {
                    visible = true;
                    break;
                }
            }

            targetObject.SetActive(visible);
        }
    }
}