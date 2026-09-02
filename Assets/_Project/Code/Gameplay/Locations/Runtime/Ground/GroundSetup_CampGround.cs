
using UnityEngine;

namespace Galactic1
{
    public class GroundSetup_CampGround : GroundSetup
    {
        /*
         *      Для коллайдера земли в лагере
         */
        
        
        public override CData GetSetup()
        {
            return new CData()
            {
                xMin = transform.position.x - transform.localScale.x / 2,
                xMax = transform.position.x + transform.localScale.x / 2,
                y = transform.position.y + transform.localScale.y / 2
            };
        }
    }
}