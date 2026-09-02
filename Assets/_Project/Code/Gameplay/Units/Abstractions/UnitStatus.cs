using System;

namespace Galactic1.Code.Gameplay.Units
{
    public sealed class UnitStatus
    {
        private bool _abilityBusy;
        private bool _isHungry;
        private bool _isThirsty;

        public bool AbilityBusy => _abilityBusy;
        public bool IsHungry => _isHungry;
        public bool IsThirsty => _isThirsty;

        public event Action<bool> AbilityBusyChanged;
        public event Action<bool> HungerChanged;
        public event Action<bool> ThirstChanged;

        public void SetAbilityBusy(bool value)
        {
            if (_abilityBusy == value) return;
            _abilityBusy = value;
            AbilityBusyChanged?.Invoke(value);
        }

        public void SetHungry(bool value)
        {
            if (_isHungry == value) return;
            _isHungry = value;
            HungerChanged?.Invoke(value);
            // TODO: активация иконки 🍖 в UI/мире — подписка на HungerChanged
        }

        public void SetThirsty(bool value)
        {
            if (_isThirsty == value) return;
            _isThirsty = value;
            ThirstChanged?.Invoke(value);
            // TODO: активация иконки 💧 в UI/мире — подписка на ThirstChanged
        }
    }
}