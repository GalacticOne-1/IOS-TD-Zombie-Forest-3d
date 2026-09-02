using System.Collections;
using Galactic1.Configs;
using Galactic1.Mobile;
using Galactic1;
using Unity.Advertisement.IosSupport;
using UnityEngine;
#if UNITY_IOS
// Include the IosSupport namespace if running on iOS:
using Unity.Advertisement.IosSupport;
#endif

namespace Galactic1.Core
{
    public class Config
    {

        /*
         *      Для запроса разрешений
         */
        

        public Coroutine CheckRequest(Coroutines coroutines, CIOS ios)
        {
#if UNITY_IOS && !UNITY_EDITOR
            return coroutines.StartCoroutine(CHECK(ios));
#else
            return null;
#endif
        }

        IEnumerator CHECK(CIOS iosRequest)
        {
            yield return null;
            var w = new WaitForSeconds(.1f);
            
            /*
             *      Проходим по разрешениям и делаем запросы
             */
            
            
            // #1 ATT gdpr
            if(iosRequest.requiresATT)
            {
                ScreenProfiler.AddMessage($"ATT >>> {ATTrackingStatusBinding.GetAuthorizationTrackingStatus()}");
                if(ATTrackingStatusBinding.GetAuthorizationTrackingStatus() == ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED) 
                {
                    ATTrackingStatusBinding.RequestAuthorizationTracking();
                    
                    ScreenProfiler.AddMessage($"ATT wait response: {ATTrackingStatusBinding.GetAuthorizationTrackingStatus()} ");
                    while (ATTrackingStatusBinding.GetAuthorizationTrackingStatus() == ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED)
                    {
                        yield return w;
                    }
                }
            }
            // result
            ServiceLocator.Current.Get<ConfigProvider>().Get<GameConfig>().SetStatusATT = 
                (byte)ATTrackingStatusBinding.GetAuthorizationTrackingStatus();
            
            
            // #2 ....
            
            
            
            
            
            ScreenProfiler.AddMessage(">>> Request Permission Complete!");
            DLog.Alert(">>> Request Permission Complete!");
        }
    }
}