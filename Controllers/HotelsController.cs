using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using agancywebProject.Data;
using agancywebProject.Helpers;
using agancywebProject.Models.DB;
using agancywebProject.Models;

namespace agancywebProject.Controllers
{
    public class HotelsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly Microsoft.AspNetCore.Hosting.IWebHostEnvironment _env;

        public HotelsController(ApplicationDbContext context, Microsoft.AspNetCore.Hosting.IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // POST: Hotels/UploadPhotos
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        [RequestSizeLimit(20_000_000)]
        public async Task<IActionResult> UploadPhotos(List<IFormFile> files)
        {
            if (files == null || files.Count == 0)
            {
                return BadRequest(new { message = Lang.Msg(HttpContext, "هیچ فایلی ارسال نشده است.") });
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "hotels");
            Directory.CreateDirectory(uploadsFolder);

            var savedUrls = new List<string>();

            foreach (var file in files)
            {
                if (file.Length == 0) continue;

                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(extension)) continue;

                var uniqueName = $"{Guid.NewGuid():N}{extension}";
                var fullPath = Path.Combine(uploadsFolder, uniqueName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                savedUrls.Add($"~/uploads/hotels/{uniqueName}");
            }

            if (savedUrls.Count == 0)
            {
                return BadRequest(new { message = Lang.Msg(HttpContext, "فرمت فایل‌ها پشتیبانی نمی‌شود (فقط jpg, jpeg, png, webp, gif).") });
            }

            return Json(new { urls = savedUrls });
        }

        // GET: Hotels
        [Authorize]
        public async Task<IActionResult> Index()
        {
            if (_context.Hotel == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Hotel'  is null.");
            }

            var hotels = await _context.Hotel.ToListAsync();

            var paidCounts = _context.Booking == null
                ? new Dictionary<int, int>()
                : await _context.Booking
                    .Where(b => b.Status == BookingStatus.Paid)
                    .GroupBy(b => b.Hotel_Id)
                    .Select(g => new { HotelId = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.HotelId, x => x.Count);

            ViewBag.PaidBookingCounts = paidCounts;
            return View(hotels);
        }

        // GET: Hotels/Details/5
        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Hotel == null)
            {
                return NotFound();
            }

            var hotel = await _context.Hotel
                .FirstOrDefaultAsync(m => m.Hotel_Id == id);
            if (hotel == null)
            {
                return NotFound();
            }

            ViewBag.Bookings = _context.Booking == null
                ? new List<Booking>()
                : await _context.Booking
                    .Where(b => b.Hotel_Id == hotel.Hotel_Id)
                    .OrderByDescending(b => b.CreatedAt)
                    .ToListAsync();

            return View(hotel);
        }

        // GET: Hotels/Create
        [Authorize]
        public async Task<IActionResult> Create()
        {
            await PopulateHotelNamesAsync();
            return View();
        }
      
        // POST: Hotels/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create([Bind("Hotel_Id,name,city,startdate,finishdate,price,address,star,meal,description,amenities,photoUrls,roomPhotoUrls,roomName,roomCapacity,DiscountPercent")] Hotel hotel)
        {
            if (hotel.startdate.Date < DateTime.Today)
                ModelState.AddModelError(nameof(Hotel.startdate), Lang.Msg(HttpContext, "تاریخ نمی‌تواند در گذشته باشد."));
            if (hotel.finishdate.Date < DateTime.Today)
                ModelState.AddModelError(nameof(Hotel.finishdate), Lang.Msg(HttpContext, "تاریخ نمی‌تواند در گذشته باشد."));

            if (ModelState.IsValid)
            {
                _context.Add(hotel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            await PopulateHotelNamesAsync();
            return View(hotel);
        }

        // GET: Hotels/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Hotel == null)
            {
                return NotFound();
            }

            var hotel = await _context.Hotel.FindAsync(id);
            if (hotel == null)
            {
                return NotFound();
            }
            await PopulateHotelNamesAsync();
            return View(hotel);
        }

        // POST: Hotels/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(int id, [Bind("Hotel_Id,name,city,startdate,finishdate,price,address,star,meal,description,amenities,photoUrls,roomPhotoUrls,roomName,roomCapacity,DiscountPercent")] Hotel hotel)
        {
            if (id != hotel.Hotel_Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(hotel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!HotelExists(hotel.Hotel_Id))
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
            await PopulateHotelNamesAsync();
            return View(hotel);
        }

        // GET: Hotels/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Hotel == null)
            {
                return NotFound();
            }

            var hotel = await _context.Hotel
                .FirstOrDefaultAsync(m => m.Hotel_Id == id);
            if (hotel == null)
            {
                return NotFound();
            }

            return View(hotel);
        }

        // POST: Hotels/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Hotel == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Hotel'  is null.");
            }
            if (_context.Booking != null)
            {
                var cutoff = BookingRules.LockCutoff();
                var hasActiveBookings = await _context.Booking.AnyAsync(b =>
                    b.Hotel_Id == id &&
                    (b.Status == BookingStatus.Paid || (b.Status == BookingStatus.Locked && b.LockedAt > cutoff)));

                if (hasActiveBookings)
                {
                    TempData["AdminError"] = Lang.Msg(HttpContext, "این هتل رزرو پرداخت‌شده یا در حال پرداخت دارد و قابل حذف نیست.");
                    return RedirectToAction(nameof(Index));
                }
            }

            var hotel = await _context.Hotel.FindAsync(id);
            if (hotel != null)
            {
                _context.Hotel.Remove(hotel);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateHotelNamesAsync()
        {
            if (_context.Hotel == null)
            {
                ViewBag.HotelNames = new List<string>();
                ViewBag.HotelNameCityMapJson = "{}";
                ViewBag.HotelNamePhotosMapJson = "{}";
                ViewBag.HotelNameDetailsMapJson = "{}";
                return;
            }

            var rows = await _context.Hotel
                .Where(h => h.name != null && h.city != null)
                .Select(h => new { h.name, h.city })
                .ToListAsync();

            var names = rows.Select(r => r.name!).Distinct().OrderBy(n => n).ToList();

            var nameCityMap = rows
                .GroupBy(r => r.name!)
                .ToDictionary(g => g.Key, g => g.Select(x => x.city!).Distinct().OrderBy(c => c).ToList());

            // Hotel photos entered before for each hotel name (latest record that has photos wins).
            var photoRows = await _context.Hotel
                .Where(h => h.name != null && h.photoUrls != null && h.photoUrls != "")
                .OrderBy(h => h.Hotel_Id)
                .Select(h => new { h.name, h.photoUrls })
                .ToListAsync();
            var namePhotosMap = new Dictionary<string, string>();
            foreach (var r in photoRows)
            {
                namePhotosMap[r.name!] = r.photoUrls!;
            }

            // Hotel details entered before for each hotel name: for every field the latest non-empty value wins.
            var detailRows = await _context.Hotel
                .Where(h => h.name != null)
                .OrderBy(h => h.Hotel_Id)
                .Select(h => new { h.name, h.description, h.amenities, h.address, h.star })
                .ToListAsync();
            var nameDetailsMap = new Dictionary<string, Dictionary<string, object>>();
            foreach (var r in detailRows)
            {
                if (!nameDetailsMap.TryGetValue(r.name!, out var d))
                {
                    d = new Dictionary<string, object>();
                    nameDetailsMap[r.name!] = d;
                }
                if (!string.IsNullOrWhiteSpace(r.description)) d["description"] = r.description!;
                if (!string.IsNullOrWhiteSpace(r.amenities)) d["amenities"] = r.amenities!;
                if (!string.IsNullOrWhiteSpace(r.address)) d["address"] = r.address!;
                if (r.star >= 1 && r.star <= 5) d["star"] = r.star;
            }

            ViewBag.HotelNames = names;
            ViewBag.HotelNameCityMapJson = System.Text.Json.JsonSerializer.Serialize(nameCityMap);
            ViewBag.HotelNamePhotosMapJson = System.Text.Json.JsonSerializer.Serialize(namePhotosMap);
            ViewBag.HotelNameDetailsMapJson = System.Text.Json.JsonSerializer.Serialize(nameDetailsMap);
        }

        private bool HotelExists(int id)
        {
          return (_context.Hotel?.Any(e => e.Hotel_Id == id)).GetValueOrDefault();
        }
    }
}
