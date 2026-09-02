using UnityEngine;

namespace Galactic1.Code.UI.RaidReport
{
    /// <summary>
    /// Результат бойца после рейда.
    /// DTO для передачи данных в UI.
    /// </summary>
    public struct RaidSurvivorResult
    {
        public RenderTexture RenderPortrait;
        public string Name;
        public string Status; // Alive / Injured / Dead / Exhausted
    }
}