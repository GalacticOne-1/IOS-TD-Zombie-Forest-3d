using System;
using System.Collections;
using Galactic1.Mobile;
using Galactic1.Core;
using UnityEngine;
using UnityEngine.Networking;

namespace Galactic1
{
    public class TimeManagement
    {
        
        /*
         *    Запуск компонентов зависящих от пропущеного времени в игре
         */

        //private static string url = "https://galactic1games.com/go.php";
        // https://galactic1games.com/go.php
        // https://naturalkin.ru/bitrix/templates/naturalkin/go.php

        /// <summary>
        /// Current (+1 every sec MonobeheviuorMaster)
        /// </summary>
        public static long currDateInSeconds;                               // дата и время в секундах (current)
        /// прошедшее время в секундах
        public static long passedTimeInSeconds { get; private set; }

        public static long timeEnter { get; private set; }
        private static long timeQuit;                                       // время когда игрок вышел из игры
        
        // прошедшее время со дня первого запуска
        private static long passedTimeFromLaunchDay => 0;// (currDateInSeconds - SaveManagement.I.gameData.dateLaunch);
        /// true - нужное кол-во дней прошло
        public static bool PassedDaysFromFirstLaunch(int days)
            => (DeveloperConsole.I.game.passedDay * dayInSeconds) + passedTimeFromLaunchDay > dayInSeconds * days;
        
        
        public static bool COMPLETE = false;
        public const int dayInSeconds = 86400;
        public const int hourInSeconds = 3600;
        

        /// <summary>
        /// true - время вышло
        /// </summary>
        public static bool TimeComplete(long finish) => currDateInSeconds > finish;
        /// <summary>
        /// Вернет время окончания процесса
        /// </summary>
        /// <param name="duration">seconds</param>
        /// <returns></returns>
        public static long GetTimeFinish(int duration) => currDateInSeconds + duration;
        /// <summary>
        /// Вернет оставшееся время
        /// </summary>
        /// <param name="duration">seconds</param>
        /// <returns></returns>
        public static int GetRemainingTime(long finish) => (int)(finish - currDateInSeconds);
        /// <summary>
        /// Вернет оставшееся время
        /// </summary>
        /// <param name="duration">seconds</param>
        /// <returns></returns>
        public static short GetTimeLeft_SHORT(long finish) => (short)(finish - currDateInSeconds);

        public static int GetPassTime(long finish) => (int) (finish - timeQuit);
        
        /// <summary>
        /// Вернет оставшееся время loop 24 hour
        /// </summary>
        /// <returns></returns>
        //public static int GetTime_loop24() => (int)(ServiceLocator.Current.Get<IGameStateProvider>().GameStateProxy.Time.Value.loop_24h - currDateInSeconds);

        public static long GetTime_AddDay() => currDateInSeconds + dayInSeconds;
        
        
        
        
        
        public static IEnumerator CheckDailyTime(DFunc onComplete = null, DFunc onFail = null) 
        {
            COMPLETE = false;
            //DLog.Alert("Start daily time","yellow");
            //ScreenProfiler.AddMessage("Start daily time");
            UnityWebRequest www = UnityWebRequest.Get(ApplicationSetup.I.Url_time_server);
            yield return www.SendWebRequest();
            //ScreenProfiler.AddMessage("Hoorey!");
            if (www.isNetworkError || www.isHttpError)
            {
                //DLog.Alert("Daily time error","orange");
                //ScreenProfiler.AddMessage("Daily time error");
                onFail?.Invoke();
                // блокируем все компоненты зависящие от времени
                
                yield break; 
            }

            
            

            COMPLETE = true;
            //currTime = ParseDateTime(www.text);
            var d = www.downloadHandler.text.Split('-');
            Debug.Log(www.downloadHandler.text);
            //Debug.Log(" ========= "+Regex.Match(www.text,@"\d{2}.\d{2}.\d{2}").Value);

            var _log = "=======================";
            _log += $"\nGame time: {currDateInSeconds.FormatTimeLong()}";

            // переводим в секунды
            currDateInSeconds =
                (int.Parse(d[0]) * 365 + int.Parse(d[1])) * 24 + int.Parse(d[2]);                 // hours
            currDateInSeconds = currDateInSeconds * 60 + int.Parse(d[3]);                         // min
            currDateInSeconds = currDateInSeconds * 60 + int.Parse(d[4]);                         // sec
            
            _log += $"\nServer time: {currDateInSeconds.FormatTimeLong()}";
            _log += "\n=========================";
            ScreenProfiler.AddMessage(_log.SetText(EDlogColor.BLUE));

            timeEnter = currDateInSeconds;
            
            Debug.Log(currDateInSeconds);
            if (_GameState.FirstStart)
            {
                passedTimeInSeconds = 0;
                //SaveManagement.I.gameData.dateLaunch = currDateInSeconds;
                //ServiceLocator.Current.Get<IGameStateProvider>().GameStateProxy.Time.Value.loop_24h = currDateInSeconds + dayInSeconds;
                ServiceLocator.Current.Get<IGameStateProvider>().GameStateProxy.Server.Value =
                    ServiceLocator.Current.Get<IGameStateProvider>().GameStateProxy.Server.Value;
                
                //PlayerPrefs.SetString("dayInGame", (currDateInSeconds + dayInSeconds).ToString());
                SaveCurrTime();
                timeQuit = currDateInSeconds;
                onComplete?.Invoke();
                CORT.BlockScreen(false);
                
                yield break;
            }
            
            
            
            // если надо чтобы прошли сутки
            if (DeveloperConsole.I.game.passDay)
            {
                currDateInSeconds += dayInSeconds * DeveloperConsole.I.game.passedDay;
            }
            //
            
            
            //if(SaveManagement.I.gameData.dateLaunch == 0)
                //SaveManagement.I.gameData.dateLaunch = currDateInSeconds;
            
            //ScreenProfiler.AddMessage("--------------------Y");
            if (PlayerPrefs.GetString("lastTimeInGame") == "") SaveCurrTime();
            timeQuit = long.Parse(PlayerPrefs.GetString("lastTimeInGame"));
            passedTimeInSeconds = currDateInSeconds - timeQuit;

            
            
            SaveCurrTime();
            SaveDayTime();
            // -------------------
            
            
            
            // ------------------- ниже добавлять методы которые нужно запускать после получения времени
            onComplete?.Invoke();
            
            
            
            
            CORT.BlockScreen(false);
            //ScreenProfiler.AddMessage("Daily time: COMPLETE");
        }

        // сохраняем для отслеживания времени вне игры
        public static void SaveCurrTime()
        {
            if(COMPLETE)
            PlayerPrefs.SetString("lastTimeInGame", currDateInSeconds.ToString());
        }

        static void SaveDayTime()
        {

            // if (ServiceLocator.Current.Get<IGameStateProvider>().GameStateProxy.Time.Value.loop_24h == 0)
            //     ServiceLocator.Current.Get<IGameStateProvider>().GameStateProxy.Time.Value.loop_24h = currDateInSeconds + dayInSeconds;
            //
            // // обновляем если прошло 24 часа
            // if (currDateInSeconds >= ServiceLocator.Current.Get<IGameStateProvider>().GameStateProxy.Time.Value.loop_24h)
            // {
            //     DLog.Alert("Passed day!");
            //     
            //     // что бы сутки считались с одного времени, а не тогда когда игрок зашел в игру
            //     //var diff = currDateInSeconds - GAMEPLAY.DataGamestat().loop_24h;
            //     ServiceLocator.Current.Get<IGameStateProvider>().GameStateProxy.Time.Value.loop_24h = currDateInSeconds + dayInSeconds;
            //     ServiceLocator.Current.Get<IGameStateProvider>().GameStateProxy.Time.Value =
            //         ServiceLocator.Current.Get<IGameStateProvider>().GameStateProxy.Time.Value;
            //     
            //     //ServiceLocator.Current.Get<AdController>().ResetAdLimit();
            //     //ServiceLocator.Current.Get<LibController>().dailyQuest.UpdateDailyQuest();
            //     new ServerTimeConfigs().Reset_24h();
            //     //Lottery.ResetSpin();
            //     
            //     // ------
            //     //GAMEPLAY_old.Saving();
            // }
        }
        
    }
    
    
    
}