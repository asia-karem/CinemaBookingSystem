using CinemaBookingSystem.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CinemaBookingSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Get the latest 8 movies
            var movies = await _context.Movies
                .Include(m => m.Category)
                .Include(m => m.Cinema)
                .OrderByDescending(m => m.Id)
                .Take(8)
                .ToListAsync();

            // Dashboard statistics
            ViewBag.TotalMovies = await _context.Movies.CountAsync();

            ViewBag.TotalCinemas = await _context.Cinemas.CountAsync();

            ViewBag.TotalShowtimes = await _context.Showtimes.CountAsync();

            ViewBag.TotalBookings = await _context.Bookings.CountAsync();

            ViewBag.TotalRevenue = await _context.Bookings
                .SumAsync(b => (decimal?)b.TotalPrice) ?? 0m;

            return View(movies);
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}