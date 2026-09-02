
using System.Collections.Generic;
using Galactic1.Code.Gameplay.Units;
using Galactic1.Code.Inventory.Abstractions;
using Galactic1.Gameplay.Player;
using Galactic1.Core.Systems.GameSession.WorldMap;
using Galactic1.Core.UI.HUD;
using Galactic1.Gameplay.Locations;
using UnityEngine;

namespace Galactic1.Core.GameSession
{
    /// <summary>
    /// ГЛОБАЛЬНЫЙ КОНТЕКСТ СЕССИИ — общие ссылки на игрока, камеру, HUD, спавнеры, AI.
    /// Он содержит все игровые объекты, необходимые в течение всей сессии
    /// Передаётся другим системам для удобного доступа.
    /// </summary>
    public class SceneSessionDefinition
    {
        
        
        // Локация
        public WorldMapContext WorldMapContext;
        public LocationContext LocationContext;
        
        public PlayerSpawnPreset PlayerSpawnPreset;
        
        // Игрок и данные
        public IInventoryResourcesPort InventoryPort;
        public TransportInstance Transport;
        public List<SurvivorInstance> Survivors;    // отряд игрока
        public List<SurvivorInstance> CampUnits;    // оставшиеся юниты в лагере    
        //public PlayerController Player;
        //public DragonController Dragon;
        

        // Камера / UI
        public Vector3 CameraPosition;
        public CameraFollow Camera;
        public HUDPlayer HUD;

        // Системы уровня
        //public Systems.Spawn.SpawnManager SpawnManager;
        //public Systems.AI.AIManager AIManager;
        //public Gameplay.Interaction.InteractionManager InteractionManager;
    }
}