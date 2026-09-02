using Galactic1;
using UnityEngine;

namespace Galactic1
{
    public class DetailObjViewModel : MVVMViewModel
    {
        public DetailObjViewModel(MVVMModel _model, MVVMView _view) : base(_model, _view)
        {
            model = _model;
            view = _view;

        }



        public void GetWindow(string title, string des, _EntityConfig_.CStatGUI[] stat)
        {
            view.Show();
            (model as DetailObjModel).Show(title, des, stat);
        }
    }
}