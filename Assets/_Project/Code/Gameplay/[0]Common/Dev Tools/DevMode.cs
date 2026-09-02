
using UniRx;
using UnityEngine;


namespace Galactic1.Dev
{
    public class DevMode : MonoBehaviour
    {
        public static ReactiveProperty<bool> Enabled { get; private set; } = new(false);

        void Update()
        {
            // Включаем/выключаем DevMode клавишей F1
            if (Input.GetKeyDown(KeyCode.F1))
            {
                Enabled.Value = !Enabled.Value;
                Debug.Log($"DevMode: {(Enabled.Value ? "ON" : "OFF")}");
            }
        }
    }

}