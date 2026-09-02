using Galactic1.Mobile;
using UnityEngine;

namespace Galactic1
{
    public class ScreenSizeController
    {
        
        /*
         *    Адаптация размера экрана для мобильных устройств
         */
        
        
        public struct CResolution
        {
            public float target;
            public float cameraSize;
        }

        public static CResolution[] resolution =
        {    
            new CResolution()
            {
                target = 1200,
                cameraSize = 5.2f
            }, 
            new CResolution()
            {
                target = 1400,
                cameraSize = 5.2f
            }, 
            new CResolution()
            {
                target = 1920,
                cameraSize = 5.1f
            }, 
            new CResolution()
            {
                target = 2200,
                cameraSize = 4.7f
            }, 
            new CResolution()
            {
                target = 2400,
                cameraSize = 4.4f
            }, 
            new CResolution()
            {
                target = 2600,
                cameraSize = 4.5f
            }, 
        };

        
        /// <summary>
        /// Запускать при старте app
        /// </summary>
        public static void SetScreenSize()
        {
            //ScreenProfiler.AddMessage($"{Screen.width}x{Screen.height}");
            DLog.Alert($"{Screen.width}x{Screen.height}");
            var l = resolution.Length-1;
            for (int i = l; i >= 0; i--)
            {
                if (Screen.width >= resolution[i].target)
                {
                    Camera.main.orthographicSize = resolution[i].cameraSize;
                    break;
                }

            }
        }
        
        
    }
}