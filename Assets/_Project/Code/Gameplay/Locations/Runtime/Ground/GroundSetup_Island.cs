using UnityEngine;

namespace Galactic1
{
    public class GroundSetup_Island : GroundSetup
    {
        /*
         *      Для коллайдеров на островах
         */
        
        
        public override CData GetSetup()
        {
            // вытаскиваем границы из контроллера острова
            var border = transform.parent.GetComponent<IIslandBorder>().Border;
            
            
            return new CData()
            {
                //xMin = transform.position.x - transform.localScale.x / 2,
                //xMax = transform.position.x + transform.localScale.x / 2,
                xMin = border.x,
                xMax = border.y,
                y = transform.position.y + transform.localScale.y / 2
            };
        }
    }
}