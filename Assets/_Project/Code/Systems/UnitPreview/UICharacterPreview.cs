
using UnityEngine;
using UnityEngine.UI;

namespace Galactic1.UI.CharacterPreview
{
    /// <summary>
    /// Рендерит 3D модель персонажа в RenderTexture → RawImage.
    /// Камера смотрит только на Layer UIPreview.
    /// Модель создаётся в фиксированной позиции вне игровой сцены.
    /// </summary>
    public sealed class UICharacterPreview : UIPreviewRendererBase
    {
        // [Header("Rendering")] [SerializeField] private Camera previewCamera;
        // [SerializeField] private RawImage displayTarget; // UI элемент
        // [SerializeField] private Vector2Int textureSize = new(512, 512);
        //
        // [Header("Model Position")] [SerializeField]
        // private Transform modelAnchor; // куда ставить модель
        //
        // [SerializeField] private Vector3 cameraOffset = new(0f, 1.5f, -2f);
        //
        // private GameObject previewContainer;
        // private RenderTexture renderTexture;
        // private GameObject currentModel;





        // public void Initialize()
        // {
        //     // === находим камеру и точку для юнита
        //     var go = FindObjectsByType<UnitPreviewTag>(
        //         FindObjectsInactive.Include,
        //         FindObjectsSortMode.None);
        //
        //     foreach (var g in go)
        //     {
        //         if (g.GetComponent<Camera>())
        //         {
        //             previewContainer = go[0].transform.parent.gameObject;
        //             previewCamera = g.GetComponent<Camera>();
        //
        //             previewCamera.renderingPath = RenderingPath.Forward;
        //             // Отключаем тени для preview камеры через CameraData (URP)
        //             var cameraData = previewCamera.GetUniversalAdditionalCameraData();
        //             cameraData.renderShadows = false;
        //         }
        //         else
        //             modelAnchor = g.GetComponent<Transform>();
        //     }
        //
        //     // Создаём RenderTexture
        //     renderTexture = new RenderTexture(
        //         textureSize.x,
        //         textureSize.y,
        //         16,
        //         RenderTextureFormat.ARGB32);
        //
        //     // Камера рендерит только слой UIPreview
        //     previewCamera.cullingMask = LayerMask.GetMask("UIPreview");
        //     previewCamera.clearFlags = CameraClearFlags.SolidColor;
        //     previewCamera.backgroundColor = Color.clear;
        // }

        /// <summary>
        /// Показывает модель персонажа.
        /// Предыдущая модель уничтожается.
        /// </summary>
        // protected override void ShowInternal(
        //     RawImage displayTarget, 
        //     GameObject prefab,
        //     UIPreviewConfig config)
        // {
        //     if (displayTarget == null || prefab == null) 
        //         return;
        //     
        //     Clear();
        //     previewContainer.SetActive(true);
        //     previewCamera.enabled = true;
        //     previewCamera.targetTexture = renderTexture;
        //     displayTarget.texture = renderTexture;
        //
        //     currentModel = Instantiate(prefab, modelAnchor.position, modelAnchor.rotation);
        //     Destroy(currentModel.GetComponent<Rigidbody>());
        //     SetLayerRecursive(currentModel, LayerMask.NameToLayer("UIPreview"));
        //
        //     ApplyConfig(config);
        // }

        // public void Clear()
        // {
        //     if (currentModel != null)
        //     {
        //         Destroy(currentModel);
        //         currentModel = null;
        //     }
        //
        //     // Очищаем текстуру — не освобождаем, просто заливаем прозрачным
        //     if (renderTexture != null)
        //     {
        //         var prev = RenderTexture.active;
        //         RenderTexture.active = renderTexture;
        //         GL.Clear(true, true, Color.clear);
        //         RenderTexture.active = prev;
        //     }
        //
        //     previewContainer.SetActive(false);
        // }
        //
        // private void OnDestroy()
        // {
        //     if (currentModel != null)
        //         Destroy(currentModel);
        //
        //     if (renderTexture != null)
        //     {
        //         renderTexture.Release();
        //         renderTexture = null;
        //     }
        // }
        //
        // private static void SetLayerRecursive(GameObject go, int layer)
        // {
        //     go.layer = layer;
        //     foreach (Transform child in go.transform)
        //         SetLayerRecursive(child.gameObject, layer);
        // }
    }
}