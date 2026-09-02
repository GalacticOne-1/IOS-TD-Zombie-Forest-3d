namespace Galactic1.Code.Gameplay.Construction.Repair
{
    /// <summary>
    /// Результат попытки ремонта. Immutable, без бизнес-логики.
    /// </summary>
    public readonly struct RepairResult
    {
        public readonly bool Success;
        public readonly RepairFailReason FailReason;

        private RepairResult(bool success, RepairFailReason failReason)
        {
            Success = success;
            FailReason = failReason;
        }

        public static RepairResult Ok() => new(true, RepairFailReason.None);
        public static RepairResult Fail(RepairFailReason reason) => new(false, reason);
    }
}