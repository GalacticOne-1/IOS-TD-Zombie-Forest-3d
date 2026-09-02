using System;
using Galactic1.Gameplay.Interaction;
using UnityEngine;

namespace Galactic1.Gameplay.Player.StateMachine
{
    /// <summary>
    /// Описывает задачу действия игрока: длительность, callback-и, возможность отмены.
    /// ActionJob инкапсулирует поведение: старт/апдейт/отмена/завершение.
    /// </summary>
    public class ActionJob
    {
        public IInteractable TargetInteractable { get; }
        public string Name { get; }
        public float Duration { get; }
        public bool CanBeInterrupted { get;  }
        public Action OnStarted;
        public Action OnFinished;
        public Action OnCancelled;
        public float RemainingTime { get; private set; }
        public bool IsCompleted { get; private set; }
        public bool IsCancelled { get; private set; }
        

        public ActionJob(string name, float duration, bool canBeInterrupted = true, IInteractable target = null)
        {
            Name = name;
            Duration = duration;
            CanBeInterrupted = canBeInterrupted;
            TargetInteractable = target;
        }

        public void Start()
        {
            RemainingTime = Duration;
            IsCompleted = false;
            IsCancelled = false;
            OnStarted?.Invoke();
        }

        /// <summary>Вызывать из Update() владельца</summary>
        public void Tick(float delta)
        {
            if (IsCompleted || IsCancelled) return;

            RemainingTime -= delta;
            if (RemainingTime <= 0f)
            {
                Complete();
            }
        }

        public void Cancel(string reason = null)
        {
            if (!CanBeInterrupted) return;
            IsCancelled = true;
            OnCancelled?.Invoke();
        }

        private void Complete()
        {
            IsCompleted = true;
            OnFinished?.Invoke();
        }
    }
}