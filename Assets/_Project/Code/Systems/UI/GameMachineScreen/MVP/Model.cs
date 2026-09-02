using UnityEngine;

namespace Galactic1
{
    public abstract class Model
    {
        protected View view;


        public Model(View _view)
        {
            view = _view;
        }




        public virtual void ShowScreen() => view.Show();
        
        public virtual void HideScreen() => view.Hide();
    }
}