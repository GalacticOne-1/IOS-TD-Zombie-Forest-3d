
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Code.Gameplay.Animation;
using Galactic1.Code.Gameplay.Equipment_Preview;
using Galactic1.Code.Gameplay.Units;
using Galactic1.Code.Gameplay.Weapon.Animation;
using Galactic1.Code.Systems.Raid;
using Galactic1.Code.UI.Equipment;
using Galactic1.Configs;
using Pathfinding;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace Galactic1.UI.CharacterPreview
{
    /// <summary>
    /// Рендерит 3D модель персонажа в RenderTexture → RawImage.
    /// Камера смотрит только на Layer UIPreview.
    /// Модель создаётся в фиксированной позиции вне игровой сцены.
    /// </summary>
    public abstract class UIPreviewRendererBase : MonoBehaviour, IGameService
    {
        [Header("Rendering")] 
        [SerializeField] protected Camera previewCamera;
        [SerializeField] protected Vector2Int textureSize = new(512, 512);

        [Header("Model Position")] [SerializeField]
        protected Transform modelAnchor; // куда ставить модель


        protected GameObject previewContainer;
        protected RenderTexture renderTexture;
        protected GameObject currentModel;





        public void Initialize()
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
                    // Отключаем тени для preview камеры через CameraData (URP)
                    var cameraData = previewCamera.GetUniversalAdditionalCameraData();
                    cameraData.renderShadows = false;
                }
                else
                    modelAnchor = g.GetComponent<Transform>();
            }

            // Создаём RenderTexture
            renderTexture = new RenderTexture(
                textureSize.x,
                textureSize.y,
                16,
                RenderTextureFormat.ARGB32);

            // Камера рендерит только слой UIPreview
            previewCamera.cullingMask = LayerMask.GetMask("UIPreview");
            previewCamera.clearFlags = CameraClearFlags.SolidColor;
            previewCamera.backgroundColor = Color.clear;
        }
        
        
        protected void ApplyConfig(UIPreviewConfig config)
        {
            if (config == null)
                return;

            previewCamera.fieldOfView = config.fieldOfView;

            previewCamera.transform.position =
                modelAnchor.position + config.cameraOffset;

            previewCamera.transform.LookAt(
                modelAnchor.position + config.lookOffset);

            if (currentModel != null)
            {
                currentModel.transform.localPosition += config.modelOffset;
                currentModel.transform.localRotation =
                    Quaternion.Euler(config.modelRotation);
                currentModel.transform.localScale =
                    Vector3.one * config.scale;
            }
        }

        /// <summary>
        /// Показывает модель персонажа.
        /// Предыдущая модель уничтожается.
        /// </summary>
        // public void Show(GameObject prefab)
        // {
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
        //     // Камера смотрит на модель
        //     previewCamera.transform.position = modelAnchor.position + cameraOffset;
        //     previewCamera.transform.LookAt(modelAnchor.position + Vector3.up * 1f);
        // }
        
        public virtual void Show(
            RawImage displayTarget,
            string prefabId,
            AppearanceId appearanceId,
            UIPreviewConfig config,
            IUnitRuntime unitRuntime = null)
        {
            if (displayTarget == null || string.IsNullOrEmpty(prefabId)) 
                return;
    
            Clear(displayTarget);
            previewContainer.SetActive(true);
            previewCamera.enabled = true;
            previewCamera.targetTexture = renderTexture;
            displayTarget.texture = renderTexture;
            displayTarget.color = new Color(1, 1, 1, 1);

            currentModel = $"{prefabId}".CreateGO(null);
            currentModel.transform.SetPositionAndRotation(modelAnchor.position, modelAnchor.rotation);
            currentModel.name = "Preview model";
            
            if (currentModel.TryGetComponent<CharacterAppearanceController>(out var component))
                component.Apply(appearanceId);
            
            Destroy(currentModel.GetComponent<Rigidbody>());
            if (currentModel.TryGetComponent<AIPath>(out var c))
                c.enabled = false;

            
            PreviewEquipment(unitRuntime);

            ServiceLocator.Current.Get<CoroutineController>().Coroutine_wait(() =>
                SetLayerRecursive(currentModel, LayerMask.NameToLayer("UIPreview")));

            ApplyConfig(config);
        }

        void PreviewEquipment(IUnitRuntime unitRuntime)
        {
            // ✅ привязываем экипировку если передана
            if (unitRuntime != null)
            {
                var configProvider = ServiceLocator.Current.Get<ConfigProvider>();
                var equipmentProvider = unitRuntime.GetEquipmentService_Preview();
                currentModel.GetComponent<WeaponAnimSwitcher>().AnimLibrary = configProvider.Get<WeaponAnimLibrary>();
                
                // animation
                currentModel.GetComponentInChildren<UnitAnimationController>()
                    .Initialize(configProvider.Get<PlayerAnimConfig>());
                currentModel.GetComponentInChildren<PlayerWeaponAnimationModule>().Initialize();
                
                var equipmentContainer = currentModel.GetComponent<EquipmentContainer>();
                equipmentContainer.BindSource(
                    equipmentProvider, 
                    new EquipmentVisualHandler_Preview(currentModel));
                equipmentProvider.RestoreEquipmentFromInventory();
            }
        }

        // public  virtual void Show(
        //     RawImage displayTarget,
        //     GameObject prefab,
        //     UIPreviewConfig config)
        // {
        //     if (displayTarget == null || prefab == null) 
        //         return;
        //     
        //     Clear(displayTarget);
        //     previewContainer.SetActive(true);
        //     previewCamera.enabled = true;
        //     previewCamera.targetTexture = renderTexture;
        //     displayTarget.texture = renderTexture;
        //     displayTarget.color = new Color(1, 1, 1, 1);
        //
        //     currentModel = Instantiate(prefab, modelAnchor.position, modelAnchor.rotation);
        //     Destroy(currentModel.GetComponent<Rigidbody>());
        //     if (currentModel.TryGetComponent<AIPath>(out var c))
        //         c.enabled = false;
        //     SetLayerRecursive(currentModel, LayerMask.NameToLayer("UIPreview"));
        //
        //     ApplyConfig(config);
        // }

        public void Clear(RawImage displayTarget)
        {
            if (currentModel != null)
            {
                Destroy(currentModel);
                currentModel = null;
            }
            
            // отвязываем текстуру от UI
            if (displayTarget != null)
            {
                displayTarget.color = new Color(1, 1, 1, 0);
            }

            // Очищаем текстуру — не освобождаем, просто заливаем прозрачным
            if (renderTexture != null)
            {
                var prev = RenderTexture.active;
                RenderTexture.active = renderTexture;
                GL.Clear(true, true, Color.clear);
                RenderTexture.active = prev;
            }

            previewContainer.SetActive(false);
        }

        private void OnDestroy()
        {
            if (currentModel != null)
                Destroy(currentModel);

            if (renderTexture != null)
            {
                renderTexture.Release();
                renderTexture = null;
            }
        }

        protected static void SetLayerRecursive(GameObject go, int layer)
        {
            if(go)
            {
                go.layer = layer;
                foreach (Transform child in go.transform)
                    SetLayerRecursive(child.gameObject, layer);
            }
        }
    }
}