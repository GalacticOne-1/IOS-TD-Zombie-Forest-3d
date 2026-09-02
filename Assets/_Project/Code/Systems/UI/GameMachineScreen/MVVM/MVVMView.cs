using UnityEngine;

namespace Galactic1
{
    public abstract class MVVMView : MonoBehaviour
    {
        protected MVVMViewModel presenter;

        

        public virtual void Init(MVVMViewModel _presenter)
        {
            presenter = _presenter;
        }





        public virtual void Show() => gameObject.SetActive(true);
        
        public virtual void Hide() => gameObject.SetActive(false);
    }
}