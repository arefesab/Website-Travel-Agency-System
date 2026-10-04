using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using agancywebProject.Data;
using agancywebProject.Helpers;
using agancywebProject.Models.DB;

namespace agancywebProject.Controllers
{
    public class FlightsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FlightsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Flights
        [Authorize]
        public async Task<IActionResult> Index()
        {
            if (_context.Flight == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Flight'  is null.");
            }

            var flights = await _context.Flight.ToListAsync();

            var cutoff = BookingRules.LockCutoff();
            ViewBag.BookedSeats = _context.FlightBooking == null
                ? new Dictionary<int, int>()
                : await _context.FlightBooking
                    .Where(b => b.Status == BookingStatus.Paid || (b.Status == BookingStatus.Locked && b.LockedAt > cutoff))
                    .GroupBy(b => b.Flight_Id)
                    .Select(g => new { FlightId = g.Key, Seats = g.Sum(b => b.SeatCount) })
                    .ToDictionaryAsync(x => x.FlightId, x => x.Seats);

            return View(flights);
        }

        // GET: Flights/Details/5
        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Flight == null)
            {
                return NotFound();
            }

            var flight = await _context.Flight
                .FirstOrDefaultAsync(m => m.Flight_Id == id);
            if (flight == null)
            {
                return NotFound();
            }

            ViewBag.Bookings = _context.FlightBooking == null
                ? new List<FlightBooking>()
                : await _context.FlightBooking
                    .Where(b => b.Flight_Id == flight.Flight_Id)
                    .OrderByDescending(b => b.CreatedAt)
                    .ToListAsync();

            return View(flight);
        }

        // GET: Flights/Create
        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Flights/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create([Bind("Flight_Id,Origin,distination,startdate,price,capacity,entrytime,flightClass")] Flight flight)
        {
            if (flight.startdate.Date < DateTime.Today)
                ModelState.AddModelError(nameof(Flight.startdate), Lang.Msg(HttpContext, "تاریخ نمی‌تواند در گذشته باشد."));

            if (ModelState.IsValid)
            {
                _context.Add(flight);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(flight);
        }

        // GET: Flights/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Flight == null)
            {
                return NotFound();
            }

            var flight = await _context.Flight.FindAsync(id);
            if (flight == null)
            {
                return NotFound();
            }
            return View(flight);
        }
        // POST: Flights/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(int id, [Bind("Flight_Id,Origin,distination,startdate,price,capacity,entrytime,flightClass")] Flight flight)
        {
            if (id != flight.Flight_Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(flight);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FlightExists(flight.Flight_Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(flight);
        }

        // GET: Flights/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Flight == null)
            {
                return NotFound();
            }

            var flight = await _context.Flight
                .FirstOrDefaultAsync(m => m.Flight_Id == id);
            if (flight == null)
            {
                return NotFound();
            }

            return View(flight);
        }

        // POST: Flights/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Flight == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Flight'  is null.");
            }
            if (_context.FlightBooking != null)
            {
                var cutoff = BookingRules.LockCutoff();
                var hasActiveBookings = await _context.FlightBooking.AnyAsync(b =>
                    b.Flight_Id == id &&
                    (b.Status == BookingStatus.Paid || (b.Status == BookingStatus.Locked && b.LockedAt > cutoff)));

                if (hasActiveBookings)
                {
                    TempData["AdminError"] = Lang.Msg(HttpContext, "این پرواز رزرو پرداخت‌شده یا در حال پرداخت دارد و قابل حذف نیست.");
                    return RedirectToAction(nameof(Index));
                }
            }

            var flight = await _context.Flight.FindAsync(id);
            if (flight != null)
            {
                _context.Flight.Remove(flight);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FlightExists(int id)
        {
          return (_context.Flight?.Any(e => e.Flight_Id == id)).GetValueOrDefault();
        }
    }
}
