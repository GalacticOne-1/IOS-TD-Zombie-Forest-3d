using UnityEngine;

namespace Galactic1.Code.Systems.Tutorial.Authoring
{
    /// <summary>
    /// Базовый класс конфигурации одного объектива тутора. Содержит ТОЛЬКО данные —
    /// маппинг Definition → рантайм-объектив полностью вынесен в TutorialObjectiveFactory
    /// (см. Objectives/TutorialObjectiveFactory.cs). CreateRuntime() здесь намеренно
    /// отсутствует — Authoring не должен знать о Runtime-реализациях (P1 corrective pass).
    /// </summary>
    public abstract class TutorialObjectiveDefinition : ScriptableObject
    {
        [Tooltip("Человекочитаемое описание для дебаггера/аналитики. Не используется в геймплее.")]
        public string debugDescription;

        /// <summary>Стабильный идентификатор типа объектива для аналитики/дебага.</summary>
        public abstract string ObjectiveTypeId { get; }

#if UNITY_EDITOR
        /// <summary>Точка расширения для валидации конкретного объектива в редакторе.</summary>
        public virtual bool Validate(out string error)
        {
            error = null;
            return true;
        }
#endif
    }
}
