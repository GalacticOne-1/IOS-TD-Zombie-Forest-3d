using System;
using UnityEngine;


namespace Galactic1.UI.Core
{
    public abstract class UIPanel : MonoBehaviour
    {
        public UIScreenId PanelId { get; private set; }
        protected DIContainer _container;

        public event Action OnClosed;


        /// <summary>
        /// Вызываем при создании окна (PreloadScreens)
        /// </summary>
        /// <param name="id"></param>
        public virtual void Initialize(DIContainer container, UIScreenId id)
        {
            PanelId = id;
            _container = container;
        }

        /// <summary>
        /// Вызываем при уничтожении окна
        /// <br/>(Для отписки)
        /// </summary>
        public virtual void Remove()
        {
            
        }
        
        
        /// <summary>
        /// Для открытия
        /// </summary>
        /// <param name="data"></param>
        public virtual void OnShow(object data = null) { }

        /// <summary>
        /// Для закрытия
        /// </summary>
        public virtual void OnHide()
        {
            OnClosed?.Invoke();
            OnClosed = null;
        }

        public virtual void ResetState(){}

        // Override if panel needs specific teardown
        public virtual void Release() { }
    }
}