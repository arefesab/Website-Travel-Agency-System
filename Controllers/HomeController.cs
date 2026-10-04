using agancywebProject.Data;
using agancywebProject.Helpers;
using agancywebProject.Models;
using agancywebProject.Models.DB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace agancywebProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Home/SetLanguage?lang=en&returnUrl=/
        [HttpGet]
        public IActionResult SetLanguage(string lang, string? returnUrl)
        {
            var toEnglish = lang == "en";
            var options = new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                IsEssential = true,
                Path = "/"
            };

            // Our own cookie: only used to switch the layout to LTR.
            Response.Cookies.Append(agancywebProject.Helpers.Lang.CookieName, toEnglish ? "en" : "fa", options);

            if (!toEnglish)
            {
                // English: the googtrans cookie ("/fa/en") is set in the browser by _GoogleTranslate.cshtml,
                // because ASP.NET would URL-encode the slashes and Google would not recognise the value.
                // Persian: remove any googtrans cookie
                // Remove the Google translation cookie (host-only and domain variants)
                Response.Cookies.Delete("googtrans", new CookieOptions { Path = "/" });
                var host = Request.Host.Host;
                if (host.Contains('.') && !System.Net.IPAddress.TryParse(host, out _))
                {
                    Response.Cookies.Delete("googtrans", new CookieOptions { Path = "/", Domain = host });
                    Response.Cookies.Delete("googtrans", new CookieOptions { Path = "/", Domain = "." + host });
                }
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Index()
        {
            var today = DateTime.Today;

            var bookedSoon = await HotelPricing.GetBookedSoonAsync(_context);
            var openHotels = _context.Hotel == null
                ? new List<Hotel>()
                : await _context.Hotel.Where(h => h.finishdate.Date > today).ToListAsync();

            var deals = openHotels
                .Select(h => { var info = HotelPricing.GetInfo(h, bookedSoon); return new { Hotel = h, Percent = info.Percent, EndDate = info.OnlyUntil ?? h.finishdate }; })
                .Where(x => x.Percent > 0)
                .OrderByDescending(x => x.Percent).ThenBy(x => x.Hotel.startdate)
                // هر هتل (اتاق‌های مختلف) فقط یک بار؛ بهترین تخفیفِ همان هتل می‌ماند تا جا برای هتل‌های دیگر باز شود
                .GroupBy(x => ((x.Hotel.name ?? string.Empty).Trim(), (x.Hotel.city ?? string.Empty).Trim()))
                .Select(g => g.First())
                .OrderByDescending(x => x.Percent).ThenBy(x => x.Hotel.startdate)
                .Take(6)
                .ToList();

            ViewBag.LastMinuteHotels = deals.Select(x => x.Hotel).ToList();
            ViewBag.HotelDiscounts = deals.ToDictionary(x => x.Hotel.Hotel_Id, x => x.Percent);
            // تاریخ پایان واقعی تخفیف: دستی = پایان بازهٔ هتل، خودکار = فقط تا فردا
            ViewBag.HotelDealEnds = deals.ToDictionary(x => x.Hotel.Hotel_Id, x => x.EndDate);

            var lastMinuteFlights = _context.Flight == null
                ? new List<Flight>()
                : await _context.Flight
                    .Where(f => f.startdate.Date >= today && f.startdate.Date <= today.AddDays(5))
                    .OrderBy(f => f.startdate)
                    .Take(6)
                    .ToListAsync();

            ViewBag.LastMinuteFlights = lastMinuteFlights;
            var routes = await FlightRoutes.GetAsync(_context);
            ViewBag.FlightOrigins = routes.Origins;
            ViewBag.FlightDestinations = routes.Destinations;

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
        public IActionResult Kish()
        {
            return View();
        }
        public IActionResult French()
        {
            return View();
        }
        public IActionResult Turkey()
        {
            return View();
        }
        public IActionResult Russia()
        {
            return View();
        }
        public IActionResult Shiraz()
        {
            return View();
        }
        public IActionResult Qeshm()
        {
            return View();
        }
        public IActionResult Maldio()
        {
            return View();
        }
        public IActionResult Kordestan()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}