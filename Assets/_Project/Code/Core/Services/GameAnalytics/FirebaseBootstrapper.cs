using System;
using System.Collections.Generic;
using Firebase;
using Firebase.Extensions;
using UnityEngine;

namespace Galactic1.Code.Systems.Analytics.Providers
{
    /// <summary>
    /// Идемпотентная инициализация FirebaseApp. Общая для analytics и (в будущем) Crashlytics.
    /// Повторный вызов после Failed запускает новую попытку.
    /// Колбэки приходят в main thread (ContinueWithOnMainThread).
    /// </summary>
    public static class FirebaseBootstrapper
    {
        private enum State { None, Checking, Ready, Failed }

        private static State _state = State.None;
        private static readonly List<Action<bool>> Waiters = new();

        public static bool IsReady => _state == State.Ready;

        public static void Ensure(Action<bool> onComplete)
        {
            switch (_state)
            {
                case State.Ready:
                    Invoke(onComplete, true);
                    return;

                case State.Checking:
                    Waiters.Add(onComplete);
                    return;
            }

            _state = State.Checking;
            Waiters.Add(onComplete);

            try
            {
                FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
                {
                    bool ok = !task.IsFaulted && !task.IsCanceled
                              && task.Result == DependencyStatus.Available;

                    if (ok)
                    {
                        try
                        {
                            var _ = FirebaseApp.DefaultInstance; // форсируем создание default app
                        }
                        catch (Exception ex)
                        {
                            Debug.LogWarning($"[Firebase] DefaultInstance failed: {ex.Message}");
                            ok = false;
                        }
                    }
                    else
                    {
                        Debug.LogWarning(task.IsFaulted
                            ? $"[Firebase] Dependency check faulted: {task.Exception?.GetBaseException().Message}"
                            : $"[Firebase] Dependencies not available: {(task.IsCanceled ? "canceled" : task.Result.ToString())}");
                    }

                    Complete(ok);
                });
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Firebase] Ensure threw: {ex.Message}");
                Complete(false);
            }
        }

        private static void Complete(bool ok)
        {
            _state = ok ? State.Ready : State.Failed;

            var copy = new List<Action<bool>>(Waiters);
            Waiters.Clear();

            foreach (var waiter in copy)
                Invoke(waiter, ok);
        }

        private static void Invoke(Action<bool> callback, bool ok)
        {
            try { callback?.Invoke(ok); }
            catch (Exception ex) { Debug.LogWarning($"[Firebase] Callback threw: {ex.Message}"); }
        }
    }
}