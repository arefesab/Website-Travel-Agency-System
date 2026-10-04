using agancywebProject.Data;
using agancywebProject.Models.DB;
using Microsoft.EntityFrameworkCore;

namespace agancywebProject.Helpers
{
    public static class HotelPricing
    {
        public const int AutoDiscountPercent = 15; // درصد تخفیف خودکار
        public const int AutoWindowDays = 1;       // 1 = تا فردا

        public readonly record struct DealInfo(int Percent, DateTime? OnlyUntil);

        public static int Apply(int price, int percent) =>
            percent <= 0 ? price : (int)Math.Round((long)price * (100 - percent) / 100.0, MidpointRounding.AwayFromZero);

        // هتل‌هایی که در بازهٔ [from, to) رزرو پرداخت‌شده یا قفل‌شده دارند
        public static async Task<HashSet<int>> GetBookedHotelIdsAsync(ApplicationDbContext db, DateTime from, DateTime to)
        {
            var cutoff = BookingRules.LockCutoff();
            var ids = await db.Booking!
                .Where(b => (b.Status == BookingStatus.Paid || (b.Status == BookingStatus.Locked && b.LockedAt > cutoff))
                            && b.CheckIn < to && b.CheckOut > from)
                .Select(b => b.Hotel_Id)
                .Distinct()
                .ToListAsync();
            return new HashSet<int>(ids);
        }

        public static Task<HashSet<int>> GetBookedSoonAsync(ApplicationDbContext db) =>
            GetBookedHotelIdsAsync(db, DateTime.Today, DateTime.Today.AddDays(AutoWindowDays + 1));

        // درصد تخفیف خود هتل (برای نمایش)؛ OnlyUntil یعنی تخفیف خودکار فقط تا آن تاریخ ورود
        public static DealInfo GetInfo(Hotel h, HashSet<int> bookedSoon)
        {
            if (h.DiscountPercent.HasValue) return new DealInfo(h.DiscountPercent.Value, null); // دستی
            var edge = DateTime.Today.AddDays(AutoWindowDays);
            bool vacant = h.startdate.Date <= edge && h.finishdate.Date > edge && !bookedSoon.Contains(h.Hotel_Id);
            return vacant ? new DealInfo(AutoDiscountPercent, edge) : new DealInfo(0, null);
        }

        // درصد تخفیف برای اقامتی که در تاریخ checkIn شروع می‌شود
        public static int GetPercent(Hotel h, DateTime checkIn, HashSet<int> bookedSoon)
        {
            var d = GetInfo(h, bookedSoon);
            return d.Percent > 0 && (d.OnlyUntil == null || checkIn.Date <= d.OnlyUntil.Value.Date) ? d.Percent : 0;
        }
    }
}