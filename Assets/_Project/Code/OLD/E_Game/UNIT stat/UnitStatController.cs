using Galactic1;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Galactic1
{
    public class UnitStatController : MonoBehaviour, IGameService
    {

        public CUnitHead headGame, headInventory, creature;
        
        [System.Serializable] 
        public struct CUnitHead
        {
            public TextMeshProUGUI tName, tHp, tLevel;
            public Image hpBar;
        }

        [Space]
        public TextMeshProUGUI tEat;
        public TextMeshProUGUI tWater;

        public GameObject bAtck;
        public GameObject bAction;
        public GameObject bPocket1, bPocket2;
        public GameObject bDragon;
        public GameObject bJump;

        [Space] 
        public CExperienceStat experienceStat;
        [System.Serializable] 
        public struct CExperienceStat
        {
            public TextMeshProUGUI tLevel, tXp;
            public Image bar;
        }
        
        
        


        public void SetHp(float cur_hp, float max_hp)
        {
            cur_hp = Mathf.CeilToInt(cur_hp);
            headGame.tHp.text = $"{cur_hp}";
            headGame.hpBar.fillAmount = cur_hp / max_hp;
            headInventory.tHp.text = $"{cur_hp}";
            headInventory.hpBar.fillAmount = cur_hp / max_hp;
        }
        
        
        public void SetHp_Creature(float cur_hp, float max_hp, Color color)
        {
            cur_hp = Mathf.CeilToInt(cur_hp);
            creature.tHp.text = $"{cur_hp}";
            creature.hpBar.fillAmount = cur_hp / max_hp;

            creature.tHp.color = color;
            creature.hpBar.color = color;
        }
        
        
    }
}