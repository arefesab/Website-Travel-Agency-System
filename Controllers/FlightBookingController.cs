using System;
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
    public class FlightBookingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPaymentGateway _paymentGateway;

        public FlightBookingController(ApplicationDbContext context, IPaymentGateway paymentGateway)
        {
            _context = context;
            _paymentGateway = paymentGateway;
        }

        // GET: FlightBooking/Reserve/5
        [HttpGet]
        public async Task<IActionResult> Reserve(int? id)
        {
            if (id == null) return NotFound();

            var flight = await _context.Flight!.FirstOrDefaultAsync(f => f.Flight_Id == id);
            if (flight == null) return NotFound();

            if (flight.startdate.Date < DateTime.Today)
            {
                TempData["FlightBookingError"] = Lang.Msg(HttpContext, "زمان این پرواز گذشته است و قابل رزرو نیست.");
                return RedirectToAction("Index", "Flightsshow");
            }

            var booked = await GetBookedSeatsAsync(flight.Flight_Id);
            var remaining = flight.capacity - booked;

            if (remaining <= 0)
            {
                TempData["FlightBookingError"] = Lang.Msg(HttpContext, "ظرفیت این پرواز تکمیل شده است.");
                return RedirectToAction("Index", "Flightsshow");
            }

            ViewBag.Flight = flight;
            ViewBag.RemainingSeats = remaining;

            var booking = new FlightBooking
            {
                Flight_Id = flight.Flight_Id,
                SeatCount = 1
            };
            return View(booking);
        }

        // POST: FlightBooking/Reserve
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reserve([Bind("Flight_Id,SeatCount,FullName,PhoneNumber,Note")] FlightBooking booking)
        {
            var flight = await _context.Flight!.FirstOrDefaultAsync(f => f.Flight_Id == booking.Flight_Id);
            if (flight == null) return NotFound();

            var bookedSeats = await GetBookedSeatsAsync(flight.Flight_Id);
            var remaining = flight.capacity - bookedSeats;

            ViewBag.Flight = flight;
            ViewBag.RemainingSeats = remaining;

            if (flight.startdate.Date < DateTime.Today)
            {
                ModelState.AddModelError(string.Empty, Lang.Msg(HttpContext, "زمان این پرواز گذشته است و قابل رزرو نیست."));
            }
            else if (remaining <= 0)
            {
                ModelState.AddModelError(string.Empty, Lang.Msg(HttpContext, "متاسفانه ظرفیت این پرواز تکمیل شده است."));
            }
            else if (booking.SeatCount > remaining)
            {
                ModelState.AddModelError(nameof(booking.SeatCount), Lang.Msg(HttpContext, "فقط {0} صندلی از این پرواز باقی مانده است.", remaining));
            }

            if (!ModelState.IsValid)
            {
                return View(booking);
            }

            var total = (long)flight.price * booking.SeatCount;
            if (total > int.MaxValue)
            {
                ModelState.AddModelError(nameof(booking.SeatCount), Lang.Msg(HttpContext, "مبلغ کل این رزرو بیش از حد مجاز است. لطفا تعداد صندلی کمتری انتخاب کنید."));
                return View(booking);
            }

            booking.TotalPrice = (int)total;
            booking.Status = BookingStatus.Pending;
            booking.CreatedAt = DateTime.Now;

            _context.FlightBooking!.Add(booking);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Preview), new { id = booking.FlightBooking_Id });
        }

        // GET: FlightBooking/Preview/5
        [HttpGet]
        public async Task<IActionResult> Preview(int? id)
        {
            if (id == null) return NotFound();

            var booking = await _context.FlightBooking!
                .Include(b => b.Flight)
                .FirstOrDefaultAsync(b => b.FlightBooking_Id == id);

            if (booking == null || booking.Flight == null) return NotFound();

            if (booking.Status != BookingStatus.Pending)
            {
                return RedirectToAction(nameof(Result), new { id = booking.FlightBooking_Id });
            }

            var otherBooked = await GetBookedSeatsAsync(booking.Flight_Id, booking.FlightBooking_Id);
            if (otherBooked + booking.SeatCount > booking.Flight.capacity)
            {
                booking.Status = BookingStatus.Cancelled;
                await _context.SaveChangesAsync();
                TempData["FlightBookingError"] = Lang.Msg(HttpContext, "متاسفانه ظرفیت این پرواز هم‌اکنون توسط افراد دیگر تکمیل شد.");
                return RedirectToAction("Index", "Flightsshow");
            }

            ViewBag.Error = TempData["FlightBookingError"];
            return View(booking);
        }

        // POST: FlightBooking/Pay/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Pay(int id)
        {
            var booking = await _context.FlightBooking!
                .Include(b => b.Flight)
                .FirstOrDefaultAsync(b => b.FlightBooking_Id == id);

            if (booking == null || booking.Flight == null) return NotFound();

            if (booking.Status != BookingStatus.Pending)
            {
                return RedirectToAction(nameof(Result), new { id = booking.FlightBooking_Id });
            }

            var now = DateTime.Now;
            var cutoff = now - BookingRules.LockTimeout;

            var lockedRows = await _context.Database.ExecuteSqlInterpolatedAsync($@"
UPDATE FlightBooking SET Status = {(int)BookingStatus.Locked}, LockedAt = {now}
WHERE FlightBooking_Id = {booking.FlightBooking_Id} AND Status = {(int)BookingStatus.Pending}
AND (
    (SELECT ISNULL(SUM(b2.SeatCount), 0) FROM FlightBooking b2
     WHERE b2.Flight_Id = {booking.Flight_Id} AND b2.FlightBooking_Id <> {booking.FlightBooking_Id}
     AND (b2.Status = {(int)BookingStatus.Paid}
          OR (b2.Status = {(int)BookingStatus.Locked} AND b2.LockedAt > {cutoff})))
    + {booking.SeatCount}
) <= (SELECT capacity FROM Flight WHERE Flight_Id = {booking.Flight_Id})");

            if (lockedRows == 0)
            {
                booking.Status = BookingStatus.Cancelled;
                await _context.SaveChangesAsync();
                TempData["FlightBookingError"] = Lang.Msg(HttpContext, "متاسفانه ظرفیت این پرواز هم‌اکنون توسط افراد دیگر تکمیل شد.");
                return RedirectToAction("Index", "Flightsshow");
            }

            booking.Status = BookingStatus.Locked;
            booking.LockedAt = now;

            var callbackUrl = Url.Action(nameof(Callback), "FlightBooking", new { id = booking.FlightBooking_Id }, Request.Scheme)!;

            var result = await _paymentGateway.RequestPaymentAsync(new PaymentRequestModel
            {
                AmountToman = booking.TotalPrice,
                Description = $"رزرو پرواز {booking.Flight.Origin} به {booking.Flight.distination} - {booking.FullName}",
                CallbackUrl = callbackUrl,
                Mobile = booking.PhoneNumber,
                FakeGatewayController = "FlightBooking"
            });

            if (!result.Success)
            {
                booking.Status = BookingStatus.Failed;
                await _context.SaveChangesAsync();
                TempData["FlightBookingError"] = Lang.Msg(HttpContext, result.ErrorMessage ?? "اتصال به درگاه پرداخت برقرار نشد.");
                return RedirectToAction(nameof(Preview), new { id = booking.FlightBooking_Id });
            }

            booking.Authority = result.Authority;
            await _context.SaveChangesAsync();

            return Redirect(result.PaymentUrl!);
        }

        // GET: FlightBooking/Callback/5
        [HttpGet]
        public async Task<IActionResult> Callback(int id, string? Authority, string? Status)
        {
            var booking = await _context.FlightBooking!
                .Include(b => b.Flight)
                .FirstOrDefaultAsync(b => b.FlightBooking_Id == id);

            if (booking == null || booking.Flight == null) return NotFound();

            if (string.IsNullOrEmpty(booking.Authority) ||
                !string.Equals(Authority, booking.Authority, StringComparison.Ordinal))
            {
                return BadRequest();
            }

            if (booking.Status != BookingStatus.Locked)
            {
                return RedirectToAction(nameof(Result), new { id = booking.FlightBooking_Id });
            }

            if (!string.Equals(Status, "OK", StringComparison.OrdinalIgnoreCase))
            {
                booking.Status = BookingStatus.Failed;
                await _context.SaveChangesAsync();
                TempData["FlightBookingError"] = Lang.Msg(HttpContext, "پرداخت توسط شما لغو شد.");
                return RedirectToAction(nameof(Result), new { id = booking.FlightBooking_Id });
            }

            var verify = await _paymentGateway.VerifyPaymentAsync(booking.Authority, booking.TotalPrice);
            if (verify.Success)
            {
                await MarkPaidAsync(booking, verify.RefId);
            }
            else
            {
                booking.Status = BookingStatus.Failed;
                TempData["FlightBookingError"] = Lang.Msg(HttpContext, verify.ErrorMessage ?? "پرداخت تایید نشد.");
            }
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Result), new { id = booking.FlightBooking_Id });
        }

        // GET: FlightBooking/FakeGateway/FAKE-xxxx
        [HttpGet]
        public async Task<IActionResult> FakeGateway(string id)
        {
            if (!_paymentGateway.IsSimulated) return NotFound();

            var authority = id;
            var booking = await _context.FlightBooking!
                .Include(b => b.Flight)
                .FirstOrDefaultAsync(b => b.Authority == authority);

            if (booking == null || booking.Flight == null) return NotFound();
            if (booking.Status != BookingStatus.Locked)
            {
                return RedirectToAction(nameof(Result), new { id = booking.FlightBooking_Id });
            }

            return View(booking);
        }

        // POST: FlightBooking/FakeGatewayConfirm
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FakeGatewayConfirm(string authority, bool approved)
        {
            if (!_paymentGateway.IsSimulated) return NotFound();

            var booking = await _context.FlightBooking!
                .Include(b => b.Flight)
                .FirstOrDefaultAsync(b => b.Authority == authority);

            if (booking == null || booking.Flight == null) return NotFound();

            if (booking.Status != BookingStatus.Locked)
            {
                return RedirectToAction(nameof(Result), new { id = booking.FlightBooking_Id });
            }

            if (approved)
            {
                await MarkPaidAsync(booking, "TEST" + new Random().Next(100000, 999999));
            }
            else
            {
                booking.Status = BookingStatus.Failed;
                TempData["FlightBookingError"] = Lang.Msg(HttpContext, "پرداخت توسط شما لغو شد.");
            }
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Result), new { id = booking.FlightBooking_Id });
        }

        // POST: FlightBooking/Cancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var booking = await _context.FlightBooking!.FirstOrDefaultAsync(b => b.FlightBooking_Id == id);
            if (booking == null) return NotFound();

            if (booking.Status == BookingStatus.Pending)
            {
                booking.Status = BookingStatus.Cancelled;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index", "Flightsshow");
        }

        // GET: FlightBooking/Result/5
        [HttpGet]
        public async Task<IActionResult> Result(int id)
        {
            var booking = await _context.FlightBooking!
                .Include(b => b.Flight)
                .FirstOrDefaultAsync(b => b.FlightBooking_Id == id);

            if (booking == null) return NotFound();

            ViewBag.Error = TempData["FlightBookingError"];
            return View(booking);
        }

        private async Task MarkPaidAsync(FlightBooking booking, string? refId)
        {
            booking.RefId = refId;

            var otherBooked = await GetBookedSeatsAsync(booking.Flight_Id, booking.FlightBooking_Id);
            if (booking.Flight != null && otherBooked + booking.SeatCount > booking.Flight.capacity)
            {
                booking.Status = BookingStatus.Failed;
                TempData["FlightBookingError"] = Lang.Msg(HttpContext,
                    "پرداخت شما انجام شد اما در این فاصله ظرفیت پرواز توسط افراد دیگر تکمیل شده است. " +
                    "لطفا با پشتیبانی تماس بگیرید تا مبلغ بازگردانده شود (کد پیگیری: {0}).", refId ?? "-");
                return;
            }

            booking.Status = BookingStatus.Paid;
            booking.PaidAt = DateTime.Now;
        }

        private async Task<int> GetBookedSeatsAsync(int flightId, int? excludeBookingId = null)
        {
            var cutoff = BookingRules.LockCutoff();

            var query = _context.FlightBooking!.Where(b =>
                b.Flight_Id == flightId &&
                (b.Status == BookingStatus.Paid || (b.Status == BookingStatus.Locked && b.LockedAt > cutoff)));

            if (excludeBookingId.HasValue)
            {
                query = query.Where(b => b.FlightBooking_Id != excludeBookingId.Value);
            }

            return await query.SumAsync(b => (int?)b.SeatCount) ?? 0;
        }
    }
}
