using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Galactic1.UI.Core
{
    public class UIPopupManager : MonoBehaviour
    {
        [Header("Containers")]
        [SerializeField] private Transform popupRoot;
        [SerializeField] private Transform overlayRoot; // для модальных фоновых затемнений

        private DIContainer _container;
        private UIPrefabProvider prefabProvider;
        private UITransitionService transitionService;
        
        // Пул по ID попапа
        private readonly Dictionary<UIScreenId, Stack<UIPopup>> popupCash = new();


        // Очереди и активные popup по слоям
        private readonly Dictionary<PopupLayer, Queue<(UIScreenId id, object data)>> queues = new();
        private readonly Dictionary<PopupLayer, UIPopup> active = new();

        private CanvasGroup overlayCg;

        private void Awake()
        {
            foreach (PopupLayer layer in System.Enum.GetValues(typeof(PopupLayer)))
            {
                queues[layer] = new Queue<(UIScreenId, object)>();
                active[layer] = null;
            }

            // Создание overlay
            var go = new GameObject("PopupOverlay");
            go.transform.SetParent(overlayRoot);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var img = go.AddComponent<UnityEngine.UI.Image>();
            img.color = new Color(0f, 0f, 0f, 0.5f); // полупрозрачный чёрный

            overlayCg = go.AddComponent<CanvasGroup>();
            overlayCg.alpha = 0f;
            overlayCg.blocksRaycasts = false;
            go.SetActive(false);
        }

        public void Initialize(
            DIContainer container,
            UIPrefabProvider provider, 
            UITransitionService trans)
        {
            _container = container;
            prefabProvider = provider;
            transitionService = trans;
        }
        
        
        
        
        public void Preload(params UIScreenId[] screenId)
        {
            var l = screenId.Length;
            for (int i = 0; i < l; i++)
            {
                var cfg = prefabProvider.GetPopupConfig(screenId[i]);
                EnsurePool(screenId[i]);
                
                var instance = GetPopupInstance(cfg);
                ReturnToPool(instance);
            }
            
        }
        
        public void RemovePopups()
        {
            foreach (var popup in popupCash.Values)
            {
                foreach (var p in popup)
                {
                    p.Remove();
                    Destroy(p.gameObject);
                }
            }

            popupCash.Clear();
            foreach (PopupLayer layer in System.Enum.GetValues(typeof(PopupLayer)))
            {
                queues[layer] = new Queue<(UIScreenId, object)>();
                active[layer] = null;
            }
        }
        
        
        


        public void OpenPopup(UIScreenId popupId, object data = null)
        {
            var cfg = prefabProvider.GetPopupConfig(popupId);
            if (cfg == null)
            {
                Debug.LogError($"[UIPopupManager] No config for {popupId}");
                return;
            }

            var layer = cfg.layer;

            // добавляет попап в очередь
            if (active[layer] != null)
            {
                //queues[layer].Enqueue((popupId, data));
                return;
            }

            StartCoroutine(ShowPopup(cfg, data));
        }
        
        

        private IEnumerator ShowPopup(UIPopupConfig cfg, object data)
        {
            GameObject go = prefabProvider.LoadSync(cfg.id);
            if (go == null) 
                yield break;

            // var spawned = Instantiate(go, popupRoot);
            // var instance = spawned.GetComponent<UIPopup>();
            // instance.Initialize(_container, cfg);
            //instance.OnShow(data);
            
            var instance = GetPopupInstance(cfg);
            instance.ResetState();      // КРИТИЧНО
            instance.OnShow(data);

            active[cfg.layer] = instance;

            if (cfg.blockUnderlying)
            {
                overlayCg.gameObject.SetActive(true);
                overlayCg.blocksRaycasts = true;
                yield return StartCoroutine(transitionService.FadeIn(overlayCg, 0.15f));
            }

            if (instance.CanvasGroup)
                yield return StartCoroutine(transitionService.FadeIn(instance.CanvasGroup, 0.2f));
        }
        
        
        
        private UIPopup GetPopupInstance(UIPopupConfig cfg)
        {
            EnsurePool(cfg.id);

            if (popupCash[cfg.id].Count > 0)
            {
                var reused = popupCash[cfg.id].Pop();
                reused.gameObject.SetActive(true);
                reused.transform.SetParent(popupRoot, false);
                return reused;
            }

            GameObject prefab = prefabProvider.LoadSync(cfg.id);
            var go = Instantiate(prefab, popupRoot);
            var popup = go.GetComponent<UIPopup>();
            popup.Initialize(_container, cfg);
            popup.OnCloseAction = ClosePopup;
            return popup;
        }
        
        private void EnsurePool(UIScreenId id)
        {
            if (!popupCash.ContainsKey(id))
                popupCash[id] = new Stack<UIPopup>();
        }

        private void ReturnToPool(UIPopup popup)
        {
            popup.OnHide();
            if(popup.CanvasGroup)
            {
                popup.CanvasGroup.alpha = 0f;
            }

            popup.transform.SetParent(transform, false); // вне popupRoot
            popup.gameObject.SetActive(false);

            popupCash[popup.Config.id].Push(popup);
        }
        
        

        public void ClosePopup(UIScreenId popupId)
        {
            var cfg = prefabProvider.GetPopupConfig(popupId);
            if (cfg == null)
            {
                Debug.LogWarning($"[UIPopupManager] ClosePopup: unknown id {popupId}");
                return;
            }

            StartCoroutine(ClosePopupRoutine(cfg));
        }

        private IEnumerator ClosePopupRoutine(UIPopupConfig cfg)
        {
            var layer = cfg.layer;
            var popup = active[layer];
            if (popup == null) yield break;

            popup.OnHide();
            if (popup.CanvasGroup)
                yield return StartCoroutine(transitionService.FadeOut(popup.CanvasGroup, 0.15f));

            ReturnToPool(popup);
            active[layer] = null;

            bool anyBlocking = false;
            foreach (var kv in active)
            {
                if (kv.Value != null && kv.Value.Config.blockUnderlying)
                {
                    anyBlocking = true;
                    break;
                }
            }

            if (!anyBlocking)
            {
                yield return StartCoroutine(transitionService.FadeOut(overlayCg, 0.15f));
                overlayCg.blocksRaycasts = false;
                overlayCg.gameObject.SetActive(false);
            }

            if (queues[layer].Count > 0)
            {
                var next = queues[layer].Dequeue();
                OpenPopup(next.id, next.data);
            }
        }

        public bool IsPopupOpenOnLayer(PopupLayer layer) => active[layer] != null;
    }
}
