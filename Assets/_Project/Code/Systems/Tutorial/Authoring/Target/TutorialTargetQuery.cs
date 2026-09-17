using System;

namespace Galactic1.Code.Systems.Tutorial.Authoring
{
    /// <summary>
    /// Полиморфная authoring-инструкция "как искать highlight-таргет" — заменяет
    /// HighlightMode + набор взаимоисключающих полей. Чистые config-данные:
    /// описывает НАМЕРЕНИЕ поиска, а не сам резолв. Runtime не работает с этим типом
    /// напрямую — см. TutorialTargetRequestFactory (Query → Request), которая держит
    /// Authoring/Runtime границу проекта в чистоте.
    /// </summary>
    [Serializable]
    public abstract class TutorialTargetQuery
    {
#if UNITY_EDITOR
        public virtual bool Validate(out string error)
        {
            error = null;
            return true;
        }
#endif
    }
}