using Galactic1.UI.Core;
using UnityEngine;

namespace Galactic1.Configs.WorldMap
{
    [CreateAssetMenu(
        fileName = "RaidReportIconConfig",
        menuName = "Game Configs/Style/Raid Report Icon Config"
    )]
    public class RaidReportIconConfig : StyleConfigBase
    {
        [SerializeField] private Sprite chargeActiveSprite;
        [SerializeField] private Sprite chargeEmptySprite;


        public Sprite ChargeActiveSprite => chargeActiveSprite;

        public Sprite ChargeEmptySprite => chargeEmptySprite;
    }
}