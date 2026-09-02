
using UnityEngine;
using UnityEngine.Serialization;

namespace Galactic1.Localisation
{
    public class LocalisationService : MonoBehaviour, IGameService
    {
        [Header("Загруженный язык")]
        public CLoc Data { get; private set; }




        /// <summary>
        /// Загрузка файла локализации
        /// </summary>
        /// <param name="type"></param>
        public void LoadLanguage()
        {
            DataSaver.I.onContinue = data =>
            {
                Data = DataSaver.I.ConvertData<CLoc>(data);
            };
            
            DataSaver.I.LoadData($"Localisation/en.json");
        }

    }
    
    [System.Serializable]
    public struct CLoc
    {
        public string[] options;
        
        public CLocRegular[] default_text;
        public string ad_disabled, ad_restoring;
        
        public string pause, defeat, victory;
        
        // ----------------- ^ DEFAULT ^ -------------------



        public string wave;
        
        // all static buttons, panels, etc
        public CLocRegular[] game;

        // units
        public string[] class_unit;
        public string[] attributes;
        public string[] variant_attack;
        public string[] variant_assist;

        // screen
        public string[] constructWidgetMark;


        public string inventory_full;
        public string not_space;
        public string tool_broken, weapon_broken, armor_broken;

        public string dragon_too_far;
        public string ground_too_far;

        // LEVEL
        public CLocRegular[] level;

    }
    
    
    [System.Serializable]
    public struct CLocRegular
    {
        public string key;
        public string value;
    }
    

    

}