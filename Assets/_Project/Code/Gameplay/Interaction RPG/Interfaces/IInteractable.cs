// File: Code/Gameplay/Interaction/Interface/IInteractable.cs
using UnityEngine;

namespace Galactic1.Gameplay.Interaction
{
    /// <summary>
    /// Общий интерфейс для всех интерактивных объектов.
    /// Обеспечивает базовую контрактную логику: доступность, фокус и взаимодействие.
    /// </summary>
    public interface IInteractable
    {
        IObjectContext IObjectContext { get; }
        
        /// <summary>Можно ли сейчас взаимодействовать (дистанция/состояние)</summary>
        bool CanInteract(Transform interactor, bool isDragon);

        /// <summary>Вызвать действие взаимодействия (например, открыть сундук)</summary>
        void Interact(Transform interactor);

        /// <summary>Информационный блок (иконка/название/флаг доступности для UI)</summary>
        InteractionInfo GetInfo();

        /// <summary>Вызывается когда объект стал ближайшим (фокус)</summary>
        void OnFocus();

        /// <summary>Вызывается когда объект потерял фокус</summary>
        void OnFocusLost();

        /// <summary>Мировая позиция интеракта (для сортировки/визуализации)</summary>
        Vector2 WorldPosition { get; }

        /// <summary>Флаг — доступен ли интеракт сейчас (для фильтрации)</summary>
        bool IsAvailable { get; }
    }

    public struct InteractionInfo
    {
        public string Name;
        public Sprite Icon;
        public bool IsAvailable;
    }
}