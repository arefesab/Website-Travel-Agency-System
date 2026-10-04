using agancywebProject.Data;
using Microsoft.EntityFrameworkCore;

namespace agancywebProject.Helpers
{
    public static class FlightRoutes
    {
        // مبدا و مقصدهایی که واقعاً پرواز ثبت‌شدهٔ آینده (از امروز به بعد) دارند
        public static async Task<(List<string> Origins, List<string> Destinations)> GetAsync(ApplicationDbContext db)
        {
            if (db.Flight == null) return (new List<string>(), new List<string>());

            var today = DateTime.Today;
            var pairs = await db.Flight
                .Where(f => f.startdate >= today)
                .Select(f => new { f.Origin, f.distination })
                .ToListAsync();

            List<string> Clean(IEnumerable<string?> src) => src
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => s!.Trim())
                .Distinct()
                .OrderBy(s => s, StringComparer.Create(new System.Globalization.CultureInfo("fa-IR"), false))
                .ToList();

            return (Clean(pairs.Select(p => p.Origin)), Clean(pairs.Select(p => p.distination)));
        }
    }
}
