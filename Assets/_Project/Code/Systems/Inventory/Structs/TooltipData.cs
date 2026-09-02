using System.Collections.Generic;
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.UI.Core;

namespace Galactic1.UI
{
    public class TooltipData
    {
        public string title;
        public string description;
        public string itemType;
        public string rarity;
        public string tier;

        public List<TooltipDataField> stats = new();
        public List<TooltipDataField> storage = new();
        public List<TooltipDataField> extra = new();

        public StatStyleEntry linkedItemStyle;
        public List<RuntimeId> linkedItems = new();
    }

    public struct TooltipDataField
    {
        public string label;
        public string value;
        public TooltipDataFieldStyle Style;
    }

    public enum TooltipDataFieldStyle
    {
        Default,    // обычный текст
        Bold,       // жирный
        Orange,     // оранжевый цвет
        Green,      // зеленый + стрелка вверх
        Red,        // красны + стрелка вниз
    }
}