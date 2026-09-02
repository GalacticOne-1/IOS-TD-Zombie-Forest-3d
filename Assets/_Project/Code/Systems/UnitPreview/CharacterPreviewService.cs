
using System;
using System.Collections;
using System.Collections.Generic;
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Code.Gameplay.Units;
using Galactic1.Meta.Configs.Recruitment;
using Pathfinding;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Galactic1.UI.CharacterPreview
{
    /// <summary>
    /// Сервис рендера портретов и полного роста персонажей.
    /// Одна камера рендерит все запросы по очереди.
    /// Каждый запрос получает свою RenderTexture.
    /// </summary>
    public sealed class CharacterPreviewService : MonoBehaviour, IGameService
    {
        [Header("Camera")] [SerializeField] private Camera previewCamera;
        [SerializeField] private Transform modelAnchor;

        [Header("Portrait settings")] [SerializeField]
        private Vector3 portraitCameraOffset = new(0f, 1.6f, -1.2f);

        [SerializeField] private Vector3 portraitLookOffset = new(0f, 1.5f, 0f);
        [SerializeField] private Vector2Int portraitTextureSize = new(256, 256);

        [Header("Full body settings")] [SerializeField]
        private Vector3 fullBodyCameraOffset = new(0f, 1.0f, -2.5f);

        [SerializeField] private Vector3 fullBodyLookOffset = new(0f, 1.0f, 0f);
        [SerializeField] private Vector2Int fullBodyTextureSize = new(256, 512);

        private readonly Queue<RenderRequest> queue = new();
        private bool isRendering;
        private GameObject previewContainer;
        private GameObject currentPreviewModel;

        // =========================================================
        // INIT
        // =========================================================

        private void Awake()
        {
            // === находим камеру и точку для юнита
            var go = FindObjectsByType<UIPreviewTag>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

            foreach (var g in go)
            {
                if (g.GetComponent<Camera>())
                {
                    previewContainer = go[0].transform.parent.gameObject;
                    previewCamera = g.GetComponent<Camera>();
                    
                    previewCamera.renderingPath = RenderingPath.Forward;
                    var cameraData = previewCamera.GetUniversalAdditionalCameraData();
                    cameraData.renderShadows = false;
                }
                else
                    modelAnchor = g.GetComponent<Transform>();
            }
            
        }


        // =========================================================
        // PUBLIC API
        // =========================================================

        /// <summary>
        /// Запрашивает рендер персонажа.
        /// Результат — RenderTexture — придёт в onComplete.
        /// Вызывающий отвечает за Release текстуры через CharacterPortraitHandle.
        /// </summary>
        public CharacterPortraitHandle Request(
            (string prefabPath, UnitIdentityPoolConfig.ArchetypePrefabEntry variant) survEntry,
            CharacterRenderMode mode,
            Action<RenderTexture> onComplete)
        {
            var size = mode == CharacterRenderMode.Portrait
                ? portraitTextureSize
                : fullBodyTextureSize;

            var rt = new RenderTexture(size.x, size.y, 16, RenderTextureFormat.ARGB32);

            var request = new RenderRequest
            {
                PrefabPath = survEntry.prefabPath,
                SurvivorVariant = survEntry.variant.AppearanceId,
                Mode = mode,
                Target = rt,
                OnComplete = onComplete
            };

            queue.Enqueue(request);

            if (!isRendering)
                StartCoroutine(ProcessQueue());

            return new CharacterPortraitHandle(rt);
        }

        // =========================================================
        // PRIVATE
        // =========================================================

        private IEnumerator ProcessQueue()
        {
            isRendering = true;
            previewContainer.gameObject.SetActive(true);

            while (queue.Count > 0)
            {
                var request = queue.Dequeue();
                yield return StartCoroutine(RenderOne(request));
            }

            isRendering = false;
            previewContainer.gameObject.SetActive(false);
        }

        private IEnumerator RenderOne(RenderRequest request)
        {
            currentPreviewModel = $"{AppConstants.PATH_PLAYER}{request.PrefabPath}".CreateGO(null);
            currentPreviewModel.transform.SetPositionAndRotation(modelAnchor.position, modelAnchor.rotation);
            currentPreviewModel.GetComponent<CharacterAppearanceController>().Apply(request.SurvivorVariant);

            if (currentPreviewModel.TryGetComponent<AIPath>(out var c))
                c.enabled = false;

            SetLayerRecursive(currentPreviewModel, LayerMask.NameToLayer("UIPreview"));

            bool isPortrait = request.Mode == CharacterRenderMode.Portrait;

            previewCamera.transform.position = modelAnchor.position +
                                               (isPortrait ? portraitCameraOffset : fullBodyCameraOffset);
            previewCamera.transform.LookAt(modelAnchor.position +
                                           (isPortrait ? portraitLookOffset : fullBodyLookOffset));

            previewCamera.targetTexture = request.Target;
            previewCamera.enabled       = true;

            yield return new WaitForEndOfFrame();

            request.OnComplete?.Invoke(request.Target);
            previewCamera.enabled = false;

            // Сдвигаем за пределы камеры перед удалением
            currentPreviewModel.transform.position = modelAnchor.position + Vector3.right * 25f;

            yield return null; // ждём кадр — камера уже выключена, модель вне кадра

            Destroy(currentPreviewModel);
        }

        private static void SetLayerRecursive(GameObject go, int layer)
        {
            go.layer = layer;

            foreach (var r in go.GetComponentsInChildren<Renderer>())
            {
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                r.receiveShadows = false;
            }

            foreach (Transform child in go.transform)
                SetLayerRecursive(child.gameObject, layer);
        }
        
        public void CancelAll()
        {
            StopAllCoroutines();
            queue.Clear();
            isRendering = false;

            // Выключаем камеру и удаляем текущую модель если была
            previewCamera.enabled = false;
            if (currentPreviewModel != null)
            {
                DestroyImmediate(currentPreviewModel);
                currentPreviewModel = null;
            }
        }

        // =========================================================
        // NESTED
        // =========================================================

        private sealed class RenderRequest
        {
            public string PrefabPath;
            public AppearanceId SurvivorVariant = null;
            public CharacterRenderMode Mode;
            public RenderTexture Target;
            public Action<RenderTexture> OnComplete;
        }
    }
}