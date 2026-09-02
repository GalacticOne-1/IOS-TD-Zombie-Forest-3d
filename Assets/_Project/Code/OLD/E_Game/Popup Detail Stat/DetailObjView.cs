using System;
using Galactic1;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Galactic1
{
    public class DetailObjView : MVVMView
    {
        [SerializeField] private TextMeshProUGUI tTitle, tDes;

        public TextMeshProUGUI TTitle => tTitle;

        public TextMeshProUGUI TDes => tDes;


        [SerializeField] private GameObject cStat;

        public GameObject CStat => cStat;
    }
}