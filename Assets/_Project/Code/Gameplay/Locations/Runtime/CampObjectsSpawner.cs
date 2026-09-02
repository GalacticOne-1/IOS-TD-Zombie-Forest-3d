using UnityEngine;

namespace Galactic1.Gameplay.Locations.Utils
{
    /// <summary>
    /// Обёртка для процедур, которые ты делал в PlayerCamp_Loading:
    /// - спавн объектов базы
    /// - загрузка комнат и производства
    /// - дополнительные загрузки для обучения
    /// </summary>
    public class CampObjectsSpawner
    {
        public void SpawnCampObjects(LocationContext ctx, DIContainer container)
        {
            // Здесь вызываем существующие методы/классы:
            // GridController.I.Load();
            // ViewGameController.CampBonusViewModel.LoadObject() и т.д.
            // Для примера сделаем вызов тир-методов, которые у тебя уже есть:

            // (пример) — spawn static camp root if needed
            var env = ServiceLocator.Current.Get<Environment>();
            // env.playerSpawnPoint etc handled in loader

            // Дополнить: вызываем кастомные загрузчики
            // new PlayerCamp_Loading(); // если хочешь интегрировать старый код
        }
    }
}