using agancywebProject.Data;
using agancywebProject.Helpers;
using agancywebProject.Models;
using agancywebProject.Models.DB;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace agancywebProject.Controllers
{
    // Admin-only management of hotel and flight bookings (list / edit / delete)
    [Authorize]
    public class AdminBookingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminBookingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /AdminBookings?type=all|hotel|flight&status=1&q=name-or-phone
        [HttpGet]
        public async Task<IActionResult> Index(string? q, string? status, string? type)
        {
            var vm = new AdminBookingsViewModel
            {
                Q = q?.Trim(),
                Status = status,
                Type = type == "hotel" || type == "flight" ? type : "all"
            };

            BookingStatus? statusFilter = null;
            if (int.TryParse(status, out var statusInt) && Enum.IsDefined(typeof(BookingStatus), statusInt))
            {
                statusFilter = (BookingStatus)statusInt;
            }

            if (vm.Type != "flight")
            {
                var query = _context.Booking!.Include(b => b.Hotel).AsQueryable();
                if (!string.IsNullOrEmpty(vm.Q))
                    query = query.Where(b => b.FullName.Contains(vm.Q) || b.PhoneNumber.Contains(vm.Q));
                if (statusFilter.HasValue)
                    query = query.Where(b => b.Status == statusFilter.Value);
                vm.HotelBookings = await query.OrderByDescending(b => b.CreatedAt).Take(500).ToListAsync();
            }

            if (vm.Type != "hotel")
            {
                var query = _context.FlightBooking!.Include(b => b.Flight).AsQueryable();
                if (!string.IsNullOrEmpty(vm.Q))
                    query = query.Where(b => b.FullName.Contains(vm.Q) || b.PhoneNumber.Contains(vm.Q));
                if (statusFilter.HasValue)
                    query = query.Where(b => b.Status == statusFilter.Value);
                vm.FlightBookings = await query.OrderByDescending(b => b.CreatedAt).Take(500).ToListAsync();
            }

            return View(vm);
        }

        // ---------------------------------------------------------------- Hotel bookings

        [HttpGet]
        public async Task<IActionResult> EditHotel(int? id)
        {
            if (id == null) return NotFound();
            var booking = await _context.Booking!.Include(b => b.Hotel).FirstOrDefaultAsync(b => b.Booking_Id == id);
            if (booking == null || booking.Hotel == null) return NotFound();
            return View(booking);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditHotel(int id,
            [Bind("Booking_Id,CheckIn,CheckOut,FullName,PhoneNumber,Note,Status,TotalPrice")] Booking input)
        {
            if (id != input.Booking_Id) return NotFound();

            var booking = await _context.Booking!.Include(b => b.Hotel).FirstOrDefaultAsync(b => b.Booking_Id == id);
            if (booking == null || booking.Hotel == null) return NotFound();
            var hotel = booking.Hotel;

            var checkIn = input.CheckIn.Date;
            var checkOut = input.CheckOut.Date;
            var isActive = input.Status == BookingStatus.Paid || input.Status == BookingStatus.Locked;

            if (ModelState.IsValid && isActive)
            {
                if (checkIn < hotel.startdate.Date || checkOut > hotel.finishdate.Date)
                {
                    ModelState.AddModelError(string.Empty, Lang.Msg(HttpContext, "بازه انتخابی باید داخل تاریخ خالی بودن این هتل باشد."));
                }
                else
                {
                    var cutoff = BookingRules.LockCutoff();
                    var overlap = await _context.Booking!.AnyAsync(b =>
                        b.Hotel_Id == booking.Hotel_Id &&
                        b.Booking_Id != booking.Booking_Id &&
                        (b.Status == BookingStatus.Paid || (b.Status == BookingStatus.Locked && b.LockedAt > cutoff)) &&
                        b.CheckIn < checkOut && b.CheckOut > checkIn);
                    if (overlap)
                    {
                        ModelState.AddModelError(string.Empty, Lang.Msg(HttpContext, "این بازه با رزرو فعال دیگری برای همین هتل تداخل دارد."));
                    }
                }
            }

            var nights = (checkOut - checkIn).Days;
            var datesChanged = booking.CheckIn.Date != checkIn || booking.CheckOut.Date != checkOut;
            long total = input.TotalPrice;
            // If the dates changed and the admin did not touch the price, recalculate it automatically.
            if (datesChanged && input.TotalPrice == booking.TotalPrice && nights > 0)
            {
                total = (long)hotel.price * nights;
            }
            if (total < 0 || total > int.MaxValue)
            {
                ModelState.AddModelError(nameof(input.TotalPrice), Lang.Msg(HttpContext, "مبلغ وارد شده معتبر نیست."));
            }

            if (!ModelState.IsValid)
            {
                input.Hotel_Id = booking.Hotel_Id;
                input.Hotel = hotel;
                return View(input);
            }

            var statusChanged = booking.Status != input.Status;
            booking.CheckIn = checkIn;
            booking.CheckOut = checkOut;
            booking.Nights = nights;
            booking.FullName = input.FullName.Trim();
            booking.PhoneNumber = input.PhoneNumber.Trim();
            booking.Note = string.IsNullOrWhiteSpace(input.Note) ? null : input.Note.Trim();
            booking.TotalPrice = (int)total;
            booking.Status = input.Status;
            if (statusChanged)
            {
                if (input.Status == BookingStatus.Paid && booking.PaidAt == null) booking.PaidAt = DateTime.Now;
                if (input.Status != BookingStatus.Paid) booking.PaidAt = null;
                if (input.Status == BookingStatus.Locked) booking.LockedAt = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            TempData["AdminBookingMsg"] = Lang.Msg(HttpContext, "رزرو هتل با موفقیت ویرایش شد.");
            return RedirectToAction(nameof(Index), new { type = "hotel" });
        }

        [HttpGet]
        public async Task<IActionResult> DeleteHotel(int? id)
        {
            if (id == null) return NotFound();
            var booking = await _context.Booking!.Include(b => b.Hotel).FirstOrDefaultAsync(b => b.Booking_Id == id);
            if (booking == null) return NotFound();
            return View(booking);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteHotelConfirmed(int id)
        {
            var booking = await _context.Booking!.FirstOrDefaultAsync(b => b.Booking_Id == id);
            if (booking != null)
            {
                _context.Booking!.Remove(booking);
                await _context.SaveChangesAsync();
            }
            TempData["AdminBookingMsg"] = Lang.Msg(HttpContext, "رزرو هتل حذف شد.");
            return RedirectToAction(nameof(Index), new { type = "hotel" });
        }

        // ---------------------------------------------------------------- Flight bookings

        [HttpGet]
        public async Task<IActionResult> EditFlight(int? id)
        {
            if (id == null) return NotFound();
            var booking = await _context.FlightBooking!.Include(b => b.Flight).FirstOrDefaultAsync(b => b.FlightBooking_Id == id);
            if (booking == null || booking.Flight == null) return NotFound();
            return View(booking);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditFlight(int id,
            [Bind("FlightBooking_Id,SeatCount,FullName,PhoneNumber,Note,Status,TotalPrice")] FlightBooking input)
        {
            if (id != input.FlightBooking_Id) return NotFound();

            var booking = await _context.FlightBooking!.Include(b => b.Flight).FirstOrDefaultAsync(b => b.FlightBooking_Id == id);
            if (booking == null || booking.Flight == null) return NotFound();
            var flight = booking.Flight;

            var isActive = input.Status == BookingStatus.Paid || input.Status == BookingStatus.Locked;

            if (ModelState.IsValid && isActive)
            {
                var cutoff = BookingRules.LockCutoff();
                var otherBooked = await _context.FlightBooking!
                    .Where(b => b.Flight_Id == booking.Flight_Id &&
                                b.FlightBooking_Id != booking.FlightBooking_Id &&
                                (b.Status == BookingStatus.Paid || (b.Status == BookingStatus.Locked && b.LockedAt > cutoff)))
                    .SumAsync(b => (int?)b.SeatCount) ?? 0;

                if (otherBooked + input.SeatCount > flight.capacity)
                {
                    var remaining = Math.Max(0, flight.capacity - otherBooked);
                    ModelState.AddModelError(nameof(input.SeatCount),
                        Lang.Msg(HttpContext, "فقط {0} صندلی از این پرواز باقی مانده است.", remaining));
                }
            }

            var seatsChanged = booking.SeatCount != input.SeatCount;
            long total = input.TotalPrice;
            // If the seat count changed and the admin did not touch the price, recalculate it automatically.
            if (seatsChanged && input.TotalPrice == booking.TotalPrice && input.SeatCount > 0)
            {
                total = (long)flight.price * input.SeatCount;
            }
            if (total < 0 || total > int.MaxValue)
            {
                ModelState.AddModelError(nameof(input.TotalPrice), Lang.Msg(HttpContext, "مبلغ وارد شده معتبر نیست."));
            }

            if (!ModelState.IsValid)
            {
                input.Flight_Id = booking.Flight_Id;
                input.Flight = flight;
                return View(input);
            }

            var statusChanged = booking.Status != input.Status;
            booking.SeatCount = input.SeatCount;
            booking.FullName = input.FullName.Trim();
            booking.PhoneNumber = input.PhoneNumber.Trim();
            booking.Note = string.IsNullOrWhiteSpace(input.Note) ? null : input.Note.Trim();
            booking.TotalPrice = (int)total;
            booking.Status = input.Status;
            if (statusChanged)
            {
                if (input.Status == BookingStatus.Paid && booking.PaidAt == null) booking.PaidAt = DateTime.Now;
                if (input.Status != BookingStatus.Paid) booking.PaidAt = null;
                if (input.Status == BookingStatus.Locked) booking.LockedAt = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            TempData["AdminBookingMsg"] = Lang.Msg(HttpContext, "رزرو پرواز با موفقیت ویرایش شد.");
            return RedirectToAction(nameof(Index), new { type = "flight" });
        }

        [HttpGet]
        public async Task<IActionResult> DeleteFlight(int? id)
        {
            if (id == null) return NotFound();
            var booking = await _context.FlightBooking!.Include(b => b.Flight).FirstOrDefaultAsync(b => b.FlightBooking_Id == id);
            if (booking == null) return NotFound();
            return View(booking);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFlightConfirmed(int id)
        {
            var booking = await _context.FlightBooking!.FirstOrDefaultAsync(b => b.FlightBooking_Id == id);
            if (booking != null)
            {
                _context.FlightBooking!.Remove(booking);
                await _context.SaveChangesAsync();
            }
            TempData["AdminBookingMsg"] = Lang.Msg(HttpContext, "رزرو پرواز حذف شد.");
            return RedirectToAction(nameof(Index), new { type = "flight" });
        }
    }
}
