using System;
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.UI.Shop;

namespace Galactic1.Window
{
    public static class WindowCardDataFactory
    {
        public static WindowCardData CreateCard(WindowCardInitialStateConfigs initialConfigs)
        {
            switch (initialConfigs.CardType)
            {
                
                case EWindowCardType.IAP:
                    return CreateCard<IAPCardData>(initialConfigs);
                //case EWindowCardType.Ad:
                    //return ...
                
                
                default:
                    throw new Exception("Not implemented window card creation: " + initialConfigs.CardType);
            }
        }



        // заполнение базовых полей для любой сущности
        static T CreateCard<T>(WindowCardInitialStateConfigs initialConfigs) where T : WindowCardData, new()
        {
            return CreateCard<T>(
                initialConfigs.CardType,
                initialConfigs.Id,
                initialConfigs.CardVariant);
        }
        
        // заполнение базовых полей для любой сущности
        static T CreateCard<T>(EWindowCardType type, IAPId configId, int cardVariant)
            where T : WindowCardData, new()
        {
            var cardData = new T
            {
                Type = type,
                ConfigId = configId.Guid,
                CardVariant = cardVariant
            };

            
            // заполнение остальных полей зависимых от конкретной карточки
            switch (cardData)
            {
                case IAPCardData iapData:
                    UpdateIapCard(iapData);
                    break;
                
                // ...
                
                default:
                    throw new Exception("Not implemented window card creation: " + type);
            }

            return cardData;
        }

        

        
        static void UpdateIapCard(IAPCardData iapCardData)
        {
            
        }
    }
}