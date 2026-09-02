using System;
using UnityEngine;

namespace Galactic1.Runtime.Preview
{
    /// <summary>
    /// Описание превью объекта внутри atlas.
    /// </summary>
    [Serializable]
    public class PreviewEntry
    {
        public string id;
        public Rect pixelRect;
    }
}