
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;

namespace Galactic1.Dev
{
    public class DevUI : MonoBehaviour
    {
        public DevSpawner spawner;
        public GameObject spawnerWindow;
        public TMP_Dropdown dropdown;

        private string selectedConfig;

        
        
        void Start()
        {
            DevMode.Enabled.Subscribe(_ => { spawnerWindow.SetActive(_); });
            
            
            // Заполняем dropdown именами префабов
            dropdown.options.Clear();
            foreach (var configId in spawner.entityConfigs)
                dropdown.options.Add(new TMP_Dropdown.OptionData(configId));

            dropdown.onValueChanged.AddListener(OnPrefabSelected);

            if (spawner.entityConfigs.Count > 0)
                selectedConfig = spawner.entityConfigs[0];
        }

        void OnPrefabSelected(int index)
        {
            selectedConfig = spawner.entityConfigs[index];
        }

        void Update()
        {
            if (!DevMode.Enabled.Value) return;

            if (Input.GetKey(KeyCode.LeftShift) && Input.GetMouseButtonDown(0))
            {
                var pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                pos.z = 0;
                spawner.Spawn(selectedConfig, pos);
            }
        }
    }


}