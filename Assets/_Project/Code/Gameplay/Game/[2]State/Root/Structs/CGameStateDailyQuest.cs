using System;

namespace Galactic1
{
    [Serializable]
    public struct CGameStateDailyQuest
    {
        public byte dailyQuestDay;                              // id дня для заданий
        public short dailyQuestWeek;                            // сколько недель пройдено по дневным квестам
        public byte dailyRewardState;
        public CSaveDailyQuest[] dailyQuest;
        public CSaveAchievement[] achievement;
    }
    
    [Serializable]
    public struct CSaveDailyQuest
    {
        public short id, id_previous;
        public byte state;
        public int progress;
    }
    
    [Serializable]
    public struct CSaveAchievement
    {
        public byte state;                  // состояние ачивка
        public byte stage;                  // вариант задания
        public long progress;               // текущее значение
    }
}