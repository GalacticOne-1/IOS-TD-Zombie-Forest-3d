
using UnityEngine;

namespace Galactic1
{
    public abstract class View : MonoBehaviour
    {
        protected Presenter presenter;

        public void Init(Presenter _presenter)
        {
            presenter = _presenter;
        }





        public virtual void Show() => gameObject.SetActive(true);
        
        public virtual void Hide() => gameObject.SetActive(false);
    }
}