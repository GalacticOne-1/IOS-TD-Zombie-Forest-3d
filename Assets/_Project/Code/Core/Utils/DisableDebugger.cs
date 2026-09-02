using UnityEngine;

public class DisableDebugger : MonoBehaviour
{
    private void OnDisable()
    {
        Debug.LogError($"{name} disabled\n{System.Environment.StackTrace}");
    }
}