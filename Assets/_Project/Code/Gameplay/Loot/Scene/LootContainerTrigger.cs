
using System.Collections.Generic;
using Galactic1.Code.Gameplay.Units;
using Galactic1.RaidLoot.Authoring;
using Galactic1.RaidLoot.Enums;
using Galactic1.RaidLoot.Runtime;
using Galactic1.RaidLoot.Services;
using UnityEngine;

namespace Galactic1.RaidLoot.Scene
{
    /// <summary>
    /// Физический коллайдер + таймер открытия.
    ///
    /// Enter → runtime.SetInProximity(true)  + старт таймера
    /// Exit  → runtime.SetInProximity(false) + сброс таймера
    /// Timer elapsed → openService.RequestOpen()
    ///
    /// Не управляет визуалами. Не хранит состояние контейнера.
    /// </summary>
    [RequireComponent(typeof(SphereCollider))]
    public sealed class LootContainerTrigger : MonoBehaviour
    {

        private LootContainerOpenService _openService;
        private LootContainerRuntime _runtime; // ← для proximity сигнала
        private string _runtimeId;

        private const string PlayerTag = "Player";

        private float _timerDelay;
        private float _elapsed;
        private bool _timerRunning;
        
        private readonly HashSet<SurvivorInstance> _unitsInside = new();

        /// <summary>
        /// Вызывается LootContainerSceneLifecycleSystem после BuildAll.
        /// </summary>
        public void Init(
            LootContainerOpenService openService,
            LootContainerRuntime runtime,
            float timerDelay)
        {
            _openService = openService;
            _runtime = runtime;
            _timerDelay = timerDelay;
            _runtimeId = runtime.Id;
        }

        // ── Unity trigger callbacks ──────────────────────────────────────────

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag(PlayerTag))
                return;

            if (!_unitsInside.Add(other.GetComponent<SurvivorInstance>()))
                return;

            if (_unitsInside.Count == 1)
            {
                _runtime.SetInProximity(true);
                _runtime.SetState(ContainerState.Opening);

                StartTimer();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag(PlayerTag))
                return;

            _unitsInside.Remove(other.GetComponent<SurvivorInstance>());

            if (_unitsInside.Count == 0)
            {
                _runtime.SetInProximity(false);

                CancelTimer();
            }
        }

        // ── Timer ────────────────────────────────────────────────────────────

        private void Update()
        {
            if (!_timerRunning) return;

            _elapsed += Time.deltaTime;

            // Publish progress to runtime so View can show progress bar
            var progress = Mathf.Clamp01(_elapsed / _timerDelay);
            _runtime.SetOpenProgress(progress);

            if (_elapsed >= _timerDelay)
            {
                _timerRunning = false;
                GetComponent<SphereCollider>().enabled = false;
                _openService.RequestOpen(_runtimeId);
            }
        }

        private void StartTimer()
        {
            _elapsed = 0f;
            _runtime.SetOpenProgress(0f);
            _timerRunning = true;
        }

        private void CancelTimer()
        {
            _timerRunning = false;
            _elapsed = 0f;
            _runtime?.SetOpenProgress(0f);
        }
    }
}