using System;
using System.Collections.Generic;
using Galactic1.AbstractFactory;
using UnityEngine;

namespace Galactic1
{
    public class RepositoryUtility : Singleton<RepositoryUtility>
    {
        [SerializeField] private string key;
        private Dictionary<string, string> log;


        public void AddLog(string key, string message)
        {
            // if (log == null)
            //     log = new();
            //
            // var allBuilds = EntityRepository.GetAllOfType<BuildEntity>();
            // foreach (var b in allBuilds)
            // {
            //     if (b.UniqueId == key && (b.STATE != EUnitStateType.DIE || b.gameObject.activeSelf))
            //     {
            //         DLog.Alert("****************************************************************", EDlogColor.ORANGE);
            //         DLog.Alert($"Проверить объект : {b.name}", EDlogColor.ORANGE);
            //         Debug.Log("obj >>> ", b);
            //     }
            // }
            //
            // if (!log.ContainsKey(key))
            //     log.Add(key, message);
        }




        public void ShowAllLogs()
        {
            //GConsole.ClearLog();
            DLog.Alert("*** Логи удаления!");
            if(log != null)
                foreach (var l in log)
                {
                    DLog.Alert(l.Key + "\n" + l.Value, EDlogColor.YELLOW);
                }
        }

        private void Update()
        {
            if (Input.GetKey(KeyCode.Space) && Input.GetKeyDown(KeyCode.F))
            {
               // GConsole.ClearLog();
                if(log != null)
                    foreach (var l in log)
                    {
                        if (l.Key == key)
                        {
                            DLog.Alert("*** Найден лог удаления!");
                            DLog.Alert(l.Key + "\n" + l.Value);
                        }
                    }
            }
            
            if (Input.GetKey(KeyCode.Space) && Input.GetKeyDown(KeyCode.A))
            {
                ShowAllLogs();
            }
        }
    }
}