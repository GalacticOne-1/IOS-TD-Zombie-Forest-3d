using System;
using System.Collections.Generic;

namespace Galactic1.Configs
{
    /*
     *      Настройки состояния по умолчанию при первом запуске приложения
     *          - какие объекты изначально существуют в лагере игрока и что там вообще есть
     */
    
    [Serializable]
    public class LocationInitialStateConfigs
    {
        public List<EntityInitialStateConfigs> Entities;
    }
}