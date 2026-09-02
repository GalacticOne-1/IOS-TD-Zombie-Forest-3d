
using System.Collections;
using System.Collections.Generic;
using Galactic1.Mobile;
using UnityEngine;

namespace Galactic1.Core
{
    public class SDKStarter
    {
        
        private readonly Coroutines _coroutines;

        public readonly Dictionary<ESdkType, bool> _sdkCompleted = new();
        
        


        public SDKStarter(Coroutines coroutines)
        {
            _coroutines = coroutines;
            _sdkCompleted[ESdkType.Iap] = false;
            _sdkCompleted[ESdkType.Ad] = false;
        }







        public IEnumerator InitializeSDK(ISdk service)
        {
            while (true)
            {
                var sdkCompleted = false;
                var waiting = true;
                var w = new WaitForSeconds(.1f);
                
                var timer = 10f;
#if UNITY_EDITOR
                timer = .5f;
#endif

                // #1 запуск сдк
                service.SDKInitialize(() => { _sdkCompleted[service.SdkType] = true; });
                
                // #2 по истечении таймера, если сдк не подключен, делаем новый запуск
                while (waiting)
                {
                    // останавливаем корутину, не длжидаясь таймера
                    if(_sdkCompleted[service.SdkType]) yield break;
                    
                    if (timer <= 0)
                    {
                        // след. попытки подключить сдк делаем в скрытом режиме
                        if (!_sdkCompleted[service.SdkType])
                        {
                            ScreenProfiler.AddMessage($"SDKStarter : Can't get sdk type {service.SdkType}".SetText(EDlogColor.ORANGE));
                            DLog.Alert($"SDKStarter : Can't get sdk type {service.SdkType}",EDlogColor.ORANGE, AppConstants.show_log_core);
                            _coroutines.StartCoroutine(InitializeSDK(service));
                            yield break;
                        }
                    }
                    
                    timer -= .1f;
                    yield return w;
                }
            }
        }
        

    }
}