using System.Collections.Generic;
using Galactic1.Configs;


/*
 *  Список компонентов для отображения в полоске загрузки при старте приложения
 */



namespace Galactic1
{
    public static class LoadingManagerSteps
    {
        public static void CreateStepList(DIContainer rootContainer)
        {
            Dictionary<string, LoadingStep> steps = new();
            var configProvider = rootContainer.Resolve<IConfigProvider>();
            var entryPointConfig = configProvider.Get<ApplicationConfig>();

            //if (entryPointConfig.requiresServerConnection)
                //steps.Add(CServiceType.REMOTE_CONFIG, new LoadingStep(){ Description = "Loading configs from server" });
            
            if (entryPointConfig.isAppstore)
                steps.Add(CServiceType.PLAYER_PERMISSION, new LoadingStep(){ Description = "Waiting for the player" });
            
            if (entryPointConfig.requiresAnalyticsService)
                steps.Add(CServiceType.ANALYTICS, new LoadingStep(){ Description = "Loading analytics service" });
            
            if (entryPointConfig.requiresIapService)
                steps.Add(CServiceType.IAP, new LoadingStep(){ Description = "Connecting to the store" });
            
            if (entryPointConfig.requiresAdService)
                steps.Add(CServiceType.AD, new LoadingStep(){ Description = "Loading some services" });
            
            
            steps.Add(CServiceType.REGISTER_GLOBAL_SERVICES, new LoadingStep(){ Description = "Registering some services" });
            
            steps.Add(CServiceType.LOADING_MAIN_SCENE, new LoadingStep() { Description = "Preparing the game scene" });
            
            
            LoadingManager.I.Launch(steps);
        }
    }
}