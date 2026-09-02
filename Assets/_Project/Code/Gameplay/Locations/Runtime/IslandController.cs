using System;
using Galactic1;
using UnityEngine;

namespace Galactic1
{
    public class IslandController : MonoBehaviour, IIslandBorder
    {
        /*
         *      Должно висеть на папке с коллайдерами острова
         *      (для каждого острова своя папка с коллайдерами)
         */


        private Vector2 border;                     // текущие границы острова

        public Vector2 Border => border;

        
        
        

        private void Start()
        {
            float xMin = float.MaxValue;
            float xMax = float.MinValue;
            float posX, check;
            int idMin = 0, idMax = 0;

            BoxCollider2D collider;
            
            var l = transform.childCount;
            for (int i = 0; i < l; i++)
            {
                
                collider = gameObject.GetChild(i).GetComponent<BoxCollider2D>();
                
                // получаем позицию с учетом офсета   
                posX = gameObject.GetChild(i).transform.position.x + 
                       collider.offset.x * gameObject.GetChild(i).transform.localScale.x;

                // #1 min border
                check = posX - collider.size.x * gameObject.GetChild(i).transform.localScale.x / 2;
                
                if(check < xMin)
                {
                    xMin = check;
                    idMin = i;
                }
                
                // #2 max border
                check = posX + collider.size.x * gameObject.GetChild(i).transform.localScale.x / 2;
                
                if(check > xMax)
                {
                    xMax = check;
                    idMax = i;
                }
            }

            // apply border
            border.x = xMin;
            border.y = xMax;
            
            var g = gameObject.CREATE_Cube();
            g.transform.position = new Vector3(xMin, gameObject.GetChild(idMin).transform.position.y, 0);
            
            g = gameObject.CREATE_Cube();
            g.transform.position = new Vector3(xMax, gameObject.GetChild(idMax).transform.position.y, 0);
        }
    }

    interface IIslandBorder
    {
        Vector2 Border { get; }
    }
}