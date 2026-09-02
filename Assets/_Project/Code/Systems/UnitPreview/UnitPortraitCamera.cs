
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Galactic1.UI.CharacterPreview
{
    /// <summary>
    /// Единственная Preview камера.
    /// Рендерит карточки по очереди — каждую в свою RenderTexture.
    /// После рендера камера переходит к следующей карточке.
    /// </summary>
    public sealed class UnitPortraitCamera : MonoBehaviour, IGameService
    {
        [SerializeField] private Camera      previewCamera;
        [SerializeField] private Transform   modelAnchor;    // модель ставится сюда
        [SerializeField] private Vector3     cameraOffset = new(0f, 1.5f, -2f);
        [SerializeField] private Vector2Int  textureSize  = new(256, 256);

        private readonly Queue<RenderRequest> queue = new();
        private bool                          isRendering;

        private void Awake()
        {
            ServiceLocator.Current.Register(this);
            previewCamera.cullingMask     = LayerMask.GetMask("UIPreview");
            previewCamera.clearFlags      = CameraClearFlags.SolidColor;
            previewCamera.backgroundColor = Color.clear;
            previewCamera.enabled         = false;
        }

        private void OnDestroy()
        {
            ServiceLocator.Current.Unregister<UnitPortraitCamera>();
        }

        /// <summary>
        /// Запрашивает рендер портрета.
        /// Результат придёт в onComplete(RenderTexture).
        /// </summary>
        public void RequestPortrait(
            GameObject                    prefab,
            System.Action<RenderTexture>  onComplete)
        {
            var rt = new RenderTexture(
                textureSize.x, textureSize.y, 16,
                RenderTextureFormat.ARGB32);

            queue.Enqueue(new RenderRequest
            {
                Prefab     = prefab,
                Target     = rt,
                OnComplete = onComplete
            });

            if (!isRendering)
                StartCoroutine(ProcessQueue());
        }

        // =========================================================
        // PRIVATE
        // =========================================================

        private IEnumerator ProcessQueue()
        {
            isRendering = true;

            while (queue.Count > 0)
            {
                var request = queue.Dequeue();
                yield return StartCoroutine(RenderOne(request));
            }

            isRendering = false;
            previewCamera.enabled = false;
        }

        private IEnumerator RenderOne(RenderRequest request)
        {
            // Создаём модель
            var model = Instantiate(
                request.Prefab,
                modelAnchor.position,
                modelAnchor.rotation);
            SetLayerRecursive(model, LayerMask.NameToLayer("UIPreview"));

            // Настраиваем камеру
            previewCamera.targetTexture = request.Target;
            previewCamera.transform.position =
                modelAnchor.position + cameraOffset;
            previewCamera.transform.LookAt(
                modelAnchor.position + Vector3.up * 1f);
            previewCamera.enabled = true;

            // Ждём один кадр чтобы камера отрендерила
            yield return new WaitForEndOfFrame();

            // Передаём текстуру
            request.OnComplete?.Invoke(request.Target);

            // Чистим
            Destroy(model);
            previewCamera.enabled = false;
        }

        private static void SetLayerRecursive(GameObject go, int layer)
        {
            go.layer = layer;
            foreach (Transform child in go.transform)
                SetLayerRecursive(child.gameObject, layer);
        }

        // =========================================================
        // NESTED
        // =========================================================

        private sealed class RenderRequest
        {
            public GameObject                   Prefab;
            public RenderTexture                Target;
            public System.Action<RenderTexture> OnComplete;
        }
    }
}