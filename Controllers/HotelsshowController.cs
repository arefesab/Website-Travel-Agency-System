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
    public class HotelsshowController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HotelsshowController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Hotelsshow
        public async Task<IActionResult> Index(string? city, DateTime? checkin, DateTime? checkout)
        {
            if (_context.Hotel == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Hotel'  is null.");
            }

            ViewBag.City = city;
            ViewBag.Checkin = checkin?.ToString("yyyy-MM-dd");
            ViewBag.Checkout = checkout?.ToString("yyyy-MM-dd");

            bool hasSearched = !string.IsNullOrWhiteSpace(city) || checkin.HasValue || checkout.HasValue;
            ViewBag.NeedsDate = hasSearched && !checkin.HasValue;

            if (!checkin.HasValue)
            {
                ViewBag.IsFiltered = hasSearched;
                return View(Enumerable.Empty<Hotel>());
            }

            if (checkin.Value.Date < DateTime.Today || (checkout.HasValue && checkout.Value.Date < DateTime.Today))
            {
                ViewBag.DateError = Lang.Msg(HttpContext, "تاریخ ورود نمی‌تواند در گذشته باشد.");
                ViewBag.IsFiltered = true;
                return View(Enumerable.Empty<Hotel>());
            }

            var query = _context.Hotel.AsQueryable();

            if (!string.IsNullOrWhiteSpace(city))
            {
                query = query.Where(h => h.city == city);
            }
            query = query.Where(h => h.startdate.Date <= checkin.Value.Date);
            if (checkout.HasValue && checkout.Value.Date > checkin.Value.Date)
            {
                query = query.Where(h => h.finishdate.Date >= checkout.Value.Date);
            }
            else
            {
                if (checkout.HasValue)
                {
                    ViewBag.DateError = Lang.Msg(HttpContext, "تاریخ خروج باید بعد از تاریخ ورود باشد.");
                }

                query = query.Where(h => h.finishdate.Date > checkin.Value.Date);
            }

            if (string.IsNullOrWhiteSpace(ViewBag.DateError as string))
            {
                var reqCheckIn = checkin.Value.Date;
                var reqCheckOut = checkout.HasValue ? checkout.Value.Date : reqCheckIn.AddDays(1);
                var lockCutoff = BookingRules.LockCutoff();

                query = query.Where(h => !_context.Booking!.Any(b =>
                    b.Hotel_Id == h.Hotel_Id &&
                    (b.Status == BookingStatus.Paid || (b.Status == BookingStatus.Locked && b.LockedAt > lockCutoff)) &&
                    b.CheckIn < reqCheckOut && b.CheckOut > reqCheckIn));
            }

            ViewBag.IsFiltered = true;

            var list = await query.OrderBy(h => h.startdate).ToListAsync();
            var bookedSoonIds = await HotelPricing.GetBookedSoonAsync(_context);
            ViewBag.HotelDiscounts = list.ToDictionary(h => h.Hotel_Id, h => HotelPricing.GetPercent(h, checkin.Value.Date, bookedSoonIds));
            return View(list);
        }

        // GET: Hotelsshow/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Hotel == null)
            {
                return NotFound();
            }

            var hotel = await _context.Hotel.FirstOrDefaultAsync(m => m.Hotel_Id == id);
            if (hotel == null)
            {
                return NotFound();
            }

            ViewBag.DiscountPercent = HotelPricing.GetInfo(hotel, await HotelPricing.GetBookedSoonAsync(_context)).Percent;
            return View(hotel);
        }

        private bool HotelExists(int id)
        {
          return (_context.Hotel?.Any(e => e.Hotel_Id == id)).GetValueOrDefault();
        }
    }
}
