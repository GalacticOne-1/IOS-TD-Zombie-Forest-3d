
using UnityEngine;

namespace Galactic1.RaidLoot.Authoring
{
    [CreateAssetMenu(
        fileName = "LocationGuaranteedProfileConfig",
        menuName = "Game Configs/Loot/Location Guaranteed Profile")]
    public sealed class LocationGuaranteedProfileConfig : ScriptableObject
    {
        // предметы которые игрок получает в любом случае за посещение локации
        // типо что бы игрок не уходил с пустыми руками (сейчас все каонфиги пустые, т.е этот лут не работает)
        // не связано с контейнерами в локации !!!
        [SerializeField]
        private LocationGuaranteedEntry[] _entries;
        public LocationGuaranteedEntry[] Entries => _entries;
        
    }
}