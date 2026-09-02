using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace Galactic1
{
    public class FinishLevelView : MVVMView
    {
        [SerializeField] private TextMeshProUGUI tStatus;
        [SerializeField] private TextMeshProUGUI tNight;


        public TextMeshProUGUI TStatus => tStatus;

        public TextMeshProUGUI TNight => tNight;


        [Space] [SerializeField] private GameObject circle;
        [SerializeField] private GameObject holdReward, holdReward2;
        [SerializeField] private GameObject bgBonus, closeBonus;

        public GameObject Circle => circle;
        public GameObject HoldReward => holdReward;

        public GameObject HoldReward2 => holdReward2;

        public GameObject BgBonus => bgBonus;

        public GameObject CloseBonus => closeBonus;


        [Space] 
        [SerializeField] private GameObject bConfirm;
        [SerializeField] private GameObject bAds;
        [SerializeField] private GameObject cDeal, bDealClose;

        public GameObject BConfirm => bConfirm;
        public GameObject BAds => bAds;

        public GameObject CDeal => cDeal;

        public GameObject BDealClose => bDealClose;
    }
}