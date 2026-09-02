
using UnityEngine;
using UnityEngine.UI;

public class AlphaButton : MonoBehaviour
{
    public float AlphaThreshold = 0.1f; // 0 - непрозрачный

    void Start()
    {
        GetComponent<Image>().alphaHitTestMinimumThreshold = AlphaThreshold;
    }
}
