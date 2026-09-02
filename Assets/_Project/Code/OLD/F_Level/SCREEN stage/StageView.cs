using Galactic1;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Galactic1
{
    public class StageView : MVVMView
    {
        [SerializeField] private GameObject holdStage;

        public GameObject HoldStage => holdStage;

        [SerializeField] private float durationMovement;

        public float DurationMovement => durationMovement;


        [SerializeField] private TextMeshProUGUI tWave;

        public TextMeshProUGUI TWave => tWave;

        [SerializeField] private Image fillWave;

        public Image FillWave => fillWave;
    }
}