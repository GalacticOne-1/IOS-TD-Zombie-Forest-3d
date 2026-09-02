
#if UNITY_EDITOR
using Galactic1;
using UnityEditor;
using UnityEngine;

public static class AstarHotkeyScan
{
    // %#s = Ctrl/Cmd + Shift + S
    [MenuItem("Tools/A*/Scan Grid %#s")]
    public static void ScanGrid()
    {
        if (AstarPath.active == null)
        {
            DLog.Alert("AstarPath.active is null — нет объекта AstarPath на сцене.", EDlogColor.ORANGE);
            return;
        }

        AstarPath.active.Scan();
        Debug.Log("A* grid scanned.");
    }
}

#endif