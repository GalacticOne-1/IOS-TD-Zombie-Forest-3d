using Galactic1;
using Galactic1.Gameplay.Interaction;
using UnityEngine;

namespace Game.Gameplay.Interaction
{
    /// <summary>
    /// Manages a pooled highlight instance that attaches to the current interactable object.
    /// The highlight object is one prefab reused for all interactions.
    /// </summary>
    public class InteractionHighlightController : MonoBehaviour
    {
        [Header("BasicSettings")]
        [SerializeField] private Vector3 offset = Vector2.up * 0.2f;


        private Camera mainCamera;
        private IObjectContext _target;
        
        
        
        
        private void Awake()
        {
            mainCamera = Camera.main;
            Hide();
        }
        
        private void Update()
        {
            if (_target != null)
            {
                Pos();
            }
        }

        
        // позиция над объектом
        void Pos()
        {
            Vector3 screenPos = mainCamera.WorldToScreenPoint(_target.PivotCenterBottom() + offset);
            transform.position = screenPos;
        }
        
        

        /// <summary>
        /// Attach highlight to a target.
        /// </summary>
        public void Show(IObjectContext target)
        {
            if (target != _target)
            {
                _target = target;
                Pos();
                gameObject.SetActive(true);
            }
        }


        /// <summary>
        /// Hide highlight if shown.
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
            _target = null;
        }
    }
}