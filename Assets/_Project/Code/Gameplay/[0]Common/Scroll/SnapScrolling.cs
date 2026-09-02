using System;
using Galactic1;
using UnityEngine;
using UnityEngine.UI;

namespace Galactic1
{
    public class SnapScrolling : MonoBehaviour
    {

        public bool vertical;
        public ScrollRect scroll;
        [SerializeField] private float spacing = 5;
        [SerializeField] private float snapSpeed = 20;
        [SerializeField] private float snapScale = 20, smoothScale = 20;

        private GameObject[] items;
        private Vector2[] itemPos, itemScale;
        private Vector2 pos, contentPos;
        private RectTransform contentRect;
        private int count;

        private float distance,
            nearestPos,
            scale,
            scrollVel;

        public bool isScrolling { get; private set; }
        public int selectedId { get; private set; }
        //public int SelectedId => selectedId;
        public int lastId { get; private set; }
        
        public DFunc onCancelSelect, onSelect, scrollFunc;

        
        private bool toNumber;
        /// <summary>
        /// Для фокуса на карточку
        /// </summary>
        /// <param name="i"></param>
        public void ToNumber(int i)
        {
            toNumber = true;
            selectedId = i;
        }
        
        
        
        
        

        public void ClearContent()
        {
            count = 0;
            onSelect = null;
            scroll.content.MakeEmpty();
        }

        /// <summary>
        /// Для запуска
        /// </summary>
        public void LoadContent()
        {
            if (scroll.content.childCount == 0) return;

            contentRect = scroll.content.GetComponent<RectTransform>();
            var size = vertical
                ? scroll.content.GetChild(0).GetComponent<RectTransform>().sizeDelta.y
                : scroll.content.GetChild(0).GetComponent<RectTransform>().sizeDelta.x;
            count = scroll.content.childCount;
            
            items = new GameObject[count];
            itemPos = new Vector2[count];
            itemScale = new Vector2[count];
            for (int i = 0; i < count; i++)
            {
                items[i] = scroll.content.GetChild(i).gameObject;
                if (i == 0)
                {
                    // ставим первый элемент в ноль, от него будут распологаться остальные
                    items[i].transform.localPosition = Vector2.zero;
                    continue;
                }

                if (vertical)
                {
                    pos.x = 0;
                    pos.y = items[i - 1].transform.localPosition.y - size - spacing;
                    items[i].transform.localPosition = pos;
                    itemPos[i] = items[i].transform.localPosition;
                    itemPos[i].y = Mathf.Abs(itemPos[i].y);
                }
                else
                {
                    pos.x = items[i - 1].transform.localPosition.x + size + spacing;
                    pos.y = 0;
                    items[i].transform.localPosition = pos;
                    itemPos[i] = -items[i].transform.localPosition;
                    
                    if(selectedId != 0)
                    {
                        contentPos.x = itemPos[selectedId].x;
                        contentRect.anchoredPosition = contentPos;
                    }
                }
                
            }

            selectedId = -1;
            lastId = -1;
            
            scroll.ScrollRectResetV();

            scrollFunc = vertical ? ScrollVertical : ScrollHorizontal;
        }

        

        private void Update()
        {
            if (count == 0) return;
            scrollFunc();
        }

        void ScrollVertical()
        {
            if (!isScrolling && (contentRect.anchoredPosition.y >= itemPos[0].y ||
                                 contentRect.anchoredPosition.y <= itemPos[count-1].y))
                scroll.inertia = false;
            
            // блокировка для авто фокуса на нужном эдементе    ! не тестировал !
            if (!toNumber)
            {
                nearestPos = float.MaxValue;
                for (int i = 0; i < count; i++)
                {
                    distance = Mathf.Abs(contentRect.anchoredPosition.y - itemPos[i].y);
                    if (distance < (selectedId != -1 ? Mathf.Abs(contentRect.anchoredPosition.y - itemPos[selectedId].y) : nearestPos))
                    {
                        nearestPos = distance;
                        lastId = selectedId;
                        selectedId = i;
                        onSelect?.Invoke();
                    }

                    scale = Mathf.Clamp(1 / (distance / spacing) * snapScale, .7f, 1f);
                    itemScale[i].y = Mathf.SmoothStep(items[i].transform.localScale.y, scale, smoothScale * Time.fixedDeltaTime);
                    itemScale[i].x = Mathf.SmoothStep(items[i].transform.localScale.x, scale, smoothScale * Time.fixedDeltaTime);
                    items[i].transform.localScale = itemScale[i];
                }
            }

            scrollVel = Mathf.Abs(scroll.velocity.y);
            if (!isScrolling && scrollVel < 400) scroll.inertia = false;

            if (isScrolling || scrollVel > 400) return;
            contentPos.y = Mathf.SmoothStep(contentRect.anchoredPosition.y, itemPos[selectedId].y, snapSpeed * Time.fixedDeltaTime);
            contentRect.anchoredPosition = contentPos;
            
            // отключит авто перемещение когда будет фокус на цели
            if (toNumber && Mathf.Abs(contentRect.anchoredPosition.y - itemPos[selectedId].y) < 30)     // 30 - можно делать меньше
            {
                toNumber = false;
            }
        }
        
        
        
        void ScrollHorizontal()
        {
            if (!isScrolling && (contentRect.anchoredPosition.x >= itemPos[0].x ||
                                 contentRect.anchoredPosition.x <= itemPos[count-1].x))
                scroll.inertia = false;
            
            // блокировка для авто фокуса на нужном эдементе
            if (!toNumber)
            {
                nearestPos = float.MaxValue;
                for (int i = 0; i < count; i++)
                {
                    distance = Mathf.Abs(contentRect.anchoredPosition.x - itemPos[i].x);
                    if (distance < (selectedId != -1 ? Mathf.Abs(contentRect.anchoredPosition.x - itemPos[selectedId].x) : nearestPos))
                    {
                        nearestPos = distance;
                        lastId = selectedId;
                        selectedId = i;
                        onSelect?.Invoke();
                        ServiceLocator.Current.Get<AudioController>().Sound_UI(9);
                    }

                    scale = Mathf.Clamp(1 / (distance / spacing) * snapScale, .6f, 1f);
                    itemScale[i].y = Mathf.SmoothStep(items[i].transform.localScale.y, scale, smoothScale * Time.fixedDeltaTime);
                    itemScale[i].x = Mathf.SmoothStep(items[i].transform.localScale.x, scale, smoothScale * Time.fixedDeltaTime);
                    items[i].transform.localScale = itemScale[i];
                }
            }

            scrollVel = Mathf.Abs(scroll.velocity.x);
            if (!isScrolling && scrollVel < 1200) scroll.inertia = false;

            if (isScrolling || scrollVel > 1200) return;
            contentPos.x = Mathf.SmoothStep(contentRect.anchoredPosition.x, itemPos[selectedId].x, snapSpeed * Time.fixedDeltaTime);
            contentRect.anchoredPosition = contentPos;

            // отключит авто перемещение когда будет фокус на цели
            if (toNumber && Mathf.Abs(contentRect.anchoredPosition.x - itemPos[selectedId].x) < 30)     // 30 - можно делать меньше
            {
                toNumber = false;
            }
        }
        
        

        public void Scrolling(bool sc)
        {
            isScrolling = sc;
            if(sc) scroll.inertia = true;
        }
    }
}