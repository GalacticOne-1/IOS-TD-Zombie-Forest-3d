namespace Galactic1.Code.Systems.Analytics
{
    /// <summary>
    /// Единственное место со строками имён параметров.
    /// Один смысл = одно имя (никаких step_id / stepId / tutorial_step).
    /// </summary>
    public static class AnalyticsParams
    {
        // --- Tutorial
        public const string CampaignId = "campaign_id";
        public const string ChapterId = "chapter_id";
        public const string StepId = "step_id";                 // immutable GUID
        public const string StepKey = "step_key";               // DebugKey, только для чтения человеком
        public const string StepIndex = "step_index";
        public const string TutorialRunId = "tutorial_run_id";
        public const string TutorialVersion = "tutorial_version";
        public const string Resumed = "resumed";                // 0/1
        public const string Instant = "instant";                // 0/1
        public const string ResumeKind = "resume_kind";         // current | checkpoint
        public const string DurationSec = "duration_sec";
        public const string TotalDurationSec = "total_duration_sec";
        public const string Reason = "reason";
        public const string CurrentDomain = "current_domain";

        // --- Economy (имена recommended-параметров Firebase)
        public const string VirtualCurrencyName = "virtual_currency_name";
        public const string Value = "value";
        public const string ItemName = "item_name";             // для spend_virtual_currency = sink
        public const string Source = "source";                  // для earn_virtual_currency

        // --- Progression
        public const string Level = "level";
        public const string PreviousLevel = "previous_level";

        // --- Shop / Ads
        public const string ProductId = "product_id";
        public const string TransactionId = "transaction_id";
        public const string Placement = "placement";

        // --- Raid
        public const string RaidRunId = "raid_run_id";
        public const string LocationId = "location_id";
        public const string RaidType = "raid_type";
        public const string Result = "result";
        public const string EndReason = "end_reason";
        public const string SquadSize = "squad_size";
        public const string PlayerLevel = "player_level";
        public const string SurvivorsLost = "survivors_lost";
        public const string KilledEnemies = "killed_enemies";
        public const string LootItems = "loot_items";
        public const string XpGained = "xp_gained";
        public const string MainBuildingDestroyed = "main_building_destroyed";

        // --- Добавляются сервисом ко всем событиям
        public const string SchemaVersion = "analytics_schema_version";
        public const string Environment = "analytics_env";
    }
}