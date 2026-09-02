
using Galactic1.RaidLoot.Definitions;
using Galactic1.RaidLoot.Enums;
using Galactic1.RaidLoot.Runtime;
using Galactic1.Systems;
using UnityEngine;
using UnityEngine.UI;

namespace Galactic1.RaidLoot.Scene
{
    /// <summary>
    /// Scene-отображение LootContainerRuntime.
    ///
    /// Ответственность:
    ///   - Инстанцировать визуальные префабы из LootContainerVisualConfig
    ///   - Маппить ContainerState → визуал
    ///   - Маппить proximity → LootContainerHighlightView
    ///   - Показывать progress bar только во время Opening
    ///
    /// Не хранит состояние. Реагирует исключительно на Runtime-события.
    /// </summary>
    public sealed class LootContainerView : MonoBehaviour
    {
        [Header("Visual root")] [SerializeField]
        private Transform _visualRoot;



        // ── Instantiated visuals ─────────────────────────────────────────────
        private LootContainerHighlightView _highlight;
        private GameObject _closedInstance;
        private LootContainerHighlightMarker _highlightMarker;


        private LootContainerRuntime _runtime;

        // ── Init ─────────────────────────────────────────────────────────────

        public void Init(LootContainerRuntime runtime, LootContainerVisualConfig visual)
        {
            _runtime = runtime;

            BuildVisuals(visual);
            _highlight?.SetDetected();

            _highlightMarker = GetComponentInChildren<LootContainerHighlightMarker>();

            // Подписки на Runtime — View только читает события
            _runtime.OnStateChanged += OnStateChanged;
            _runtime.OnProximityChanged += OnProximityChanged;
            _runtime.OnOpenProgressChanged += OnOpenProgressChanged;

            // Начальное состояние
            ApplyState(_runtime.State);
        }

        private void OnDestroy()
        {
            if (_runtime == null) return;
            _runtime.OnStateChanged -= OnStateChanged;
            _runtime.OnProximityChanged -= OnProximityChanged;
            _runtime.OnOpenProgressChanged -= OnOpenProgressChanged;
        }

        // ── Visual construction ───────────────────────────────────────────────

        private void BuildVisuals(LootContainerVisualConfig visual)
        {
            if (visual == null)
            {
                Debug.LogError($"[LootContainerView] VisualConfig == null на {name}.");
                return;
            }

            var root = _visualRoot != null ? _visualRoot : transform;

            _closedInstance = InstantiatePrefab(visual.ClosedVisualPrefab, root);
            
            _highlight = _closedInstance.GetComponent<LootContainerHighlightView>();
        }

        private static GameObject InstantiatePrefab(GameObject prefab, Transform parent)
        {
            if (prefab == null) return null;
            var obj = prefab.CreateGO(parent);
            obj.transform.localPosition = Vector3.zero;
            obj.SetActive(true);
            return obj;
        }

        // ── State mapping ────────────────────────────────────────────────────

        private void OnStateChanged(ContainerState state) => ApplyState(state);

        private void ApplyState(ContainerState state)
        {
            switch (state)
            {
                case ContainerState.Closed:
                    break;

                case ContainerState.Opening:

                    _highlight?.SetOpening();

                    break;

                case ContainerState.Open:

                    _highlight?.SetIdle();
                    
                    if(_highlightMarker) 
                        _highlightMarker.gameObject.SetActive(false);
                    
                    AudioService.Play(AudioService.Environment.Command.ChestOpen);

                    break;

                case ContainerState.FullyLooted:

                    _highlight?.SetIdle();
                    
                    if(_highlightMarker) 
                        _highlightMarker.gameObject.SetActive(false);

                    break;
            }
        }

        // ── Proximity mapping ────────────────────────────────────────────────

        private void OnProximityChanged(bool inProximity)
        {
            if (_runtime.IsOpened)
                return;

            if (inProximity)
                _highlight?.SetInRange();
            else
                _highlight?.SetDetected();
        }

        // ── Progress mapping ─────────────────────────────────────────────────

        private void OnOpenProgressChanged(float progress)
        {
            
        }

        // ── Helpers ──────────────────────────────────────────────────────────



        private static void SetActive(GameObject obj, bool active)
        {
            if (obj != null) obj.SetActive(active);
        }

        public Vector3 GetFeedbackAnchor() => transform.position + Vector3.up;
    }
}