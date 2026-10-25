
using CinemaBookingSystem.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CinemaBookingSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.AdminEmail = User.Identity?.Name ?? "Admin";

            ViewBag.TotalMovies =
                await _context.Movies.CountAsync();

            ViewBag.TotalCinemas =
                await _context.Cinemas.CountAsync();

            ViewBag.TotalShowtimes =
                await _context.Showtimes.CountAsync();

            ViewBag.TotalBookings =
                await _context.Bookings.CountAsync();

            ViewBag.TotalRevenue =
                await _context.Bookings
                    .Where(b => b.Status != "Cancelled")
                    .SumAsync(b => (decimal?)b.TotalPrice) ?? 0m;

            return View();
        }

        [Authorize(Roles = "Admin")]
        public IActionResult AccessDenied()
        {
            return RedirectToAction("AccessDenied", "Account");
        }
    }
}
