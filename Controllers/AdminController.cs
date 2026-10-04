using agancywebProject.Data;
using agancywebProject.Helpers;
using agancywebProject.Models.DB;
using agancywebProject.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace agancywebProject.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Authorize]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction(nameof(Index));
            }
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(loginadmin l, string? returnUrl)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(l);
            }

            var user = await _context.UserLogin!
                .FirstOrDefaultAsync(m => m.username == l.username);

            var passwordOk = false;
            if (user != null)
            {
                if (PasswordHasher.IsHashed(user.password))
                {
                    passwordOk = PasswordHasher.Verify(l.password, user.password);
                }
                else
                {
                    passwordOk = CryptographicOperations.FixedTimeEquals(
                        Encoding.UTF8.GetBytes(l.password),
                        Encoding.UTF8.GetBytes(user.password ?? string.Empty));

                    if (passwordOk)
                    {
                        user.password = PasswordHasher.Hash(l.password);
                        await _context.SaveChangesAsync();
                    }
                }
            }

            if (user == null || !passwordOk)
            {
                ModelState.AddModelError(string.Empty, Lang.Msg(HttpContext, "نام کاربری یا رمز عبور اشتباه است"));
                return View(l);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.username),
                new Claim(ClaimTypes.NameIdentifier, user.User_Id.ToString())
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity));

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }
            return RedirectToAction(nameof(Index));
        }

        // TEMPORARY helper: while logged in as admin, open /Admin/ExportSeedData to download all current
        // hotels and flights as JSON (no bookings, no users/passwords). Used to build the project's seed data.
        // Delete this action once the seed data has been created.
        [Authorize]
        public async Task<IActionResult> ExportSeedData()
        {
            var hotels = await _context.Hotel!.OrderBy(h => h.Hotel_Id)
                .Select(h => new
                {
                    h.name, h.city, h.startdate, h.finishdate, h.price, h.address, h.star, h.meal,
                    h.description, h.amenities, h.photoUrls, h.roomPhotoUrls, h.roomName, h.roomCapacity
                }).ToListAsync();
            var flights = await _context.Flight!.OrderBy(f => f.Flight_Id)
                .Select(f => new
                {
                    f.Origin, f.distination, f.startdate, f.price, f.capacity, f.entrytime, f.flightClass
                }).ToListAsync();

            var json = System.Text.Json.JsonSerializer.Serialize(new { hotels, flights },
                new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                });
            return File(Encoding.UTF8.GetBytes(json), "application/json", "seed-data.json");
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }
    }
}
