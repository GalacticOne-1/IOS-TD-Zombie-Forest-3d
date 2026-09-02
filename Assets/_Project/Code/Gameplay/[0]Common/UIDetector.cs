
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace Galactic1.Code.Systems
{
    public class UIDetector : MonoBehaviour, IGameService
    {
        [Header("Detection BasicSettings")]
        [SerializeField] private bool enableDetection = true;
        [SerializeField] private bool logDetectedUI = false;
        [SerializeField] private LayerMask uiLayerMask = -1;
        
        [Header("Canvas BasicSettings")]
        [SerializeField] private List<Canvas> canvasesToCheck = new List<Canvas>();
        [SerializeField] private bool autoFindCanvases = true;
        
        private Camera uiCamera;
        private EventSystem eventSystem;
        private List<GraphicRaycaster> graphicRaycasters = new List<GraphicRaycaster>();
        
        // Events
        public event Action<List<RaycastResult>> OnUIDetected;
        public event Action OnNoUIDetected;
        public event Action<bool> OnUIStateChanged; // true если UI найден, false если нет
        
        // Properties
        public bool IsPointerOverUI { get; private set; }
        public List<RaycastResult> LastDetectedUI { get; private set; } = new List<RaycastResult>();
        
        private void Start()
        {
            Initialize();
        }
        
        private void Initialize()
        {
            // Получаем EventSystem
            eventSystem = EventSystem.current;
            if (eventSystem == null)
            {
                Debug.LogWarning("UIDetector: EventSystem not found in scene!");
                return;
            }
            
            // Получаем UI камеру
            uiCamera = Camera.main;
            
            // Автоматически находим Canvas'ы если включено
            if (autoFindCanvases)
            {
                FindAllCanvases();
            }
            
            // Получаем GraphicRaycaster'ы для каждого Canvas
            SetupGraphicRaycasters();
        }
        
        private void FindAllCanvases()
        {
            canvasesToCheck.Clear();
            Canvas[] allCanvases = FindObjectsOfType<Canvas>();
            
            foreach (Canvas canvas in allCanvases)
            {
                // Добавляем только активные Canvas'ы
                if (canvas.gameObject.activeInHierarchy)
                {
                    canvasesToCheck.Add(canvas);
                }
            }
            
            if (logDetectedUI)
            {
                Debug.Log($"UIDetector: Found {canvasesToCheck.Count} active canvases");
            }
        }
        
        private void SetupGraphicRaycasters()
        {
            graphicRaycasters.Clear();
            
            foreach (Canvas canvas in canvasesToCheck)
            {
                GraphicRaycaster raycaster = canvas.GetComponent<GraphicRaycaster>();
                if (raycaster != null)
                {
                    graphicRaycasters.Add(raycaster);
                }
                else if (logDetectedUI)
                {
                    Debug.LogWarning($"UIDetector: Canvas '{canvas.name}' doesn't have GraphicRaycaster component!");
                }
            }
        }
        
        private void Update()
        {
            if (!enableDetection || eventSystem == null)
                return;
                
            // Проверяем нажатие левой кнопки мыши
            if (Input.GetMouseButtonDown(0))
            {
                CheckUIUnderCursor();
            }
            
            // Постоянно отслеживаем состояние UI под курсором
            bool wasOverUI = IsPointerOverUI;
            IsPointerOverUI = IsPointerCurrentlyOverUI();
            
            // Вызываем событие при изменении состояния
            if (wasOverUI != IsPointerOverUI)
            {
                OnUIStateChanged?.Invoke(IsPointerOverUI);
            }
        }
        
        /// <summary>
        /// Проверяет наличие UI под курсором при нажатии левой кнопки мыши
        /// </summary>
        public void CheckUIUnderCursor()
        {
            if (!enableDetection)
                return;
                
            List<RaycastResult> results = GetUIUnderCursor();
            LastDetectedUI = results;
            
            if (results.Count > 0)
            {
                if (logDetectedUI)
                {
                    LogDetectedUI(results);
                }
                
                OnUIDetected?.Invoke(results);
            }
            else
            {
                if (logDetectedUI)
                {
                    Debug.Log("UIDetector: No UI elements found under cursor");
                }
                
                OnNoUIDetected?.Invoke();
            }
        }
        
        /// <summary>
        /// Получает список UI элементов под курсором
        /// </summary>
        private List<RaycastResult> GetUIUnderCursor()
        {
            // Получаем позицию мыши в экранных координатах
            Vector2 mousePosition = Input.mousePosition;

            // Создаём Raycast в позицию мыши
            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                position = mousePosition
            };

            // Получаем список всех объектов, с которыми пересекается луч
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);
            
            // if (eventSystem == null)
            //     return results;
            //
            // // Создаем PointerEventData для текущей позиции мыши
            // PointerEventData pointerData = new PointerEventData(eventSystem)
            // {
            //     position = Input.mousePosition
            // };
            //
            // // Выполняем raycast для каждого GraphicRaycaster
            // foreach (GraphicRaycaster raycaster in graphicRaycasters)
            // {
            //     if (raycaster != null && raycaster.enabled)
            //     {
            //         List<RaycastResult> raycastResults = new List<RaycastResult>();
            //         raycaster.Raycast(pointerData, raycastResults);
            //         
            //         // Фильтруем по слоям если нужно
            //         foreach (RaycastResult result in raycastResults)
            //         {
            //             if (IsInLayerMask(result.gameObject.layer, uiLayerMask))
            //             {
            //                 results.Add(result);
            //             }
            //         }
            //     }
            // }
            
            return results;
        }
        
        /// <summary>
        /// Быстрая проверка, находится ли курсор над UI (использует встроенный метод Unity)
        /// </summary>
        public bool IsPointerCurrentlyOverUI()
        {
            if (eventSystem == null)
                return false;
                
            return EventSystem.current.IsPointerOverGameObject();
        }
        
        /// <summary>
        /// Проверяет, находится ли курсор над конкретным UI элементом
        /// </summary>
        public bool IsPointerOverSpecificUI(GameObject uiElement)
        {
            List<RaycastResult> results = GetUIUnderCursor();
            
            foreach (RaycastResult result in results)
            {
                if (result.gameObject == uiElement)
                    return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Проверяет, находится ли курсор над UI элементом с определенным тегом
        /// </summary>
        public bool IsPointerOverUIWithTag(string tag)
        {
            List<RaycastResult> results = GetUIUnderCursor();
            
            foreach (RaycastResult result in results)
            {
                
                if (result.gameObject.CompareTag(tag))
                    return true;
            }
            
            return false;
        }
        
        
        /// <summary>
        /// Получает все UI элементы под курсором с определенным компонентом
        /// </summary>
        public List<T> GetUIComponentsUnderCursor<T>() where T : Component
        {
            List<T> components = new List<T>();
            List<RaycastResult> results = GetUIUnderCursor();
            
            foreach (RaycastResult result in results)
            {
                T component = result.gameObject.GetComponent<T>();
                if (component != null)
                {
                    components.Add(component);
                }
            }
            
            return components;
        }
        
        private void LogDetectedUI(List<RaycastResult> results)
        {
            Debug.Log($"UIDetector: Found {results.Count} UI elements under cursor:");
            
            for (int i = 0; i < results.Count; i++)
            {
                RaycastResult result = results[i];
                string hierarchy = GetGameObjectHierarchy(result.gameObject);
                Debug.Log($"  [{i}] {hierarchy} (Distance: {result.distance:F2})");
            }
        }
        
        private string GetGameObjectHierarchy(GameObject obj)
        {
            string path = obj.name;
            Transform parent = obj.transform.parent;
            
            while (parent != null)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }
            
            return path;
        }
        
        private bool IsInLayerMask(int layer, LayerMask layerMask)
        {
            return (layerMask.value & (1 << layer)) != 0;
        }
        
        /// <summary>
        /// Обновляет список Canvas'ов для проверки
        /// </summary>
        public void RefreshCanvases()
        {
            if (autoFindCanvases)
            {
                FindAllCanvases();
            }
            SetupGraphicRaycasters();
        }
        
        /// <summary>
        /// Добавляет Canvas для проверки
        /// </summary>
        public void AddCanvas(Canvas canvas)
        {
            if (canvas != null && !canvasesToCheck.Contains(canvas))
            {
                canvasesToCheck.Add(canvas);
                SetupGraphicRaycasters();
            }
        }
        
        /// <summary>
        /// Удаляет Canvas из списка проверки
        /// </summary>
        public void RemoveCanvas(Canvas canvas)
        {
            if (canvasesToCheck.Contains(canvas))
            {
                canvasesToCheck.Remove(canvas);
                SetupGraphicRaycasters();
            }
        }
        
        /// <summary>
        /// Включает/выключает детекцию UI
        /// </summary>
        public void SetDetectionEnabled(bool enabled)
        {
            enableDetection = enabled;
        }
        
        private void OnDestroy()
        {
            // Очищаем события
            OnUIDetected = null;
            OnNoUIDetected = null;
            OnUIStateChanged = null;
        }
        
        // Публичные методы для внешнего использования
        public void ForceCheckUI()
        {
            CheckUIUnderCursor();
        }
        
        public bool HasUIUnderCursor()
        {
            return GetUIUnderCursor().Count > 0;
        }
        
        public int GetUICountUnderCursor()
        {
            return GetUIUnderCursor().Count;
        }
    }
}
