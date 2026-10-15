namespace agancywebProject.Models.DB
{
    public static class BookingRules
    {
        public static readonly TimeSpan LockTimeout = TimeSpan.FromMinutes(10);

        public static DateTime LockCutoff() => DateTime.Now - LockTimeout;

        // Seconds left before a lock taken at lockedAt expires (0 when already expired or unknown).
        public static int SecondsRemaining(DateTime? lockedAt)
        {
            if (lockedAt == null) return 0;
            var left = (lockedAt.Value + LockTimeout - DateTime.Now).TotalSeconds;
            return left <= 0 ? 0 : (int)Math.Ceiling(left);
        }
    }
}
