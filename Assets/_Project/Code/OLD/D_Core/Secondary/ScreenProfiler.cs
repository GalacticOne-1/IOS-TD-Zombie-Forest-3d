using System.Collections.Generic;
using Galactic1;
using TMPro;
using UnityEngine;

namespace Galactic1.Mobile
{
    public class ScreenProfiler
    {
        private static bool Active;
        public static TextMeshProUGUI tAlert;
        public static List<string> arMessage;



        public static void Init(bool active)
        {
            Active = active;
            tAlert = GameObject.Find("Screen_profiler").GetComponent<TextMeshProUGUI>();
            arMessage = new List<string>();
        }


        public static void Clear() => arMessage.Clear();
        
        
        public static void AddMessage(string t)
        {
            if (!Active) return;
            
            arMessage.Add(t);
            if(arMessage.Count > 20)
                arMessage.RemoveAt(0);

            tAlert.text = "";
            var l = arMessage.Count;
            for (int i = 0; i < l; i++)
            {
                tAlert.text += $"{arMessage[i]} \n";
            }
        }


        public static void ClearMessage()
        {
            arMessage.Clear();
            tAlert.text = "";
            var l = arMessage.Count;
            for (int i = 0; i < l; i++)
            {
                tAlert.text += $"{arMessage[i]} \n";
            }
        }



    }
}