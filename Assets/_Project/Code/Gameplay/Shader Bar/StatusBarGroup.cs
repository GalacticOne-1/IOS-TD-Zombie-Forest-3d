
using UnityEngine;

namespace Galactic1
{
    public class StatusBarGroup : MonoBehaviour
    {
        [SerializeField] private Vector3 _worldOffset;
        private Camera _worldCamera;
        private Transform _target;
        
        
        [Space]
        [SerializeField] private Bar hpBar;
        [SerializeField] private Bar armorBar;
        [SerializeField] private Bar manaBar;

        private RectTransform rectTransform;
        

        void Awake()
        {
            hpBar?.Initialize();
            manaBar?.Initialize();
            armorBar?.Initialize();
        }

        void OnValidate()
        {
            hpBar?.UpdateMaterial();
            manaBar?.UpdateMaterial();
            armorBar?.UpdateMaterial();
        }
        
        public void OnDestroy()
        {
            
        }


        public void Initialize(Transform target)
        {
            _target = target;
            rectTransform = GetComponent<RectTransform>();
            //_worldCamera = ServiceLocator.Current.Get<RTSCameraController>().Camera;
        }

        

        void LateUpdate()
        {
            if (_target == null || _worldCamera == null) return;

            Vector3 screenPos = _worldCamera.WorldToScreenPoint(_target.position + _worldOffset);
            if (screenPos.z < 0)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);
            rectTransform.position = screenPos;
        }
        

        // Пример интерфейсов управления
        public void SetHP(float value) => hpBar?.SetProgress(value);
        public void SetMana(float value) => manaBar?.SetProgress(value);
        public void SetArmor(float value) => armorBar?.SetProgress(value);
    }

}