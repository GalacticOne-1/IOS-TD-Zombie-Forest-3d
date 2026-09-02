using UnityEngine;

namespace Galactic1
{
    public class FinishLevelViewModel : MVVMViewModel, IScreenT
    {
        
        
        
        public FinishLevelViewModel(MVVMModel _model, MVVMView _view) : base(_model, _view)
        {
            model = _model;
            view = _view;
            
            var vw = view as FinishLevelView;
            
            // кнопка для перехода в лобби
            vw.BConfirm.GetComponent<CoreBtn>()._event.AddListener((model as FinishLevelModel).CloseWindow);
        }
        





        public void OpenWindow<T>(T data)
        {
            var d = data as FinishLevelModel.CData;
            (model as FinishLevelModel).Load(d); 
        }

        public void CloseWindow(){}

        
    }
}