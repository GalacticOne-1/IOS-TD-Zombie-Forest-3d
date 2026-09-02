using System.Collections;
using UnityEngine;

namespace Galactic1
{





    public class SERVER_Connect
    {
        public SERVER_Connect()
        {
            ApplicationSetup.I.serverConnect.SetActive(true);
            ServiceLocator.Current.Get<CoroutineController>().StartCoroutine(connect());
        }

        IEnumerator connect()
        {

            for (int i = 0; i < 10; i++)
            {
                DLog.Alert($"REMOTE CONFIG TRY CONNECT {i}", EDlogColor.YELLOW);
                
                // 1 делаем запрос
                var corSerever = ServiceLocator.Current.Get<CoroutineController>().StartCoroutine(
                    TimeManagement.CheckDailyTime(() =>
                        {
                            //IAPOffer.I.Load();
                            //IAPConvert.I.LoadStarterPack();
                            //OfflineReward.I.CheckOfflineReward();
                            //RefController.I.dataBase.gameStat.CheckGoldBonus();
                            //Economica.CalculateMoney();
                            //ADSBlockManager.I.CheckTimer();
                        }));

                // 2 ждем
                yield return new WaitForSeconds(ApplicationSetup.I.tryConnectWait);

                // 3 проверяем подключение
                if (TimeManagement.COMPLETE)
                {
                    new SERVER_ConnectComplete();
                    yield break;
                }

                ServiceLocator.Current.Get<CoroutineController>().StopCoroutine(corSerever);
            }

            if (!TimeManagement.COMPLETE)
            {
                new SERVER_ConnectError();
                yield break;
            }
        }
    }


    public class SERVER_Config
    {
        // private readonly Coroutines _coroutine;
        // private readonly IConfigsProvider__ _configsProvider;
        // private readonly ServerConnectParams _serverConnectParams;
        //
        // public SERVER_Config(Coroutines coroutine, IConfigsProvider__ configsProvider, ServerConnectParams serverConnectParams)
        // {
        //     _coroutine = coroutine;
        //     _configsProvider = configsProvider;
        //     _serverConnectParams = serverConnectParams;
        //     //ApplicationSetup.I.serverConnect.SetActive(true);
        // }
        //
        // public Coroutine Loading() => _coroutine.StartCoroutine(connect());
        //
        // IEnumerator connect()
        // {
        //     for (int i = 0; i < 10; i++)
        //     {
        //         DLog.Alert($"REMOTE CONFIG TRY CONNECT {i}", EDlogColor.YELLOW, AppConstants.show_log_structure);
        //         // 1 делаем запрос
        //         var corSerever = _coroutine.StartCoroutine(_configsProvider.LoadGameSettings());
        //
        //         // #2 ждем
        //         for (float w = _serverConnectParams.waitConnect; w >= 0; w -= Time.deltaTime)
        //         {
        //             // #3 проверяем подключение
        //             if (_configsProvider.Complete)
        //             {
        //                 break;
        //             }
        //             
        //             yield return null;
        //         }
        //
        //         if (_configsProvider.Complete)
        //             break;
        //
        //         _coroutine.StopCoroutine(corSerever);
        //     }
        //
        //     if (!_configsProvider.Complete)
        //     {
        //         new SERVER_ConnectError();
        //         yield break;
        //     }
        //     
        // }
    }
    
    
    


    public class SERVER_ConnectComplete
    {
        public SERVER_ConnectComplete()
        {
            //ApplicationSetup.I.serverConnect.SetActive(false);
        }
    }

    public class SERVER_ConnectError
    {
        public SERVER_ConnectError()
        {
            DLog.Alert($"server : connect error", EDlogColor.ORANGE);
            //ApplicationSetup.I.serverConnect.SetActive(false);
            //ApplicationSetup.I.serverError.SetActive(true);
        }
    }
}