using UnityEngine;

namespace Galactic1.AbstractFactory
{
    public class _Timer
    {
        public _Timer()
        {
            Elapsed = 0;
        }

        private float elapsed;

        public float Elapsed
        {
            set => elapsed = value;

            get => elapsed += Time.deltaTime;
        }
    }
}