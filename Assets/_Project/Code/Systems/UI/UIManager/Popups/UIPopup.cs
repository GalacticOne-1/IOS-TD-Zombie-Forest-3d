using System;
using UnityEngine;


namespace Galactic1.UI.Core
{
    public abstract class UIPopup : UIPanel
    {
        public UIPopupConfig Config { get; private set; }
        public CanvasGroup CanvasGroup { get; private set; }

        public Action<UIScreenId> OnCloseAction;


        public virtual void Initialize(DIContainer container, UIPopupConfig config)
        {
            base.Initialize(container, config.id);
            Config = config;
            CanvasGroup = gameObject.GetComponent<CanvasGroup>();
            if (CanvasGroup != null)
                CanvasGroup.alpha = 0;
        }

        public override void ResetState()
        {
            if (CanvasGroup != null)
            {
                CanvasGroup.alpha = 0f;
            }
        }

        public override void OnShow(object data = null) { }
        public override void OnHide() { base.OnHide(); }

        
    }
}