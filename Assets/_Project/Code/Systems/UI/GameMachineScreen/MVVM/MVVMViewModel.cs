using System;
using System.Collections.Generic;
using UnityEngine;

namespace Galactic1
{
    public abstract class MVVMViewModel
    {
        protected MVVMModel model;
        protected MVVMView view;

        /// Даст объект виджета
        public GameObject GetScreen() => view.gameObject;
        
        
        public List<IDisposable> _disposables = new List<IDisposable>();
        

        public MVVMViewModel(MVVMModel _model, MVVMView _view)
        {
            model = _model;
            view = _view;
        }
        
        
        public void Dispose()
        {
            foreach (var disposable in _disposables)
            {
                disposable.Dispose();
            }
            _disposables.Clear();
        }

        
        

        public virtual void ResetState() {}

        public virtual void ShowScreen() => view.Show();
        
        public virtual void HideScreen() => view.Hide();
        
        
        /// <summary>
        /// Доуступ к кнопкам
        /// </summary>
        /// <param name="level"></param>
        public virtual void LoadAccess(int level) {}
    }
}