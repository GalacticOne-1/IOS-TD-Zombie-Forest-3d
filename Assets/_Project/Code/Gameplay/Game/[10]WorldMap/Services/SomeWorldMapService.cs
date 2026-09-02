using System;

namespace Galactic1
{
    public class SomeWorldMapService : IDisposable
    {
        // private readonly SomeProjectService _someProjectService;
        //
        // public SomeWorldMapService(SomeProjectService someProjectService)
        // {
        //     _someProjectService = someProjectService;
        //     DLog.Alert($"{GetType().Name} has been created", AppConstants.show_log_structure);
        // }

        public void Dispose()
        {
            DLog.Alert($"Clear all subscriptions",EDlogColor.YELLOW, AppConstants.show_log_core);
        }
    }
}