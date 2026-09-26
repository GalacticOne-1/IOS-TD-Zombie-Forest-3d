using UnityEngine;

namespace Galactic1.Code.Systems.Tutorial.Authoring
{
    /// <summary>
    /// Базовый абстрактный конфиг разового действия, выполняемого в момент активации шага
    /// (открыть дверь, заспавнить/удалить объект и т.п.). Никакой связи с TutorialTargetId
    /// или guidance-системой — чистое "на старте шага сделать X". Конкретное действие —
    /// отдельный ScriptableObject-наследник с реализацией Evaluate().
    /// </summary>
    public abstract class TutorialActionDefinition : ScriptableObject
    {
        public abstract void Evaluate();
    }
}