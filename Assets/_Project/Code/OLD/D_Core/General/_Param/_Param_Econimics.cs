using UnityEngine;

namespace Galactic1
{
    
    /*
     *      Экономика для обычных механик
     */






    public class PARAM_ECONOMICS_CostHeroSummon
    {
        public struct CCost
        {
            public short regular, hard;
        }
        
        /// <summary>
        /// Стоимость разного призыва героев
        /// </summary>
        /// <param name="list"></param>
        public PARAM_ECONOMICS_CostHeroSummon(out CCost[] list)
        {
            list = new CCost[]
            {
                new (){ regular = 10, hard = 20},       // 20
                new (){ regular = 50, hard = 45},       // 450
                new (){ regular = 100, hard = 100},     // 100
                new (){ regular = 250, hard = 250},     // 2500
            };
        }
    }




    public class PARAM_ECONOMICS_Construct
    {
        
    }
    
}