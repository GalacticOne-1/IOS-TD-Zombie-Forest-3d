
using UnityEngine;

namespace Galactic1
{
    public abstract class Presenter
    {
        protected Model model;


        public Presenter(Model _model)
        {
            model = _model;
        }




        public virtual void ShowScreen() => model.ShowScreen();
        
        public virtual void HideScreen() => model.HideScreen();
    }
}