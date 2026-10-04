namespace agancywebProject.Models.DB
{
    public static class BookingRules
    {
        public static readonly TimeSpan LockTimeout = TimeSpan.FromMinutes(20);

        public static DateTime LockCutoff() => DateTime.Now - LockTimeout;
    }
}
