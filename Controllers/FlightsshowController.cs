using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using agancywebProject.Data;
using agancywebProject.Helpers;
using agancywebProject.Models.DB;

namespace agancywebProject.Controllers
{
    public class FlightsshowController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FlightsshowController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Flightsshow
        public async Task<IActionResult> Index(string? origin, string? destination, DateTime? date,
            string? tripType, DateTime? returnDate, int? passengers)
        {
            if (_context.Flight == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Flight'  is null.");
            }

            bool isRoundTrip = tripType == "roundtrip";

            var routes = await FlightRoutes.GetAsync(_context);
            ViewBag.FlightOrigins = routes.Origins;
            ViewBag.FlightDestinations = routes.Destinations;

            ViewBag.Origin = origin;
            ViewBag.Destination = destination;
            ViewBag.Date = date?.ToString("yyyy-MM-dd");
            ViewBag.TripType = isRoundTrip ? "roundtrip" : "oneway";
            ViewBag.ReturnDate = returnDate?.ToString("yyyy-MM-dd");
            ViewBag.Passengers = passengers;

            ViewBag.Error = TempData["FlightBookingError"];

            bool hasSearched = !string.IsNullOrWhiteSpace(origin) || !string.IsNullOrWhiteSpace(destination)
                || date.HasValue || (passengers.HasValue && passengers.Value > 1);
            ViewBag.NeedsDate = hasSearched && !date.HasValue;

            if (!date.HasValue)
            {
                ViewBag.IsFiltered = hasSearched;
                ViewBag.ReturnFlights = new List<Flight>();
                return View(Enumerable.Empty<Flight>());
            }

            if (date.Value.Date < DateTime.Today || (isRoundTrip && returnDate.HasValue && returnDate.Value.Date < DateTime.Today))
            {
                ViewBag.DateError = Lang.Msg(HttpContext, "تاریخ رفت نمی‌تواند در گذشته باشد.");
                ViewBag.IsFiltered = true;
                ViewBag.ReturnFlights = new List<Flight>();
                return View(Enumerable.Empty<Flight>());
            }

            var query = _context.Flight.AsQueryable();

            if (!string.IsNullOrWhiteSpace(origin))
            {
                query = query.Where(f => f.Origin == origin);
            }
            if (!string.IsNullOrWhiteSpace(destination))
            {
                query = query.Where(f => f.distination == destination);
            }
            query = query.Where(f => f.startdate.Date == date.Value.Date);

            var bookedSeats = await GetBookedSeatsMapAsync();
            bool HasEnoughSeats(Flight f)
            {
                if (!passengers.HasValue || passengers.Value <= 0) return true;
                var booked = bookedSeats.TryGetValue(f.Flight_Id, out var seats) ? seats : 0;
                return f.capacity - booked >= passengers.Value;
            }

            var outboundFlights = (await query.OrderBy(f => f.startdate).ToListAsync())
                .Where(HasEnoughSeats)
                .ToList();

            var returnFlights = new List<Flight>();
            if (isRoundTrip && returnDate.HasValue)
            {
                if (returnDate.Value.Date <= date.Value.Date)
                {
                    ViewBag.DateError = Lang.Msg(HttpContext, "تاریخ برگشت باید بعد از تاریخ رفت باشد.");
                }
                else
                {
                    var returnQuery = _context.Flight.AsQueryable();
                    if (!string.IsNullOrWhiteSpace(destination))
                    {
                        returnQuery = returnQuery.Where(f => f.Origin == destination);
                    }
                    if (!string.IsNullOrWhiteSpace(origin))
                    {
                        returnQuery = returnQuery.Where(f => f.distination == origin);
                    }
                    returnQuery = returnQuery.Where(f => f.startdate.Date == returnDate.Value.Date);
                    returnFlights = (await returnQuery.OrderBy(f => f.startdate).ToListAsync())
                        .Where(HasEnoughSeats)
                        .ToList();
                }
            }

            ViewBag.IsFiltered = true;
            ViewBag.ReturnFlights = returnFlights;

            ViewBag.BookedSeats = bookedSeats;

            return View(outboundFlights);
        }

        private async Task<Dictionary<int, int>> GetBookedSeatsMapAsync()
        {
            if (_context.FlightBooking == null) return new Dictionary<int, int>();

            var cutoff = BookingRules.LockCutoff();
            return await _context.FlightBooking
                .Where(b => b.Status == BookingStatus.Paid || (b.Status == BookingStatus.Locked && b.LockedAt > cutoff))
                .GroupBy(b => b.Flight_Id)
                .Select(g => new { FlightId = g.Key, Seats = g.Sum(b => b.SeatCount) })
                .ToDictionaryAsync(x => x.FlightId, x => x.Seats);
        }

        private bool FlightExists(int id)
        {
          return (_context.Flight?.Any(e => e.Flight_Id == id)).GetValueOrDefault();
        }
    }
}
