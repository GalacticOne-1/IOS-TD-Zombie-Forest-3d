using System.Collections;
using Galactic1.Code.UI.Tooltips;
using Galactic1.Configs;
using Galactic1.Game.Meta.Items;
using Galactic1.Game.UI.Stats;
using Galactic1.UI;
using Galactic1.UI.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Galactic1.Code.UI.Inventory
{
    public class TooltipInventoryUI : MonoBehaviour
    {
        [SerializeField] private CanvasGroup group;
        [SerializeField] private RectTransform root;
        
        [Space] 
        [SerializeField] TMP_Text titleText;
        [SerializeField] TMP_Text descriptionText;
        [SerializeField] TMP_Text rarityText;
        [SerializeField] TMP_Text tierText;
        [SerializeField] private GameObject statColumn;
        [SerializeField] private GameObject storageColumn;
        [SerializeField] private GameObject extraColumn;
        [SerializeField] private GameObject statItemPrefab, extraItemPrefab;
        [SerializeField] private GameObject listItemsPrefab;
        

        [Header("Offsets")]
        [SerializeField] private float slotOffset = 8f;    // отступ от слота
        [SerializeField] private float screenPadding = 12f; // отступ от края экрана

        private Canvas canvas;
        private RectTransform canvasRect;

        private void Awake()
        {
            canvas = GetComponentInParent<Canvas>();
            canvasRect = canvas.GetComponent<RectTransform>();
            Hide();
        }



        public void LoadData(ItemConfig item, int durability)
        {
            if (item == null) return;
            
            root.anchoredPosition = new(0,5000);


            var data = HintResolver.Formatting(HintSource.InventoryUnit, item, durability);
            
            // ====== META ======
            titleText.text = data.title;
            descriptionText.text = data.description;
            rarityText.text = data.itemType;
            //rarityText.text = data.rarity;
            //tierText.text = data.tier;
            tierText.text = "";

            // ====== ОЧИСТКА СПИСКОВ ======
            foreach (Transform child in statColumn.transform)
                if (child.GetComponent<TMP_Text>() || child.GetComponent<ItemListFieldView>())
                    Destroy(child.gameObject);
            foreach (Transform child in storageColumn.transform)
                if (child.GetComponent<TMP_Text>())
                    Destroy(child.gameObject);
            foreach (Transform child in extraColumn.transform)
                if (child.GetComponent<TMP_Text>())
                    Destroy(child.gameObject);

            // ====== СТАТЫ ======
            var kvps = data.stats;
            if (kvps != null && kvps.Count > 0)
            {
                statColumn.SetActive(true);
                var layoutConfig = ServiceLocator.Current.Get<ConfigProvider>().Get<StatLayoutConfig>();
            
                foreach (var kvp in kvps)
                {
                    // prefab → [StatName | Value]
                    var go = Instantiate(statItemPrefab, statColumn.transform);
                    var texts = go.GetComponentsInChildren<TMP_Text>();
                    if (texts.Length >= 2)
                    {
                        texts[0].text = kvp.label;
                        texts[1].text = kvp.value;
                        texts[1].gameObject.GetChild(0).SetActive(kvp.Style != TooltipDataFieldStyle.Orange);
                        texts[1].gameObject.GetChild(0).CMP_Image().sprite = layoutConfig.GetCompareIcon(kvp.Style);
                    }
                }
            }
            else
            {
                statColumn.SetActive(false);
            }
            
            
            // === список связанных предметов
            if (data.linkedItems.Count > 0)
            {
                statColumn.SetActive(true);
                var listItems = listItemsPrefab.CreateGO(statColumn.transform);
                StatUIBuilder.Apply(
                    data.linkedItemStyle,
                    listItems.GetChild(0).CMP_Text(), 
                    listItems.GetChild(1).transform, 
                    data.linkedItems);
            }

            
            // ====== STORAGE ======
            kvps = data.storage;
            if (kvps != null && kvps.Count > 0)
            {
                storageColumn.SetActive(true);
            
                foreach (var kvp in kvps)
                {
                    // prefab → [StatName | Value]
                    var go = Instantiate(statItemPrefab, storageColumn.transform);
                    var texts = go.GetComponentsInChildren<TMP_Text>();
                    if (texts.Length >= 2)
                    {
                        texts[0].text = kvp.label;
                        texts[1].text = kvp.value;
                    }
                }
            }
            else
            {
                storageColumn.SetActive(false);
            }
            

            // ====== EXTRA ======
            kvps = data.extra;
            if (kvps != null && kvps.Count > 0)
            {
                extraColumn.SetActive(true);
            
                foreach (var kvp in kvps)
                {
                    // prefab → [StatName | Value]
                    var go = Instantiate(extraItemPrefab, extraColumn.transform);
                    go.GetComponent<TMP_Text>().text = kvp.label;
                }
            }
            else
            {
                extraColumn.SetActive(false);
            }

            ServiceLocator.Current.Get<CoroutineController>().StartCoroutine(e());
        }

        IEnumerator e()
        {
            yield return null;
            yield return null;
            root.gameObject.SetActive(false);
            yield return null;
            root.gameObject.SetActive(true);
            yield return null;
            root.GetChild(1).GetComponent<VerticalLayoutGroup>().enabled = false;
            yield return null;
            root.GetChild(1).GetComponent<VerticalLayoutGroup>().enabled = true;
            yield return null;
            root.CMP_RectTr().sizeDelta = root.GetChild(1).CMP_RectTr().sizeDelta;
        }
        

        /// <summary>
        /// Показывает подсказку рядом с объектом
        /// </summary>
        public void Show(RectTransform targetRect)
        {
            
            // получаем центр слота в world space
            Vector3[] corners = new Vector3[4];
            targetRect.GetWorldCorners(corners);
            Vector2 slotSize = new Vector2(corners[2].x - corners[0].x, corners[2].y - corners[0].y);
            Vector2 slotCenter = (Vector2)corners[0] + slotSize / 2f;

            // конвертируем в локальные координаты Canvas
            Vector2 canvasPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                slotCenter,
                canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
                out canvasPos
            );

            // определяем сторону относительно центра Canvas
            Vector2 tooltipPos = canvasPos;
            Vector2 tooltipSize = root.sizeDelta;
            Vector2 canvasSize = canvasRect.sizeDelta;

            if (canvasPos.x < 0) 
            {
                // слот слева от центра → панель справа
                tooltipPos.x += slotSize.x / 2f + tooltipSize.x / 2f + slotOffset;
            }
            else
            {
                // слот справа от центра → панель слева
                tooltipPos.x -= slotSize.x / 2f + tooltipSize.x / 2f + slotOffset;
            }

            // проверка верх/низ
            float topEdge = tooltipPos.y + tooltipSize.y / 2f + screenPadding;
            float bottomEdge = tooltipPos.y - tooltipSize.y / 2f - screenPadding;

            if (topEdge > canvasSize.y / 2f)
                tooltipPos.y -= (topEdge - canvasSize.y / 2f);
            if (bottomEdge < -canvasSize.y / 2f)
                tooltipPos.y -= (bottomEdge + canvasSize.y / 2f);

            root.anchoredPosition = tooltipPos;
            group.alpha = 1;
        }

        public void Hide()
        {
            group.alpha = 0;
        }
    }
}
