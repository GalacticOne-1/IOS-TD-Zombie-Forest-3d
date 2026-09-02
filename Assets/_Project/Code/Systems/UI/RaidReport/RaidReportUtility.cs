using System.Collections.Generic;
using Galactic1.Code.Systems.GameLoop;
using Galactic1.Code.Systems.Runtime;
using Galactic1.Core.Systems.GameLoopSession;
using Galactic1.UI.CharacterPreview;
using Galactic1.UI.Core;
using Galactic1.UI.Text;
using UnityEngine;

namespace Galactic1.Code.UI.RaidReport
{
    public static class RaidReportUtility
    {

        /// <summary>
        /// Рапорт состояния отряда
        /// </summary>
        /// <returns></returns>
        public static List<RaidSurvivorResult> Survivors(IEnumerable<UnitRuntime> units)
        {
            var portraitCache = ServiceLocator.Current.Get<CharacterPortraitCache>();
            UIStyleResolver style = ServiceLocator.Current.Get<UIStyleResolver>();
            
            // survivors
            var survivors = new List<RaidSurvivorResult>();

            // === для отображения текущего хп у живых
            foreach (var unit in units)
            {
                if (!unit.Stats.IsDead)
                {
                    var value01 = unit.Stats.CurrentHP / unit.Stats.MaxHP;
                    var status = "HP " + TextBuilder.Start()
                        .Color(style.ResolveValueColor(ValueRangeType.Health, value01))
                        .Size(90)
                        .Text(Mathf.FloorToInt(unit.Stats.CurrentHP).ToString())
                        .End() // size
                        .End() // color
                        .Text("/")
                        .Size(80)
                        .Text(unit.Stats.MaxHP.ToString())
                        .End();
                    
                    survivors.Add(new()
                    {
                        RenderPortrait = portraitCache.GetPortrait(unit.ArchetypeId),
                        Name = unit.DisplayName,
                        Status = status
                    });
                }

                else
                {
                    var status = TextBuilder.Start()
                        .Color(Color.red)
                        .Text("Dead")
                        .End()
                        .ToString();
                
                    survivors.Add(new()
                    {
                        RenderPortrait = portraitCache.GetPortrait(unit.ArchetypeId),
                        Name = unit.DisplayName,
                        Status = status
                    });
                }
            }

            return survivors;
        }
    }
}