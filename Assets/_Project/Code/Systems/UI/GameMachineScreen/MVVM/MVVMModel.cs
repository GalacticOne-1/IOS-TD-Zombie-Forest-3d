using System;
using System.Collections.Generic;
using UnityEngine;

namespace Galactic1
{
    public class MVVMModel
    {

        #region RX

        public List<IDisposable> _disposables = new List<IDisposable>();
        
        public void Dispose()
        {
            foreach (var disposable in _disposables)
            {
                disposable.Dispose();
            }
            _disposables.Clear();
        }
        

        #endregion
        
        
        
        protected MVVMView view;
        

        public MVVMModel(MVVMView _view)
        {
            view = _view;
        }




        
        
        public virtual void LoadAccess(int level) {}
    }
}