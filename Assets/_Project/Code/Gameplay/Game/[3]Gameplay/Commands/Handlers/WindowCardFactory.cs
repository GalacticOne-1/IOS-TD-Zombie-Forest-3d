using System;
using Galactic1.UI.Shop;

namespace Galactic1.Window
{
    public static class WindowCardFactory
    {
        
        /*
         *      Загрузка карточек
         */
        
        
        public static void InitializeCard<T>(WindowCardInitialStateConfigs config, T card)
        {
            switch (config)
            {
                case IAPConfig iapConfig:
                    
                    break;

                default:
                    throw new Exception("Not implemented window card inializing: " + config.CardType);
            }
        }

        
        
        
        
    }

    public static class WindowItemCardFactory
    {
        public static void InitializeCard<T>(WindowCardInitialStateConfigs config, T card)
        {
            
        }
    }
}