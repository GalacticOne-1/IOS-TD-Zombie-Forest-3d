namespace Galactic1.Code.Systems.Tutorial.Authoring
{
    /// <summary>
    /// Declarative execution domain for a tutorial step. Maps onto
    /// GameLoopContext.IsCampState/IsWorldMapState/IsRaidState — the project's own
    /// existing three-bucket domain split — instead of matching raw GameLoopState
    /// enum values one by one.
    ///
    /// KNOWN LIMITATION: GameLoopState.PostRaidReport/RaidResolving/PreparingSquad/CampReport
    /// all fall under IsCampState by construction (see GameLoopContext.IsCampState
    /// = !IsWorldMapState && !IsRaidState). "show_raid_result" therefore resumes under
    /// domain=Camp, not a distinct "report" domain. Flagged deliberately, not silently
    /// special-cased. If a distinct domain is required later, add
    /// GameLoopContext.IsPostRaidReportState and a corresponding enum value here.
    /// </summary>
    public enum TutorialStepDomain
    {
        /// <summary>No domain restriction — safe to resume anywhere.</summary>
        Any,
        Camp,
        WorldMap,
        Raid
    }
}
