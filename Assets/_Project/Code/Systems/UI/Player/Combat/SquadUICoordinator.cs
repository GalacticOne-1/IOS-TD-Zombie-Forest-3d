using System;
using Galactic1.Code.UI.Interaction;
using UnityEngine;

namespace Galactic1.Code.UI.UnitCard
{
    /// <summary>
    /// В долгосрочной перспективе
    /// <br/>- выделение юнита, фокус камеры и т.д.
    /// </summary>
    public sealed class SquadUICoordinator
    {
        private readonly UIStateController _uiState;

        public event Action OnAbilitySelectOpened;
        public event Action<TargetingUIData> OnTargetingStarted;
        public event Action OnTargetingStopped;


        public SquadUICoordinator(UIStateController uiState)
        {
            _uiState = uiState;
        }

        public void NotifyAbilitySelectOpened()
            => OnAbilitySelectOpened?.Invoke();

        public void NotifyTargetingStarted(TargetingUIData data)
        {
            _uiState.Push(new AbilityState());
            OnTargetingStarted?.Invoke(data);
        }

        public void NotifyTargetingStopped()
        {
            _uiState.Remove<AbilityState>();
            OnTargetingStopped?.Invoke();
        }
    }

    public struct TargetingUIData
    {
        public Sprite Icon;
        public string ItemName;
        public Action OnCancel;
    }
}