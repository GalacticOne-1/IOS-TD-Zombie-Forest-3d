using Galactic1;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace Galactic1
{
    public class NewLevelView : MVVMView, IWidgetClose
    {
        [SerializeField] private GameObject[] pBox;
        public GameObject[] PBox => pBox;



        [SerializeField] private TextMeshProUGUI tLevel, tH2;
        public TextMeshProUGUI TLevel => tLevel;
        public TextMeshProUGUI TH2 => tH2;


        [Space]
        [SerializeField] private GameObject holdReward;
        public GameObject HoldReward => holdReward;

        [Space] 
        [SerializeField] private GameObject cBlueprints;
        public GameObject CBlueprints => cBlueprints;


        [Space] 
        [SerializeField] private GameObject bConfirm;
        [SerializeField] private GameObject cBonusDeal, bCloseDeal, bAds;

        public GameObject BConfirm => bConfirm;

        public GameObject CBonusDeal => cBonusDeal;

        public GameObject BCloseDeal => bCloseDeal;
        public GameObject BAds => bAds;
        
        
        public void RequireClosing() => (presenter as NewLevelViewModel).CloseWindow();
    }
}