using UnityEngine;

namespace Galactic1
{
    public static class AdaptiveFPS
    {
        public enum GameState { Active, Idle, Paused }

        private static GameState currentState = GameState.Active;

        // Хранение FPS для каждого состояния
        private static readonly int activeFPS = 60;
        private static readonly int idleFPS = 30;
        private static readonly int pausedFPS = 15;

        public static void SetState(GameState newState)
        {
            currentState = newState;

            int targetFPS = GetFPSForState(newState);
            ApplyFPS(targetFPS);
        }

        private static int GetFPSForState(GameState state)
        {
            switch (state)
            {
                case GameState.Active:
                    return activeFPS;
                case GameState.Idle:
                    return idleFPS;
                case GameState.Paused:
                    return pausedFPS;
                default:
                    return 60;
            }
        }

        private static void ApplyFPS(int fps)
        {
            if (Application.targetFrameRate != fps)
            {
                Application.targetFrameRate = fps;
                QualitySettings.vSyncCount = 0;
                Debug.Log($"[AdaptiveFPS] State: {currentState}, Target FPS: {fps}");
            }
        }

        // Пример получения текущего состояния (если нужно)
        public static GameState GetCurrentState()
        {
            return currentState;
        }
    }

}