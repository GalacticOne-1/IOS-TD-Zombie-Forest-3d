using Galactic1.Game.Meta.Items;

namespace Galactic1.Code.Gameplay.Construction
{
    /// <summary>
    /// Контекст FSM режима строительства.
    /// Хранит текущее состояние взаимодействия игрока.
    /// </summary>
    public class ConstructionContext
    {
        /// <summary>
        /// Выбранный существующий объект
        /// </summary>
        public BuildableObject SelectedObject;

        /// <summary>
        /// Конфиг здания для строительства
        /// </summary>
        public FacilityModule BuildConfig;

        /// <summary>
        /// Текущий ghost объект
        /// </summary>
        public BuildableObject CurrentGhost;
        
        public PlacementPreviewRuntime Preview;

        public bool HasSelection => SelectedObject != null;
        public bool HasBuildConfig => BuildConfig != null;
        public bool HasGhost => CurrentGhost != null;

        public void ClearSelection()
        {
            SelectedObject = null;
        }

        public void ClearBuild()
        {
            BuildConfig = null;
            CurrentGhost = null;
            Preview?.Clear();
            Preview = null;
        }

        public void Reset()
        {
            SelectedObject = null;
            BuildConfig = null;
            CurrentGhost = null;
            Preview?.Clear();
            Preview = null;
        }
    }
}