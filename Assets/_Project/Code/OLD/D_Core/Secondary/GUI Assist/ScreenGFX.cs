using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Galactic1
{
    public class ScreenGFX : MonoBehaviour, IGameService
    {
        
        /*
         *     Ддя остановки корутин, нужно вызывать StopCoroutine через этот класс
         *     потому что он создает корутину
         */
        
        
        public enum EScreenFx
        {
            FLASH, 
            FLASH_PANEL,
            SHORT_DARK,
        }


        private GameObject coreCanvas;
            
        private void Start()
        {
            coreCanvas = GameObject.Find("Canvas Core");
        }



        
        /// <summary>
        /// Для вызова эффекта (Passive)
        /// </summary>
        /// <param name="type"></param>
        public void Get(EScreenFx type, GameObject g = null)
        {
            switch (type)
            {
                case EScreenFx.FLASH:
                {
                    StartCoroutine(flash());
                } break;
                
                case EScreenFx.FLASH_PANEL:
                {
                    StartCoroutine(flash(g));
                } break;
                
                case EScreenFx.SHORT_DARK:
                {
                    StartCoroutine(shortDark());
                } break;
            }
        }





        #region PASSIVE
        
        IEnumerator flash()
        {
            CanvasGroup cg = "Tools/flash".CreateGO(coreCanvas.transform).GetComponent<CanvasGroup>();
            cg.gameObject.SetActive(true);
            cg.alpha = 1;

            for (float i = 1; i >= 0; i-= Yie(.4f))
            {
                cg.alpha = i;
                yield return null;
            }

            Destroy(cg.gameObject);
        }
        
        IEnumerator flash(GameObject g)
        {
            CanvasGroup cg = g.GetComponent<CanvasGroup>();
            cg.alpha = 1;
            g.SetActive(true);
            

            for (float i = 1; i >= 0; i-= Yie(.2f))
            {
                cg.alpha = i;
                yield return null;
            }

            g.SetActive(false);
        }

        IEnumerator shortDark()
        {
            CanvasGroup cg = "Tools/shortDark".CreateGO(coreCanvas.transform).GetComponent<CanvasGroup>();
            cg.gameObject.SetActive(true);
            cg.alpha = 1;

            for (float i = 1; i >= 0; i-= Yie(.4f))
            {
                cg.alpha = i;
                yield return null;
            }

            Destroy(cg.gameObject);
        }
        

        #endregion
        
        
        #region FLASH SHADER

        /// <summary>
        /// Флеш через шейдер
        /// </summary>
        /// <param name="g"></param>
        public void Flash(Image[] g)
        {
            StartCoroutine(flash(g));
        }
        public IEnumerator flash(Image[] g)
        {
            for (int i = 0; i < g.Length; i++)
                g[i].SetShaiderFlash(1f, Color.white);
            
            for (float fl = 1; fl >= 0; fl-= Yie(.4f))
            {
                for (int i = 0; i < g.Length; i++)
                    g[i].SetShaiderFlash(fl, Color.white);
                yield return null;
            }

        }

        #endregion
        
        

        
        
        
        
        /// <summary>
        /// Передавать желаемое время (не скорость) для эффекта
        /// </summary>
        /// <param name="sec"></param>
        /// <returns></returns>
        public static float Yie(float sec) => Time.deltaTime * (1f / sec);
        
        
        

        /// 1 вариант: панель с анимацией и запоздалым затемнением фона
        public void PanelAnim1(GameObject panel, bool show, float smooth = .1f)
        {
            StartCoroutine(canvasGroup(panel, show, smooth));
        }
        IEnumerator canvasGroup(GameObject panel, bool show, float smooth)
        {
            CanvasGroup cg = panel.GetComponent<CanvasGroup>();

            if (show)
            {
                panel.SetActive(true);
                panel.GetComponent<Animator>().SetBool("open", true);
                for (float i = cg.alpha; i < 1f; i += Yie(smooth))
                {
                    cg.alpha = i;
                    yield return null;
                }
                cg.alpha = 1;
            }
            else
            {
                panel.SetActive(false);
                cg.alpha = 0;
                //panel.GetComponent<Animator>().SetBool("open", false);
            }
        }
        
        

        /// <summary>
        /// Анимация мигания спрайта, через прозрачность
        /// </summary>
        /// <param name="sp"></param>
        /// <returns>Вернет созданую корутину для возможности остановки</returns>
        public Coroutine AlphaPingPong(SpriteRenderer sp)
            => StartCoroutine(animAlpha(sp));
        IEnumerator animAlpha(SpriteRenderer sp)
        {
            float smooth = .05f;
            Color c = sp.color;
            bool y = true;
            while (true)
            {
                for (float i = c.a; i >= 0; i-=Yie(smooth))
                {
                    c.a = i;
                    sp.color = c;
                    yield return null;
                }
                
                for (float i = c.a; i < 1; i+=Yie(smooth))
                {
                    c.a = i;
                    sp.color = c;
                    yield return null;
                }
            }
        }
        
        
        
        /// <summary>
        /// Скрытие спрайта
        /// </summary>
        /// <param name="timeWait"></param>
        /// <param name="sr"></param>
        public void Hide_Sprite(float timeWait, SpriteRenderer sr)
        {
            StartCoroutine(hide_Sprite(timeWait, sr));
        }
        IEnumerator hide_Sprite(float timeWait, SpriteRenderer sr)
        {
            yield return new WaitForSeconds(timeWait);

            Color c = sr.color;
            for (float i = c.a; i >= 0; i-=.02f)
            {
                c.a = i;
                sr.color = c;
                yield return null;
            }
            c.a = 0;
            sr.color = c;
        }
        
        /// <summary>
        /// Скрытие спрайта
        /// </summary>
        /// <param name="timeWait"></param>
        /// <param name="sr"></param>
        public void Hide_Sprite(float timeWait, SpriteRenderer[] sr)
        {
            StartCoroutine(hide_Sprite(timeWait, sr));
        }
        IEnumerator hide_Sprite(float timeWait, SpriteRenderer[] sr)
        {
            yield return new WaitForSeconds(timeWait);

            Color c = sr[0].color;
            var l = sr.Length;
            for (float i = c.a; i >= 0; i -= .02f)
            {
                for (int j = 0; j < l; j++)
                {
                    c.a = i;
                    sr[j].color = c;
                }
                yield return null;
            }
        }
    }
}