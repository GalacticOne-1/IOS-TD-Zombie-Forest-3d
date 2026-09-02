namespace Galactic1.Code.UI.RaidReport
{
    public class RaidBonusEligibility
    {
        public bool IsEligible;
        public bool AdBonusAvail;
        public IneligibleReason Reason;

        public static RaidBonusEligibility Eligible(bool adBonusAvail)
            => new() { IsEligible = true , AdBonusAvail = adBonusAvail };

        public static RaidBonusEligibility Ineligible(IneligibleReason r)
            => new() { IsEligible = false, Reason = r };
    }
}