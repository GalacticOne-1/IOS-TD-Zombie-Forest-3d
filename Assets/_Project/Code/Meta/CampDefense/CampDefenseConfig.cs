
using UnityEngine;

namespace Galactic1.Code.Gameplay.CampDefense
{
    [CreateAssetMenu(
        fileName = "CampDefenseConfig",
        menuName = "Game Configs/Camp/Camp Defense Config")]
    public sealed class CampDefenseConfig : ScriptableObject
    {
        [SerializeField] private int campHpDefault = 200;


        public int CampHpDefault => campHpDefault;
    }
}