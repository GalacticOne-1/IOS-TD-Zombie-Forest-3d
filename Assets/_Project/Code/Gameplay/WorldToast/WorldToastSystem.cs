using System.Collections.Generic;
using Galactic1.Code.Cameras;
using UnityEngine;

namespace Galactic1.Code.UI.World
{
    /// <summary>
    /// Сервис тостов в 3D сцене.
    /// Один на сцену. Регистрируется в ServiceLocator.
    /// Использует пул — без аллокаций на каждый тост.
    /// </summary>
    public sealed class WorldToastSystem : MonoBehaviour, IGameService
    {
        [SerializeField] private WorldToastItem _prefab;
        [SerializeField] private int _poolSize = 20;
        [SerializeField] private float _yOffset = 2.2f; // над головой

        private readonly Queue<WorldToastItem> _pool = new();

        
        
        
        // =========================
        // Init
        // =========================
        public void Prewarm()
        {
            var camera = Object.FindAnyObjectByType<CameraController>(FindObjectsInactive.Include)
                .GetComponentInChildren<Camera>();
            
            for (int i = 0; i < _poolSize; i++)
            {
                var item = Instantiate(_prefab, transform);
                item.Setup(camera.transform);
                _pool.Enqueue(item);
            }
        }


        // =========================
        // API
        // =========================
        public void Show(Vector3 position, string text, Color color)
        {
            if (_pool.Count == 0) return; // пул исчерпан — пропускаем

            var item = _pool.Dequeue();
            item.Play(
                position + Vector3.up * _yOffset,
                text,
                color,
                ReturnToPool);
        }

        // Shortcuts
        public void ShowDamage(Vector3 position, float amount) =>
            Show(position, $"-{amount:0}", new Color(1f, 0.25f, 0.25f));

        public void ShowHeal(Vector3 position, float amount) =>
            Show(position, $"+{amount:0}", new Color(0.25f, 1f, 0.4f));

        public void ShowStatus(Vector3 position, string text) =>
            Show(position, text, new Color(1f, 0.85f, 0.2f));
        
        public void ShowMessage(Vector3 position, string text) =>
            Show(position, text, new Color(1, 1, 1));

        public void ShowAmmo(Vector3 position, string text) =>
            Show(position, text, new Color(0.6f, 0.8f, 1f));

        // =========================
        // Pool
        // =========================
        private void ReturnToPool(WorldToastItem item) =>
            _pool.Enqueue(item);
    }
}