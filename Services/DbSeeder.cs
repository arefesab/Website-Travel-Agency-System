using System.Security.Cryptography;
using System.Text.Encodings.Web;
using System.Text.Json;
using agancywebProject.Data;
using agancywebProject.Models.DB;
using Microsoft.EntityFrameworkCore;

namespace agancywebProject.Services
{
    /// <summary>Shape of SeedData/seed.json. Dates are stored as day offsets from the export day, so the data never goes stale.</summary>
    public class SeedFile
    {
        public List<SeedHotel> Hotels { get; set; } = new();
        public List<SeedFlight> Flights { get; set; } = new();
    }

    public class SeedHotel
    {
        public string? Name { get; set; }
        public string? City { get; set; }
        public int StartOffsetDays { get; set; }
        public int FinishOffsetDays { get; set; }
        public int Price { get; set; }
        public string? Address { get; set; }
        public int Star { get; set; }
        public string? Meal { get; set; }
        public string? Description { get; set; }
        public string? Amenities { get; set; }
        public string? PhotoUrls { get; set; }
        public string? RoomPhotoUrls { get; set; }
        public string? RoomName { get; set; }
        public int RoomCapacity { get; set; }
        public int? DiscountPercent { get; set; }
    }

    public class SeedFlight
    {
        public string? Origin { get; set; }
        public string? Destination { get; set; }
        public int DayOffset { get; set; }
        public string Time { get; set; } = "00:00";
        public int Price { get; set; }
        public int Capacity { get; set; }
        public string? FlightClass { get; set; }
    }

    public static class DbSeeder
    {
        public static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping   // keep Persian text readable in the file
        };

        /// <summary>
        /// Runs at startup. On a fresh database (no admin user yet) it creates the admin account and loads SeedData/seed.json.
        /// Set Seed:Force=true in config to also fill empty Hotel/Flight tables on a database that already has users.
        /// </summary>
        public static async Task SeedAsync(IServiceProvider sp, IWebHostEnvironment env, IConfiguration cfg)
        {
            var log = sp.GetRequiredService<ILoggerFactory>().CreateLogger("DbSeeder");
            try
            {
                var db = sp.GetRequiredService<ApplicationDbContext>();

                if ((await db.Database.GetPendingMigrationsAsync()).Any())
                {
                    await db.Database.MigrateAsync();
                }

                var fresh = !await db.UserLogin!.AnyAsync();
                var force = cfg.GetValue<bool>("Seed:Force");

                if (fresh) await SeedAdminAsync(db, cfg, log);
                if (fresh || force) await SeedCatalogAsync(db, env, log);
            }
            catch (Exception ex)
            {
                // never take the site down because of seeding
                log.LogError(ex, "Database seeding failed.");
            }
        }

        private static async Task SeedAdminAsync(ApplicationDbContext db, IConfiguration cfg, ILogger log)
        {
            var username = cfg["Seed:AdminUsername"];
            if (string.IsNullOrWhiteSpace(username)) username = "admin";

            var password = cfg["Seed:AdminPassword"];
            var generated = string.IsNullOrWhiteSpace(password);
            if (generated)
            {
                password = Convert.ToHexString(RandomNumberGenerator.GetBytes(8)); // 16 hex chars
            }

            db.UserLogin!.Add(new UserLogin { username = username!, password = PasswordHasher.Hash(password!) });
            await db.SaveChangesAsync();

            if (generated)
                log.LogWarning("Seeded admin user '{User}' with a generated password: {Password}  (change it, or set Seed:AdminPassword)", username, password);
            else
                log.LogInformation("Seeded admin user '{User}'.", username);
        }

        private static async Task SeedCatalogAsync(ApplicationDbContext db, IWebHostEnvironment env, ILogger log)
        {
            var path = Path.Combine(env.ContentRootPath, "SeedData", "seed.json");
            if (!File.Exists(path))
            {
                log.LogInformation("No SeedData/seed.json found; skipping catalog seed.");
                return;
            }

            var data = JsonSerializer.Deserialize<SeedFile>(await File.ReadAllTextAsync(path), JsonOptions) ?? new SeedFile();
            var today = DateTime.Today;

            if (!await db.Hotel!.AnyAsync() && data.Hotels.Count > 0)
            {
                foreach (var h in data.Hotels)
                {
                    var start = today.AddDays(h.StartOffsetDays);
                    var finish = today.AddDays(Math.Max(h.FinishOffsetDays, h.StartOffsetDays + 1));
                    db.Hotel!.Add(new Hotel
                    {
                        name = h.Name, city = h.City,
                        startdate = start, finishdate = finish,
                        price = h.Price, address = h.Address, star = h.Star, meal = h.Meal,
                        description = h.Description, amenities = h.Amenities,
                        photoUrls = h.PhotoUrls, roomPhotoUrls = h.RoomPhotoUrls,
                        roomName = h.RoomName, roomCapacity = h.RoomCapacity,
                        DiscountPercent = h.DiscountPercent
                    });
                }
                log.LogInformation("Seeded {Count} hotels.", data.Hotels.Count);
            }

            if (!await db.Flight!.AnyAsync() && data.Flights.Count > 0)
            {
                foreach (var f in data.Flights)
                {
                    var time = TimeSpan.TryParse(f.Time, out var t) ? t : TimeSpan.Zero;
                    db.Flight!.Add(new Flight
                    {
                        Origin = f.Origin, distination = f.Destination,
                        startdate = today.AddDays(f.DayOffset),
                        entrytime = today.Add(time),
                        price = f.Price, capacity = f.Capacity, flightClass = f.FlightClass
                    });
                }
                log.LogInformation("Seeded {Count} flights.", data.Flights.Count);
            }

            await db.SaveChangesAsync();
        }
    }
}
