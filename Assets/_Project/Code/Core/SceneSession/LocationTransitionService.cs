using System;
using Galactic1.Code.Systems.GameTime;

namespace Galactic1.Core.Systems
{
    public class LocationTransitionService : IGameService
    {
        public struct LocationEntry
        {
            public bool CampDefense;
            public bool ResetRootPlayerScene;
        }
        
        Action<int, LocationEntry> onLocationChanged;
        

        public LocationTransitionService(Action<int, LocationEntry> onLocationChanged)
        {
            this.onLocationChanged = onLocationChanged;
        }

        
        
        /*
         *  Использовать на карте или для тестов
         */
        
        /// <summary>
        /// Переход к новой локации по ID
        /// </summary>
        public void GoToLocation(int locationId, LocationEntry entry)
        {
            
#if UNITY_EDITOR
            DLog.Alert($"============== [LocationTransition] GoTo {locationId}", AppConstants.show_log_core);
#endif
            // сбрасываем скорость игры
            ServiceLocator.Current.Get<GameTimeScaleService>().Clear();

            // #1 записываем в глобальное состояние
            SetLocation(locationId);
            
            // #2 load scene
            onLocationChanged?.Invoke(locationId, entry);
        }

        /// <summary>
        /// Сохраняет занчение локации
        /// </summary>
        /// <param name="locationId"></param>
        public void SetLocation(int locationId)
        {
            ServiceLocator.Current.Get<IGameStateProvider>().GameStateProxy
                .GameLoopContext.PlayerOnMap.Value = locationId == -1;
            
            ServiceLocator.Current.Get<IGameStateProvider>().GameStateProxy
                .GameLoopContext.CurrentLocationStateId.Value = locationId;
        }

    }

}