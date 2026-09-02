using System;
using Galactic1.Localisation;
using UnityEngine;

namespace Galactic1
{
    public abstract class _AttributesConfigs_ : _EntityConfig_
    {

        #region FIELDS

        [Space] 
        [Header("Выбрать хар-ки")]
        public CAttributes[] attributes;
        [Serializable]
        public struct CAttributes
        {
            [Header("Выбрать нужный")]
            public StatId type;
            
            [Header("Применение свойства как значение или %")]
            public EAttributeEffect effect;
            
            [Header("Эффект в плюс или минус")]
            public EAttributeStatus status;
            
            [Header("Если имеется иконка")]
            public bool showIcon;
            
            [Header("Как отображать значение (99; 99%; 99/sec)")]
            public EAttributeEffectGUI effectGUI;
            
            [Header("Отбражение значка впереди значения (+99/-99)")]
            public bool showSymbol;
            
            [Range(0, 10000)]
            public int value;
        }
        public enum EAttributeEffect { value, percent }
        public enum EAttributeStatus { bonus, manus }
        public enum EAttributeEffectGUI { value, percent, per_sec }

        

        #endregion
        
        
        
        
        public override CStatGUI[] GetStatGUI()
        {
            if (attributes == null)
            {
                Debug.LogError($"Attributes NULL: {name}");
                return new CStatGUI[0];
            }
            
            
            var l = attributes.Length;
            var resp = new CStatGUI[l];
            for (int i = 0; i < l; i++)
            {
                resp[i] = new CStatGUI();
                resp[i].title = ServiceLocator.Current.Get<LocalisationService>().Data.attributes[(byte)attributes[i].type];

                // icon
                //resp[i].icon = attributes[i].showIcon
                    //? ServiceLocator.Current.Get<IconHub>().GetSpriteAttribute(attributes[i].type)
                    //: ServiceLocator.Current.Get<IconHub>()._null;
                
                // vaule
                var str = attributes[i].status == EAttributeStatus.bonus ? "+" : "-";
                if (!attributes[i].showSymbol) str = "";
                resp[i].value = attributes[i].effectGUI == EAttributeEffectGUI.value
                    ? $"{str}{attributes[i].value}"
                    : attributes[i].effectGUI == EAttributeEffectGUI.percent
                        ? $"{str}{attributes[i].value}%"
                        : $"{str}{attributes[i].value}/sec";
            }

            return resp;
        }
        
        
        /// <summary>
        /// Конкретный аттрибут
        /// </summary>
        /// <param name="type"></param>
        /// <param name="attr"></param>
        public void GetAttribute(StatId type, out CAttributes attr)
        {
            attr = new CAttributes();
            var l = attributes.Length;
            for (int i = 0; i < l; i++)
            {
                if (attributes[i].type == type) attr = attributes[i];
            }
        }


        /// <summary>
        /// Передача аттрибутов предмета 
        /// </summary>
        /// <param name="attribute_default">для % при загрузке []attribute</param>
        /// <param name="[]attribute">идет в работу</param>
        public void GetAttributes(float[] attribute_default, ref float[] attribute)
        {
            float value;
            var l = attributes.Length;
            for (int i = 0; i < l; i++)
            {
                //DLog.Alert($"attrribute {attributes[i].type}_{attribute_default[(int)attributes[i].type]}");
                // 1a обычное добавление своего значения
                if (attributes[i].effect == EAttributeEffect.value)
                {
                    value = attributes[i].value;
                        
                    if (attributes[i].status == EAttributeStatus.bonus)
                        attribute[(int)attributes[i].type] += value;
                    
                    else
                        attribute[(int)attributes[i].type] -= value;
                }
                
                // 1b увеличение через %. должно быть базовое значение!!!
                // EAttributeEffect.percent
                else if(attribute_default[(int)attributes[i].type] > 0)
                {
                    value = attribute_default[(int)attributes[i].type] * attributes[i].value.ToPercent();
                    //value = (int)value;
                    
                    if (attributes[i].status == EAttributeStatus.bonus)
                        attribute[(int)attributes[i].type] += value;
                    
                    else
                        attribute[(int)attributes[i].type] -= value;
                }
            }
            
            
            
            // ограничение макс значение
            if (attribute[(int)StatId.Dodge] > 30) attribute[(int)StatId.Dodge] = 30;
            if (attribute[(int)StatId.Accuracy] > 100) attribute[(int)StatId.Accuracy] = 100;
        }
        
        
    }
}