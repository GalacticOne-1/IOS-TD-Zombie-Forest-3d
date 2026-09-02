using System;
using System.Collections;
using Galactic1;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Galactic1
{
    public class TaskLoggerController : MonoBehaviour, IGameService
    {
        [SerializeField] private GameObject bOpen, bClose;
        
        
        [SerializeField] private GameObject prefab, prefabComplete;

        [SerializeField] private ScrollRect scroll, scrollHideden;



        
        public struct CData
        {
            public string stage;
            public string title, des;
        }

        private TaskItem[] list;

        public TaskItem[] List => list;


        private void Awake()
        {
            bOpen.EventBtnOne_old(StatePanel);
            bClose.EventBtnOne_old(StatePanel);
        }
        
        // open/close
        void StatePanel()
        {
            // for closing
            if (scroll.gameObject.activeSelf)
            {
                bOpen.SetActive(true);
                bClose.SetActive(false);
                scroll.gameObject.SetActive(false);
            }
            
            else
            {
                bOpen.SetActive(false);
                bClose.SetActive(true);
                scroll.gameObject.SetActive(true);
            }
        }


        
        
        /// <summary>
        /// Для добавления нового лога задания
        /// </summary>
        /// <param name="task"></param>
        /// <param name="id"></param>
        public void AddTask(CData task, out short id)
        { 
            // item
            // сначало создаем элемент в вынесенный за экран скролл
            // потом через кадр переносим в нормальный скролл, что бы не было дерганий при изменении размеров
            TaskItem item = prefab.CreateGO(scrollHideden.content.GetChild(0)).GetComponent<TaskItem>();
            item.tStage.text = task.stage;
            item.tTitle.text = task.title;
            AddTask(item, out id);
            
            
            // *** load task list
            ServiceLocator.Current.Get<CoroutineController>().StartCoroutine(e(id));
        }

        IEnumerator e(short id)
        {
            float width = scroll.transform.GetRectTr().sizeDelta.x;
            float height;
            
                
            yield return null;
                
            // * set size item
            list[id].transform.SetSizeContentWithChildsV(out height, new float[] { -1, -1, 25 });
            list[id].gameObject.SetUISize(new Vector2(width, height));
            
            
            yield return null;

            // перенос в видимый скролл
            list[id].transform.parent = scroll.content.GetChild(0);
            var coord = list[id].transform.localPosition;
            coord.x = 0;
            list[id].transform.localPosition = coord;
            
            
            // *** set size content
            scroll.content.GetChild(0).SetSizeContentWithChildsV(out height, new float[]{25});
            scroll.content.SetUISize(new Vector2(width, 
                height > scroll.transform.GetRectTr().sizeDelta.y ? height : scroll.transform.GetRectTr().sizeDelta.y));
            scroll.ScrollRectResetV();
        }

        void AddTask(TaskItem data, out short id)
        {
            id = -1;
            if (list == null || list.Length == 0)
            {
                list = new TaskItem[1];
                id = 0;
                list[0] = data;
            }

            else
            {
                // #1 find free slot
                var l = list.Length;
                for (short i = 0; i < l; i++)
                {
                    if (!list[i])
                    {
                        id = i;
                        list[i] = data;
                        return;
                    }
                }
                
                // #2 add new element
                var cash = list;
                list = new TaskItem[cash.Length+1];
                l = cash.Length;
                for (short i = 0; i < l; i++)
                {
                    list[i] = cash[i];
                }

                id = (short)l;
                list[l] = data;
            }
        }

        
        /// <summary>
        /// Для удаления лога задания
        /// </summary>
        /// <param name="id"></param>
        public void RemoveTask(short id)
        {
            DLog.Alert($"remove >>> {id}");
            StartCoroutine(remove(id));
        }

        IEnumerator remove(short id)
        {
            list[id].tTitle.gameObject.SetActive(false);
            
            yield return new WaitForSeconds(.3f);
            
            var g = prefabComplete.CreateGO(list[id].transform);
            g.transform.localPosition = list[id].tTitle.transform.localPosition;
            
            yield return new WaitForSeconds(1f);
            
            Destroy(list[id].gameObject);
            list[id] = null;

            yield return null;
            
            // *** set size content
            if(scroll.content.GetChild(0).childCount > 0)
            {
                float width = scroll.transform.GetRectTr().sizeDelta.x;
                float height;
                scroll.content.GetChild(0).SetSizeContentWithChildsV(out height, new float[] { 25 });
                scroll.content.SetUISize(new Vector2(width,
                    height > scroll.transform.GetRectTr().sizeDelta.y
                        ? height
                        : scroll.transform.GetRectTr().sizeDelta.y));
                scroll.ScrollRectResetV();
            }
        }

        
        
        
        


        
        
        
        
        
        
        /*private void Update()
        {
            if (Input.GetKeyDown(KeyCode.L))
            {
                AddTask(new CData()
                {
                    stage = "Settling",
                    title = $"Collect berries #{Random.Range(0,100)}"
                }, out short id);
            }
            
            if (Input.GetKeyDown(KeyCode.K))
            {
                RemoveTask(0);
            }
        }*/
        
    }


    public class Task_Log_ADD
    {
        public Task_Log_ADD(TaskLoggerController.CData task, out short id)
        {
            ServiceLocator.Current.Get<TaskLoggerController>().AddTask(task, out id);
        }
    }
    
    public class Task_Log_GET
    {
        public Task_Log_GET(short id, out TaskItem item)
        {
            item = ServiceLocator.Current.Get<TaskLoggerController>().List[id];
        }
    }
    
    
    public class Task_Log_REMOVE
    {
        public Task_Log_REMOVE(short id)
        {
            ServiceLocator.Current.Get<TaskLoggerController>().RemoveTask(id);
        }
    }
}