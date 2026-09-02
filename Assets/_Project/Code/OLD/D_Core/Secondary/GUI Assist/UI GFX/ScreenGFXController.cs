
using System.Collections;
using Galactic1;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Galactic1
{
    
    public enum EFloatObj
    {
        money, gems, exp, energy
    }
    public class ScreenGFXController : MonoBehaviour, IGameService
    {
        [SerializeField] private GameObject[] prefabFloatObj;
        [SerializeField] private GameObject coins;
        [SerializeField, Header("Particles")] 
        private GameObject ps_up;

        [SerializeField] private GameObject ps_coins,
            ps_gems, ps_permits1, ps_permits2;
        [SerializeField] private GameObject uiHold, world;

        public float smooth;

        
        

        #region NEW CONTENT
        
        [Header("New Content")]
        [SerializeField] private GameObject darkScreen;
        [SerializeField] private GameObject whiteCircle;
        [SerializeField] private GameObject newContent;
        [SerializeField] private TextMeshProUGUI tNewContent, tReward;
        [SerializeField] private float newContentTime;
        
        public struct CGFXData
        {
            public Sprite button;
            public Sprite icon;
            public string txt;
            public Vector2 sizeBtn, sizeIcon;
            public Vector3 coord;
        }

        private Vector3 new_content_coord;
        
        #endregion
        
        
        #region REWARD
        
        /*[Header("Reward")]
        [SerializeField] private GameObject whiteCircle2;
        [SerializeField] private GameObject newContent2;
        [SerializeField] private TextMeshProUGUI newContentTitle2;*/
        
        public struct CGFXData2
        {
            public Sprite icon;
            public Vector2 size;
            public string txt;
        }
        
        #endregion
        
        
        
        

        public static Vector2 center => new Vector2(Screen.width / 2, Screen.height / 2);


        private void Start()
        {
            //Debug.Log(Screen.height);
            
            //if (Screen.width < 1300)
                //smooth -= smooth * .4f;
            //else if (Screen.width < 2000)
                //smooth -= smooth * .15f;


        }


        public void AnimFloatMoney(EFloatObj type, Vector2 center)
        {
            StartCoroutine(floatMoney(type, center));
        }
        
        
        IEnumerator floatMoney(EFloatObj type, Vector2 center)
        {
            GameObject prefab = prefabFloatObj[(int) type];
            sbyte q = 15;
            Vector3[] pos = new Vector3[q];
            byte[] step = new byte[q];
            GameObject[] obj = new GameObject[q];
            sbyte liveObj = q;

            // создаем кучку объектов и раскидываем рядом 
            for (int i = 0; i < q; i++)
            {
                obj[i] = prefab.CreateGO(uiHold.transform);
                obj[i].transform.position = center;
                pos[i] = obj[i].transform.position;
                pos[i].x += Random.Range(-90f, 90f);
                pos[i].y += Random.Range(-90f, 90f);
                //obj[i].transform.position = pos[i];
            }

            while (liveObj > 0)
            {
                for (int i = 0; i < q; i++)
                {
                    if (obj[i] == null) continue;

                    obj[i].transform.position =
                        Vector3.MoveTowards(obj[i].transform.position, pos[i], 
                            smooth * 100 * Time.deltaTime);
                    
                    if (Vector3.Distance(obj[i].transform.position,pos[i]) < 1)
                    {
                        step[i]++;
                        if (step[i] == 1)
                        {
                            //if (type == EFloatObj.money)
                                //pos[i] = ServiceLocator.Current.Get<UIStatController>().GetCoord(EBankResourceType.CurrencySoft);
                            //else if (type == EFloatObj.gems)
                                //pos[i] = ServiceLocator.Current.Get<UIStatController>().GetCoord(EBankResourceType.CurrencyPremium);
                            //else if (type == EFloatObj.energy)
                                //pos[i] = UIStatController.I.posEnergy;
                            //else if (type == EFloatObj.exp)
                                //pos[i] = UIStatController.I.posExp;
                        }

                        if (step[i] == 2)
                        {
                            Destroy(obj[i]);
                            liveObj--;
                        }
                    } 
                    
                }
                
                yield return null;
            }
        }


        
        
        
        public void AnimFloatMoney(Vector2 coord)
        {
            var g = coins.CreateGO(uiHold.transform);
            g.transform.position = coord;
        }






        /// <summary>
        /// Создание в канвасе партикла награды
        /// </summary>
        /// <param name="coord"></param>
        /// <param name="type"></param>
        /// <param name="t"></param>
        public void PS_regular(Vector2 coord, EBankResourceType type , string t = "")
        {
            GameObject inst = null;
            switch (type)
            {
                case EBankResourceType.CurrencyPremium:
                    inst = ps_gems;
                    break;
                
                /*case EStat.permit_weapon:
                    inst = ps_permits1;
                    break;
                case EStat.permit_obj:
                    inst = ps_permits2;
                    break;*/
                
                default:
                    inst = ps_coins;
                    break;
            }
            
            var g = inst.CreateGO(ApplicationSetup.I.coreCanvas.transform);
            g.transform.position = coord;
            if (t.Length > 0)
                g.GetChild(0,0).GetComponent<TextMeshProUGUI>().text = t;
        }
        
        /*public void PS_gems(Vector2 coord, string t = "")
        {
            var g = ps_gems.CreateGO(ApplicationSetup.I.coreCanvas.transform);
            g.transform.position = coord;
            
            if (t.Length > 0)
                g.GetChild(0).GetComponent<TextMeshPro>().text = t;
        }
        
        public void PS_coins(Vector2 coord, string t = "")
        {
            //Debug.Log(coord);
            //Debug.Log(camera.ScreenToWorldPoint(coord)+" camera");
            var g = ps_coins.CreateGO(ApplicationSetup.I.coreCanvas.transform);
            g.transform.position = coord;

            if (t.Length > 0)
                g.GetChild(0).GetComponent<TextMeshPro>().text = t;
        }*/
        
        public void PS_up(Vector2 coord)
        {
            var g = ps_up.CreateGO(ApplicationSetup.I.coreCanvas.transform);
            g.transform.position = coord;
        }



        #region FLOATING HEAP
        
        
        public struct CHeapData
        {
            public EBankResourceType type;
            public Vector2 start;
        }
        
        public struct CHeap
        {
            public Vector2 coord;
            public GameObject g;
        }

        
        /// <summary>
        /// Всплывающие спрайты летящие в цель
        /// </summary>
        /// <param name="type"></param>
        /// <param name="start"></param>
        public void FloatingHeap(EBankResourceType type, Vector2 start)
        {
            //StartCoroutine(floatinHeap(type, start, ServiceLocator.Current.Get<UIStatController>().GetCoord(type), null));
        }
        
        /// <summary>
        /// Всплывающие спрайты летящие в цель (массив по очереди)
        /// </summary>
        /// <param name="data"></param>
        public void FloatingHeap(CHeapData[] data)
        {
            StartCoroutine(floatinHeap(data));
        }
        
        /// <summary>
        /// Всплывающие спрайты летящие в цель (массив по очереди)
        /// </summary>
        /// <param name="data"></param>
        /// <param name="g">сигналы окончания</param>
        public void FloatingHeap(CHeapData[] data, out GameObject[] g)
        {
            g = new GameObject[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                g[i] = new GameObject();
                g[i].name = $"(TEMP.) GFX Signal Complete #{data[i].type}";
                g[i].transform.parent = ServiceLocator.Current.Get<ViewGameController>().GetCanvas(ECanvas.GAME).transform;
            }
            StartCoroutine(floatinHeap(data, g));
        }
        
        // для поочередного появления групп объектов
        IEnumerator floatinHeap(CHeapData[] data, GameObject[] g = null)
        {
            var l = data.Length;
            for (int i = 0; i < l; i++)
            {
               // StartCoroutine(floatinHeap(data[i].type, data[i].start, 
                    //ServiceLocator.Current.Get<UIStatController>().GetCoord(data[i].type), g != null ? g[i] : null));
                yield return new WaitForSeconds(.02f);
            }
        }

        // процесс появление и движения
        IEnumerator floatinHeap(EBankResourceType type, Vector2 start, Vector2 target, GameObject g)
        {
            CHeap[] ar = new CHeap[5];
            //var sprite = ServiceLocator.Current.Get<IconHub>().GetSpriteStat(type);

            // create items
            var l = ar.Length;
            for (int i = 0; i < l; i++)
            {
                // ar[i].g = ServiceLocator.Current.Get<IconHub>().PrefabStat
                //     .CreateGO(ServiceLocator.Current.Get<ViewGameController>().GetCanvas(ECanvas.OVER).transform);
                // ar[i].g.GetComponent<Image>().sprite = sprite;
                ar[i].g.transform.position = start;
                ar[i].coord = start;
                ar[i].coord.x += Random.Range(-80, 80);
                ar[i].coord.y += Random.Range(-80, 80);
            }

            

            float time = 0;
            
            // двигаем из точки спавна
            while (time < .3f)
            {
                yield return null;

                for (int i = 0; i < l; i++)
                {
                    ar[i].g.transform.position = Vector2.Lerp(ar[i].g.transform.position, ar[i].coord, (time / .3f).DelayIn());
                }
                time += Time.deltaTime;
            }
            
            yield return new WaitForSeconds(.05f);
            
            // двигаем в конечные координаты
            time = 0;
            while (time < 1f)
            {
                yield return null;

                for (int i = 0; i < l; i++)
                {
                    ar[i].g.transform.position = Vector2.Lerp(ar[i].g.transform.position, target, (time / 2f).DelayIn());
                }
                time += Time.deltaTime;
                //DLog.Alert($">>> {time}", "yellow");
            }
            
            // update GUI
            //ServiceLocator.Current.Get<StatController>().UpdateGUI_FX(type);
            
            // удаляем когда прибыли
            for (int i = 0; i < l; i++)
            {
                Destroy(ar[i].g);
            }

            if (g) Destroy(g);
            DLog.Alert(">>> GFX Complete!");
        }

        #endregion



        #region FX NEW BUTTON
        
        /*
         *      При открытии неовой кнопки по центру появляется вид этой кнопки
         *      после чего она движется к оригинальной кнопке и эффект заканчивается
         */
        

        private EventBinding<AnyKeyDownEvent> finish;

        public GameObject FXNewContent(CGFXData data)
        {
            CORT.BlockScreen(true);
            newContent.SetUIPosition(Vector3.zero);
            newContent.GetChild(0).transform.localScale = Vector3.one;
            newContent.GetChild(0).GetRectTr().sizeDelta = data.sizeBtn;
            newContent.GetComponent<Animator>().enabled = true;
            newContent.GetChild(0).GetComponent<Image>().sprite = data.button;
            newContent.GetChild(0,0).GetComponent<Image>().sprite = data.icon;
            newContent.GetChild(0,0).GetRectTr().sizeDelta = data.sizeIcon;
            new_content_coord = data.coord;

            tNewContent.text = "New content has been reached.";

            StartCoroutine(new_content_1(data));
            return darkScreen;
        }

        IEnumerator new_content_1(CGFXData data)
        {
            darkScreen.SetActive(true);
            yield return new WaitForSeconds(.1f);
            whiteCircle.SetActive(true);
            yield return new WaitForSeconds(.1f);
            newContent.SetActive(true);
            yield return new WaitForSeconds(.2f);
            //newContent.GetComponent<Animator>().enabled = false;
            //newContent.transform.localScale = Vector3.one * 1.4f;

            // окончание процесса через любой клик
            finish = new EventBinding<AnyKeyDownEvent>(FXResetNewContent);
            EventBus<AnyKeyDownEvent>.Register(finish, true);
            
            //yield return new WaitForSeconds(.3f);
            tNewContent.gameObject.SetActive(true);
            CORT.BlockScreen(false);
        }

        public void FXResetNewContent()
        {
            CORT.BlockScreen(true);
            whiteCircle.SetActive(false);
            tNewContent.gameObject.SetActive(false);
            newContent.GetComponent<Animator>().enabled = false;
            StartCoroutine(new_content_2(new_content_coord));
            
        }
        
        IEnumerator new_content_2(Vector3 coord)
        {
            float speed = Vector2.Distance(newContent.transform.position, coord) / newContentTime;
            float smooth = 1 / newContentTime;
            while (true)
            {
                yield return null;
                
                if(Vector2.Distance(newContent.transform.position, coord) < .1f) break;

                newContent.transform.localScale = Vector2.MoveTowards(newContent.transform.localScale, Vector2.one,
                    smooth * Time.deltaTime);
                newContent.transform.position = Vector2.MoveTowards(newContent.transform.position, coord,
                    speed * Time.deltaTime);
            }
            
            yield return new WaitForSeconds(.1f);

            darkScreen.SetActive(false);
            newContent.SetActive(false);
            CORT.BlockScreen(false);
        }

        #endregion


        #region FX REWARD
        

        public void FXReward(CGFXData2 data)
        {
            CORT.BlockScreen(true);
            newContent.SetUIPosition(Vector3.zero);
            newContent.GetChild(0).transform.localScale = data.size;
            newContent.GetChild(0).GetComponent<RectTransform>().sizeDelta = Vector2.one * 100;
            newContent.GetComponent<Animator>().enabled = true;
            newContent.GetChild(0).GetComponent<Image>().sprite = data.icon;
            //newContent.GetChild(0, 0).GetComponent<Image>().sprite = ServiceLocator.Current.Get<IconHub>()._null;

            
            tReward.text = data.txt;

            StartCoroutine(reward(data));
        }

        IEnumerator reward(CGFXData2 data)
        {
            darkScreen.SetActive(true);
            yield return new WaitForSeconds(.1f);
            whiteCircle.SetActive(true);
            yield return new WaitForSeconds(.1f);
            newContent.SetActive(true);
            yield return new WaitForSeconds(.1f);
            //newContent.GetComponent<Animator>().enabled = false;
            //newContent.transform.localScale = Vector3.one * 1.4f;

            // окончание процесса через любой клик
            finish = new EventBinding<AnyKeyDownEvent>(() =>
            {
                darkScreen.SetActive(false);
                whiteCircle.SetActive(false);
                newContent.SetActive(false);
                tReward.transform.parent.gameObject.SetActive(false);
                newContent.GetComponent<Animator>().enabled = false;
            });
            EventBus<AnyKeyDownEvent>.Register(finish, true);
            
            //yield return new WaitForSeconds(.3f);
            tReward.transform.parent.gameObject.SetActive(true);
            CORT.BlockScreen(false);
        }

        #endregion

    }
}