using System;
using R3;

namespace Galactic1.Code.Core.State
{
    /// <summary>
    /// Универсальный безопасный писатель для структур GameState,
    /// находящихся внутри ReactiveProperty.
    /// Гарантирует реактивное уведомление и отсутствие скрытых мутаций.
    /// </summary>
    public static class StateWriter
    {
        /// <summary>
        /// Безопасно модифицирует структуру состояния.
        /// </summary>
        public static void Write<TState>(
            ReactiveProperty<TState> property,
            ActionRef<TState> mutation)
            where TState : struct
        {
            var value = property.Value;     // копия
            mutation(ref value);            // изменяем
            property.Value = value;         // ОБЯЗАТЕЛЬНАЯ реактивная запись
        }
    }

    /// <summary>
    /// Делегат с ref-параметром для мутаций struct.
    /// </summary>
    public delegate void ActionRef<T>(ref T value);
}