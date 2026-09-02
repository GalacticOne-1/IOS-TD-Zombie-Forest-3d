using Galactic1;
using UnityEngine;

namespace Galactic1
{

    public interface ICreatureStat
    {
        string Title { get; }
        float Cur_hp { get; }
        float Max_hp { get; }
        
        bool Animal { get; }
    }
    
    
    
    public class UpdateCreatureStat
    {
        /// <summary>
        /// Стата для существа - цели
        /// <br/>(Вверху экрана)
        /// </summary>
        /// <param name="creatureStat"></param>
        public UpdateCreatureStat(ICreatureStat creatureStat)
        {
            // ServiceLocator.Current.Get<UnitStatController>().creature.tName.transform.parent.gameObject.SetActive(true);
            // ServiceLocator.Current.Get<UnitStatController>().creature.tName.text = creatureStat.Title;
            // ServiceLocator.Current.Get<UnitStatController>().SetHp_Creature(creatureStat.Cur_hp, creatureStat.Max_hp, 
            //     ServiceLocator.Current.Get<IconHub>().SystemColor(creatureStat.Animal ? EColor.yellow : EColor.red));
        }
    }

    public class UpdateCreatureStat_Cancel
    {
        public UpdateCreatureStat_Cancel()
        {
            //ServiceLocator.Current.Get<UnitStatController>().creature.tName.transform.parent.gameObject.SetActive(false);
        }
    }
}