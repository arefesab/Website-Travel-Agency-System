using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using agancywebProject.Data;
using agancywebProject.Helpers;
using agancywebProject.Models.DB;
using agancywebProject.Services.Payment;

namespace agancywebProject.Controllers
{
    public class BookingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPaymentGateway _paymentGateway;

        public BookingController(ApplicationDbContext context, IPaymentGateway paymentGateway)
        {
            _context = context;
            _paymentGateway = paymentGateway;
        }

        // GET: Booking/Reserve/5
        [HttpGet]
        public async Task<IActionResult> Reserve(int? id)
        {
            if (id == null) return NotFound();

            var hotel = await _context.Hotel!.FirstOrDefaultAsync(h => h.Hotel_Id == id);
            if (hotel == null) return NotFound();

            ViewBag.Hotel = hotel;
            await SetDiscountViewBagAsync(hotel);
            ViewBag.BookedRanges = await GetBookedRangesAsync(hotel.Hotel_Id);

            var suggestedCheckIn = hotel.startdate.Date > DateTime.Today ? hotel.startdate.Date : DateTime.Today;
            var suggestedCheckOut = suggestedCheckIn.AddDays(1) <= hotel.finishdate.Date
                ? suggestedCheckIn.AddDays(1)
                : hotel.finishdate.Date;

            var booking = new Booking
            {
                Hotel_Id = hotel.Hotel_Id,
                CheckIn = suggestedCheckIn,
                CheckOut = suggestedCheckOut
            };
            return View(booking);
        }

        // POST: Booking/Reserve
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reserve([Bind("Hotel_Id,CheckIn,CheckOut,FullName,PhoneNumber,Note")] Booking booking)
        {
            var hotel = await _context.Hotel!.FirstOrDefaultAsync(h => h.Hotel_Id == booking.Hotel_Id);
            if (hotel == null) return NotFound();

            ViewBag.Hotel = hotel;
            await SetDiscountViewBagAsync(hotel);
            ViewBag.BookedRanges = await GetBookedRangesAsync(hotel.Hotel_Id);

            if (booking.CheckIn.Date < DateTime.Today)
            {
                ModelState.AddModelError(nameof(booking.CheckIn), Lang.Msg(HttpContext, "تاریخ ورود نمی‌تواند در گذشته باشد."));
            }
            else if (booking.CheckIn.Date < hotel.startdate.Date || booking.CheckOut.Date > hotel.finishdate.Date)
            {
                ModelState.AddModelError(string.Empty, Lang.Msg(HttpContext, "بازه انتخابی باید داخل تاریخ خالی بودن این هتل باشد."));
            }
            else if (booking.CheckOut.Date > booking.CheckIn.Date && await HasOverlapAsync(hotel.Hotel_Id, booking.CheckIn.Date, booking.CheckOut.Date))
            {
                ModelState.AddModelError(string.Empty, Lang.Msg(HttpContext, "این بازه قبلا توسط شخص دیگری رزرو شده. لطفا بازه دیگری از تاریخ‌های خالی را انتخاب کنید."));
            }

            if (!ModelState.IsValid)
            {
                return View(booking);
            }

            var nights = (booking.CheckOut.Date - booking.CheckIn.Date).Days;

            var bookedSoon = await HotelPricing.GetBookedSoonAsync(_context);
            var percent = HotelPricing.GetPercent(hotel, booking.CheckIn.Date, bookedSoon);
            var total = (long)HotelPricing.Apply(hotel.price, percent) * nights;
            if (total > int.MaxValue)
            {
                ModelState.AddModelError(string.Empty, Lang.Msg(HttpContext, "مبلغ کل این رزرو بیش از حد مجاز است. لطفا تعداد شب‌های کمتری انتخاب کنید."));
                return View(booking);
            }

            booking.CheckIn = booking.CheckIn.Date;
            booking.CheckOut = booking.CheckOut.Date;
            booking.Nights = nights;
            booking.TotalPrice = (int)total;
            booking.Status = BookingStatus.Pending;
            booking.CreatedAt = DateTime.Now;

            _context.Booking!.Add(booking);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Preview), new { id = booking.Booking_Id });
        }

        // GET: Booking/Preview/5
        [HttpGet]
        public async Task<IActionResult> Preview(int? id)
        {
            if (id == null) return NotFound();

            var booking = await _context.Booking!
                .Include(b => b.Hotel)
                .FirstOrDefaultAsync(b => b.Booking_Id == id);

            if (booking == null || booking.Hotel == null) return NotFound();

            if (booking.Status != BookingStatus.Pending)
            {
                return RedirectToAction(nameof(Result), new { id = booking.Booking_Id });
            }

            if (await HasOverlapAsync(booking.Hotel_Id, booking.CheckIn, booking.CheckOut, booking.Booking_Id))
            {
                booking.Status = BookingStatus.Cancelled;
                await _context.SaveChangesAsync();
                TempData["BookingError"] = Lang.Msg(HttpContext, "متاسفانه این بازه هم‌اکنون توسط شخص دیگری رزرو شد. لطفا بازه دیگری انتخاب کنید.");
                return RedirectToAction(nameof(Reserve), new { id = booking.Hotel_Id });
            }

            ViewBag.Error = TempData["BookingError"];
            return View(booking);
        }

        // POST: Booking/Pay/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Pay(int id)
        {
            var booking = await _context.Booking!
                .Include(b => b.Hotel)
                .FirstOrDefaultAsync(b => b.Booking_Id == id);

            if (booking == null || booking.Hotel == null) return NotFound();

            if (booking.Status != BookingStatus.Pending)
            {
                return RedirectToAction(nameof(Result), new { id = booking.Booking_Id });
            }

            var now = DateTime.Now;
            var cutoff = now - BookingRules.LockTimeout;

            var lockedRows = await _context.Database.ExecuteSqlInterpolatedAsync($@"
UPDATE Booking SET Status = {(int)BookingStatus.Locked}, LockedAt = {now}
WHERE Booking_Id = {booking.Booking_Id} AND Status = {(int)BookingStatus.Pending}
AND NOT EXISTS (
    SELECT 1 FROM Booking b2
    WHERE b2.Hotel_Id = {booking.Hotel_Id} AND b2.Booking_Id <> {booking.Booking_Id}
    AND (b2.Status = {(int)BookingStatus.Paid}
         OR (b2.Status = {(int)BookingStatus.Locked} AND b2.LockedAt > {cutoff}))
    AND b2.CheckIn < {booking.CheckOut} AND b2.CheckOut > {booking.CheckIn}
)");

            if (lockedRows == 0)
            {
                booking.Status = BookingStatus.Cancelled;
                await _context.SaveChangesAsync();
                TempData["BookingError"] = Lang.Msg(HttpContext, "متاسفانه این بازه هم‌اکنون توسط شخص دیگری رزرو شد. لطفا بازه دیگری انتخاب کنید.");
                return RedirectToAction(nameof(Reserve), new { id = booking.Hotel_Id });
            }

            booking.Status = BookingStatus.Locked;
            booking.LockedAt = now;

            var callbackUrl = Url.Action(nameof(Callback), "Booking", new { id = booking.Booking_Id }, Request.Scheme)!;

            var result = await _paymentGateway.RequestPaymentAsync(new PaymentRequestModel
            {
                AmountToman = booking.TotalPrice,
                Description = $"رزرو هتل {booking.Hotel.name} - {booking.FullName}",
                CallbackUrl = callbackUrl,
                Mobile = booking.PhoneNumber
            });

            if (!result.Success)
            {
                booking.Status = BookingStatus.Failed;
                await _context.SaveChangesAsync();
                TempData["BookingError"] = Lang.Msg(HttpContext, result.ErrorMessage ?? "اتصال به درگاه پرداخت برقرار نشد.");
                return RedirectToAction(nameof(Preview), new { id = booking.Booking_Id });
            }

            booking.Authority = result.Authority;
            await _context.SaveChangesAsync();

            return Redirect(result.PaymentUrl!);
        }

        // GET: Booking/Callback/5
        [HttpGet]
        public async Task<IActionResult> Callback(int id, string? Authority, string? Status)
        {
            var booking = await _context.Booking!
                .Include(b => b.Hotel)
                .FirstOrDefaultAsync(b => b.Booking_Id == id);

            if (booking == null || booking.Hotel == null) return NotFound();

            if (string.IsNullOrEmpty(booking.Authority) ||
                !string.Equals(Authority, booking.Authority, StringComparison.Ordinal))
            {
                return BadRequest();
            }

            if (booking.Status != BookingStatus.Locked)
            {
                return RedirectToAction(nameof(Result), new { id = booking.Booking_Id });
            }

            if (!string.Equals(Status, "OK", StringComparison.OrdinalIgnoreCase))
            {
                booking.Status = BookingStatus.Failed;
                await _context.SaveChangesAsync();
                TempData["BookingError"] = Lang.Msg(HttpContext, "پرداخت توسط شما لغو شد.");
                return RedirectToAction(nameof(Result), new { id = booking.Booking_Id });
            }

            var verify = await _paymentGateway.VerifyPaymentAsync(booking.Authority, booking.TotalPrice);
            if (verify.Success)
            {
                await MarkPaidAsync(booking, verify.RefId);
            }
            else
            {
                booking.Status = BookingStatus.Failed;
                TempData["BookingError"] = Lang.Msg(HttpContext, verify.ErrorMessage ?? "پرداخت تایید نشد.");
            }
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Result), new { id = booking.Booking_Id });
        }

        // GET: Booking/FakeGateway/FAKE-xxxx
        [HttpGet]
        public async Task<IActionResult> FakeGateway(string id)
        {
            if (!_paymentGateway.IsSimulated) return NotFound();

            var authority = id;
            var booking = await _context.Booking!
                .Include(b => b.Hotel)
                .FirstOrDefaultAsync(b => b.Authority == authority);

            if (booking == null || booking.Hotel == null) return NotFound();
            if (booking.Status != BookingStatus.Locked)
            {
                return RedirectToAction(nameof(Result), new { id = booking.Booking_Id });
            }

            return View(booking);
        }

        // POST: Booking/FakeGatewayConfirm
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FakeGatewayConfirm(string authority, bool approved)
        {
            if (!_paymentGateway.IsSimulated) return NotFound();

            var booking = await _context.Booking!
                .Include(b => b.Hotel)
                .FirstOrDefaultAsync(b => b.Authority == authority);

            if (booking == null || booking.Hotel == null) return NotFound();

            if (booking.Status != BookingStatus.Locked)
            {
                return RedirectToAction(nameof(Result), new { id = booking.Booking_Id });
            }

            if (approved)
            {
                await MarkPaidAsync(booking, "TEST" + new Random().Next(100000, 999999));
            }
            else
            {
                booking.Status = BookingStatus.Failed;
                TempData["BookingError"] = Lang.Msg(HttpContext, "پرداخت توسط شما لغو شد.");
            }
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Result), new { id = booking.Booking_Id });
        }

        // POST: Booking/Cancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var booking = await _context.Booking!.FirstOrDefaultAsync(b => b.Booking_Id == id);
            if (booking == null) return NotFound();

            if (booking.Status == BookingStatus.Pending)
            {
                booking.Status = BookingStatus.Cancelled;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index", "Hotelsshow");
        }

        // GET: Booking/Result/5
        [HttpGet]
        public async Task<IActionResult> Result(int id)
        {
            var booking = await _context.Booking!
                .Include(b => b.Hotel)
                .FirstOrDefaultAsync(b => b.Booking_Id == id);

            if (booking == null) return NotFound();

            ViewBag.Error = TempData["BookingError"];
            return View(booking);
        }

        private async Task MarkPaidAsync(Booking booking, string? refId)
        {
            booking.RefId = refId;

            if (await HasOverlapAsync(booking.Hotel_Id, booking.CheckIn, booking.CheckOut, booking.Booking_Id))
            {
                booking.Status = BookingStatus.Failed;
                TempData["BookingError"] = Lang.Msg(HttpContext,
                    "پرداخت شما انجام شد اما در این فاصله همین بازه توسط شخص دیگری رزرو شده است. " +
                    "لطفا با پشتیبانی تماس بگیرید تا مبلغ بازگردانده شود (کد پیگیری: {0}).", refId ?? "-");
                return;
            }

            booking.Status = BookingStatus.Paid;
            booking.PaidAt = DateTime.Now;
        }

        private async Task SetDiscountViewBagAsync(Hotel hotel)
        {
            var info = HotelPricing.GetInfo(hotel, await HotelPricing.GetBookedSoonAsync(_context));
            ViewBag.DiscountPercent = info.Percent;
            ViewBag.DiscountUntil = info.OnlyUntil?.ToString("yyyy-MM-dd") ?? "";
        }

        private async Task<bool> HasOverlapAsync(int hotelId, DateTime checkIn, DateTime checkOut, int? excludeBookingId = null)
        {
            var cutoff = BookingRules.LockCutoff();

            var query = _context.Booking!.Where(b =>
                b.Hotel_Id == hotelId &&
                (b.Status == BookingStatus.Paid || (b.Status == BookingStatus.Locked && b.LockedAt > cutoff)) &&
                b.CheckIn < checkOut && b.CheckOut > checkIn);

            if (excludeBookingId.HasValue)
            {
                query = query.Where(b => b.Booking_Id != excludeBookingId.Value);
            }

            return await query.AnyAsync();
        }

        private async Task<List<Booking>> GetBookedRangesAsync(int hotelId)
        {
            var cutoff = BookingRules.LockCutoff();

            return await _context.Booking!
                .Where(b => b.Hotel_Id == hotelId &&
                    (b.Status == BookingStatus.Paid || (b.Status == BookingStatus.Locked && b.LockedAt > cutoff)))
                .OrderBy(b => b.CheckIn)
                .ToListAsync();
        }
    }
}
