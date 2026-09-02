using Galactic1;
using UnityEngine;

namespace Galactic1
{
    public class StageViewModel : MVVMViewModel
    {
        public StageViewModel(MVVMModel _model, MVVMView _view) : base(_model, _view)
        {
            model = _model;
            view = _view;
        }







        public void Load()
        {
            (model as StageModel).Load();
        }
        
        
        public void StateWave() => (model as StageModel).StateWave();
        
        public void NextStage() => (model as StageModel).NextStage();

        public void WaveBar(float cur, float max)
            => (view as StageView).FillWave.fillAmount = cur / max;
    }
}