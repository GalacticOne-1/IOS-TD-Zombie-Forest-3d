namespace Galactic1.Code.Systems.Tutorial.Objectives
{
    public interface ITutorialSquadQuery
    {
        int GetStrategicSquadSize();

        /// <summary>
        /// true - выбранный юнит не состоит в отряде
        /// </summary>
        bool SurvivorIsFree();
    }
}
