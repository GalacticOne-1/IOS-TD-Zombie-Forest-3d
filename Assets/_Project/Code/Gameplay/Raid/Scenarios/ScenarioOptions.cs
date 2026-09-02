namespace Galactic1.Code.Systems.Raid.Scenarios
{
    /// <summary>
    /// Флаги, которыми сценарий сообщает RaidInProgressState,
    /// какие ОБЩИЕ системы пайплайна нужно включить.
    /// Сценарий сам эти системы не создаёт и не хранит.
    /// </summary>
    public record ScenarioOptions
    {
        public bool UseDefenseFacilities = false;
        public bool UseWaveSpawner = false;
        public bool UseAmbientPopulation = true;
        public bool UseLoot = true;
        public bool UseExitZones = true;
        public bool UseTransport = true;
    }
}