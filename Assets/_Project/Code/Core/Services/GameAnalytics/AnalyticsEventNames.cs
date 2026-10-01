namespace Galactic1.Code.Systems.Analytics
{
    /// <summary>
    /// Единственное место со строками имён событий. Строки в gameplay-коде запрещены.
    /// [R] = Firebase recommended event, [C] = custom.
    /// </summary>
    public static class AnalyticsEventNames
    {
        // --- Tutorial
        public const string TutorialBegin = "tutorial_begin";                       // [R]
        public const string TutorialStepStart = "tutorial_step_start";              // [C]
        public const string TutorialStepComplete = "tutorial_step_complete";        // [C]
        public const string TutorialResume = "tutorial_resume";                     // [C]
        public const string TutorialResumeFallback = "tutorial_resume_fallback";    // [C]
        public const string TutorialComplete = "tutorial_complete";                 // [R]

        // --- Economy
        public const string EarnVirtualCurrency = "earn_virtual_currency";          // [R]
        public const string SpendVirtualCurrency = "spend_virtual_currency";        // [R]

        // --- Shop / IAP
        public const string BeginCheckout = "begin_checkout";                       // [R]
        public const string IapCompleted = "iap_completed";                         // [C] (revenue = авто in_app_purchase)
        public const string PurchaseFailed = "purchase_failed";                     // [C]

        // --- Ads
        public const string AdRewardGranted = "ad_reward_granted";                  // [C]

        // --- Progression / Raid
        public const string LevelUp = "level_up";                                   // [R]
        public const string RaidStart = "raid_start";                               // [C]
        public const string RaidEnd = "raid_end";                                   // [C]
    }
}