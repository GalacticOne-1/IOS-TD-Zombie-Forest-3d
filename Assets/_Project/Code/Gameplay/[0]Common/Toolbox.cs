
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Galactic1
{
    public class Toolbox
    {



        /// <summary>
        /// Создает текст в 3d сцене
        /// </summary>
        /// <param name="s"></param>
        /// <param name="coord"></param>
        public static void TextInScene(string s, Vector3 coord, Transform hold)
        {
            var t = "Tools/text_scene".CreateGO(hold);
            t.GetChild(0).GetComponent<TextMeshPro>().text = s;
            t.transform.position = coord;
            //Destroy(numb, 1);
        }
        
        public static void TextInScene(string s, Vector2 coord, Transform hold)
        {
            var t = "Tools/text_scene".CreateGO(hold);
            t.GetChild(0).GetComponent<TextMeshPro>().text = s;
            t.transform.position = coord;
            //Destroy(numb, 1);
        }

        
        /// <summary>
        /// Обычный текст для технических целей
        /// </summary>
        /// <param name="s"></param>
        /// <param name="coord"></param>
        /// <param name="hold"></param>
        public static void TechText(string s, Vector2 coord, Transform hold)
        {
            var t = "Tools/tech_text".CreateGO(hold);
            t.GetComponent<TextMeshPro>().text = s;
            t.transform.position = coord;
        }



        /// <summary>
        /// true - элемент под курсором
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public static bool ElementUnderMouse(string tag, out GameObject obj)
        {
            obj = null;
            PointerEventData pointerData = new PointerEventData (EventSystem.current)
            {
                pointerId = -1,
            };
         
            pointerData.position = Input.mousePosition;
 
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);
            var l = results.Count;
            for (int i = 0; i < l; i++)
            {
                if (results[i].gameObject.CompareTag(tag))
                {
                    obj = results[i].gameObject;
                    return true;
                }
            }

            return false;
        }
        /// <summary>
        /// true - элемент под нужными координатами
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public static bool ElementUnderMouse(string tag, Vector2 pos, out GameObject obj)
        {
            obj = null;
            PointerEventData pointerData = new PointerEventData (EventSystem.current)
            {
                pointerId = -1,
            };
         
            pointerData.position = pos;
 
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);
            var l = results.Count;
            for (int i = 0; i < l; i++)
            {
                if (results[i].gameObject.CompareTag(tag))
                {
                    obj = results[i].gameObject;
                    return true;
                }
            }

            return false;
        }
    }
}