using System;

namespace Galactic1.Systems.Server
{
    /// <summary>
    /// Глобальный источник серверного времени (в секундах)
    /// </summary>
    public static class ServerTime
    {
        private static double serverUnixTime;
        private static double localStartTime;

        /// <summary>
        /// Вызывается один раз после получения времени с сервера
        /// </summary>
        public static void Sync(double unixTimeSeconds)
        {
            serverUnixTime = unixTimeSeconds;
            localStartTime = UnityEngine.Time.realtimeSinceStartup;
        }

        /// <summary>
        /// Текущее серверное время (секунды)
        /// </summary>
        public static double Now =>
            serverUnixTime + (UnityEngine.Time.realtimeSinceStartup - localStartTime);
    }
}