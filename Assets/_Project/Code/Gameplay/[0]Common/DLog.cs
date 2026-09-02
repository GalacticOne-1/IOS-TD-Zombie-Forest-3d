using UnityEngine;

namespace Galactic1
{
    public class DLog
    {
        public static string[] color = new []
        {
            "lime",
            "yellow",
            "orange",
            "red",
            "green",
            "#70C1FB"
        };


        public static void Alert(string mes)
        {
            Debug.Log($"<color=lime>{mes}</color>");
        }
        
        public static void Alert(string mes, bool showLog = true)
        {
            Alert(mes, EDlogColor.LIME, showLog);
        }
        
        public static void Alert(string mes, EDlogColor color, bool showLog = true)
        {
            if (showLog)
                Debug.Log($"<color={DLog.color[(byte)color]}>{mes}</color>");
        }
        
        public static void Alert(string mes, EDlogColor n, byte unitLog)
        {
            if (DeveloperConsole.I.game.unitLog == 0 || DeveloperConsole.I.game.unitLog == unitLog)
                Alert(mes, n);
        }

        public static void Alert(string mes, EDlogColor n, byte unitLog, bool onlyThis)
        {
            if (DeveloperConsole.I.game.showAllLogs)
                Alert(mes, n, unitLog);
            
            else if (onlyThis)
                Alert(mes, n);
        }
        
    }

    public enum EDlogColor
    {
        LIME = 0,
        YELLOW = 1,
        ORANGE = 2,
        RED = 3,
        GREEN,
        BLUE
    }

}