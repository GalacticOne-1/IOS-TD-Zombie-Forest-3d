using System;
using UnityEngine;

namespace Galactic1.UI.Shop
{
    /// <summary>
    /// Централизованное управление состояниями магазина.
    /// </summary>
    public class ShopStateMachine
    {
        public ShopState Current { get; private set; }

        public event Action<ShopState> OnStateChanged;

        public ShopStateMachine(ShopState initial = ShopState.Closed)
        {
            Current = initial;
        }

        /// <summary>
        /// Попытка смены состояния.
        /// Возвращает true, если смена успешна.
        /// </summary>
        public void TransitionTo(ShopState next)
        {
            if (Current == next || !CanTransitionTo(next))
                return;

            Current = next;
            OnStateChanged?.Invoke(Current);
        }

        /// <summary>
        /// Проверяет, возможен ли переход из текущего состояния в указанное.
        /// </summary>
        public bool CanTransitionTo(ShopState next)
        {
            return Current switch
            {
                ShopState.Closed => next == ShopState.Loading || next == ShopState.Ready,
                ShopState.Loading => next == ShopState.Ready || next == ShopState.Closed,
                ShopState.Ready => next == ShopState.Purchasing || next == ShopState.Restoring || next == ShopState.Closed,
                ShopState.Purchasing => next == ShopState.Ready,
                ShopState.Restoring => next == ShopState.Ready,
                _ => false
            };
        }
    }
}