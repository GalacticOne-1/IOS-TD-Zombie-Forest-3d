using System;
using R3;
using UnityEngine;

namespace Galactic1.Code.UI.Core
{
    public abstract class ReactiveWidget<T> : ReactiveWidgetBase
    {
        private IDisposable _subscription;

        protected abstract Observable<T> GetObservable();
        protected abstract void Refresh(T value);


        public override void Initialize()
        {
            _subscription = GetObservable().Subscribe(Refresh);

            EventBus<SceneClearEvent>.Register(new EventBinding<SceneClearEvent>(() =>
            {
                _subscription?.Dispose();
                _subscription = null;
            }));
        }

    }
}