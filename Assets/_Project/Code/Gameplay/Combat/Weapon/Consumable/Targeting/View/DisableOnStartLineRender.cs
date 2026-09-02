using System;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Targeting
{
    public class DisableOnStartLineRender : MonoBehaviour
    {
        private void Start()
        {
            GetComponent<LineRenderer>().enabled = false;
        }
    }
}