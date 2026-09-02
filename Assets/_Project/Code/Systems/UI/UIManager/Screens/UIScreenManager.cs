using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Galactic1.UI.Core
{
    public class UIScreenManager : MonoBehaviour
    {
        private DIContainer _container;
        private UIPrefabProvider prefabProvider;
        private UITransitionService transitions;

        private UIManager.CTransformRoot _layerRoot;

        // Активные экраны по id
        private readonly Dictionary<UIScreenId, UIScreenPanel> activeScreens = new();
        
        private readonly Dictionary<UIScreenId, UIScreenPanel> screenCache = new();


        
        

        public void Initialize(
            DIContainer container,
            UIPrefabProvider provider, 
            UITransitionService transition,
            UIManager.CTransformRoot layerRoot)
        {
            _container = container;
            prefabProvider = provider;
            transitions = transition;
            _layerRoot = layerRoot;
        }
        
        
        
        public void PreloadScreens(DIContainer container, params UIScreenId[] screenId)
        {
            activeScreens.Clear();
            screenCache.Clear();
            var l = screenId.Length;
            for (int i = 0; i < l; i++)
            {
                var prefab = prefabProvider.LoadSync(screenId[i]);
                var go = Instantiate(prefab, GetRoot(screenId[i]));
                go.SetActive(false);
                var panel = go.GetComponent<UIScreenPanel>() ?? go.AddComponent<UIScreenPanel>();
                panel.Initialize(container, screenId[i]);
                

                screenCache[screenId[i]] = panel;
            }
            
            // *** ставим экран смерти выше остальных
            if(screenCache.ContainsKey(UIScreenId.DeathScreen))
                screenCache[UIScreenId.DeathScreen].transform.SetSiblingIndex(GetRoot(UIScreenId.DeathScreen).childCount);
        }

        public void RemoveScreens()
        {
            var panels = screenCache.Values.ToList();
            var l = panels.Count;
            for (int i = 0; i < l; i++) // сначало отписываем
            {
                if (panels[i])
                    panels[i].Remove();
            }
            
            for (int i = 0; i < l; i++) // потом удаляем
            {
                if (panels[i])
                    Destroy(panels[i].gameObject);
            }
        }

        /// <summary>
        /// Открыть экран по id, передав данные через object data
        /// </summary>
        public IEnumerator OpenScreen(UIScreenId id, object data = null, Action<GameObject> onShow = null)
        {
            // Закрываем все активные экраны (можно изменить для multi-layer)
            foreach (var s in activeScreens.Values)
            {
                s.OnHide();
                //Destroy(s.gameObject);
                s.gameObject.SetActive(false);
            }

            activeScreens.Clear();

            // Загружаем prefab
            // var prefab = prefabProvider.LoadSync(id);
            // if (prefab == null) yield break;
            //
            // // Создаём экран
            // var go = Instantiate(prefab, screensRoot);
            // var panel = go.GetComponent<UIScreenPanel>() ?? go.AddComponent<UIScreenPanel>();
            // panel.Initialize(id);
            // activeScreens[id] = panel;
            
            // NEW
            var panel = screenCache[id];
            if (panel == null) yield break;
            activeScreens[id] = panel;
            // NEW

            // Вызываем метод OnShow с данными
            onShow?.Invoke(panel.gameObject);
            panel.OnShow(data);
            panel.gameObject.SetActive(true);

            // Получаем CanvasGroup и анимируем появление
            var cg = panel.GetComponent<CanvasGroup>();
            if (cg == null)
                cg = panel.gameObject.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
            yield return StartCoroutine(transitions.FadeIn(cg));
        }

        /// <summary>
        /// Закрыть все экраны
        /// </summary>
        public void CloseAll()
        {
            foreach (var s in activeScreens.Values)
            {
                s.OnHide();
                //Destroy(s.gameObject);
                s.gameObject.SetActive(false);
            }

            activeScreens.Clear();
        }

        /// <summary>
        /// Проверка, открыт ли экран
        /// </summary>
        public bool IsScreenOpen(UIScreenId id) => activeScreens.ContainsKey(id);



        /// <summary>
        /// Передача родителя для экрана
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Transform GetRoot(UIScreenId id)
            => id switch
            {
                UIScreenId.Settings or
                    UIScreenId.DeathScreen or
                    UIScreenId.PurchaseRewardScreen
                    => _layerRoot.overlayRoot,

                UIScreenId.HUDInput or
                    UIScreenId.HUDCamp or
                    UIScreenId.HUDLocation or
                    UIScreenId.HUDMap
                    => _layerRoot.hudRoot,

                UIScreenId.BaseConstructionMenu
                    => _layerRoot.constructionRoot,

                _ => _layerRoot.screensRoot
            };
    }
}
