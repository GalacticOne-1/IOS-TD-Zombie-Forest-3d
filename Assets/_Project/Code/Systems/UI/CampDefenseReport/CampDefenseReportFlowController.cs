using System;
using Galactic1.Code.Core;
using Galactic1.Code.Systems.GameLoop;
using Galactic1.Code.Systems.Raid;
using Galactic1.Code.UI.RaidReport;
using Galactic1.UI.Core;
using Galactic1.UI.Core.TabPanel;

namespace Galactic1.Code.UI.CampDefenseReport
{
    /// <summary>
    /// Упрощённый поток отчёта после защиты лагеря.
    /// Показывает только состояние выживших.
    /// </summary>
    public sealed class CampDefenseReportFlowController
    {
        private readonly GameLoopContext _context;
        private readonly UIScreenId _requiresScreen;

        public CampDefenseReportFlowController(GameLoopContext context, UIScreenId requiresScreen)
        {
            _context = context;
            _requiresScreen = requiresScreen;
        }

        public void StartFlow(
            RaidRuntime raidRuntime, 
            RaidResultProxy raidResult, 
            Action onClosed)
        {
            var data = BuildData(raidRuntime, raidResult);
            
            
            // ServiceLocator.Current.Get<TabPanelController>().EntryParam = new()
            // {
            //     HideTab = true
            // };
            
            
            ServiceLocator.Current.Get<UIManager>().OpenScreen(
                _requiresScreen,
                null,
                _ =>
                {
                    if (_requiresScreen == UIScreenId.CampDefenseReport)
                        _.GetComponent<CampDefenseReportController>().Show(
                            data,
                            onClosed);
                    else
                        _.GetComponent<CampDefenseMapReportController>().Show(
                            data,
                            onClosed);
                });
        }
        
        

        private RaidReportData BuildData(RaidRuntime raidRuntime, RaidResultProxy raidResult)
        {
            var data = CampDefenseReportMapper.Build(_context);
            data.RaidResult = raidResult;

            return data;
        }
    }
}