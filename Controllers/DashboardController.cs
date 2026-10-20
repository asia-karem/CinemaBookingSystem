
using CinemaBookingSystem.Data;

using CinemaBookingSystem.Data;
using CinemaBookingSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CinemaBookingSystem.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.MoviesCount = await _context.Movies.CountAsync();
            ViewBag.CinemasCount = await _context.Cinemas.CountAsync();
            ViewBag.CategoriesCount = await _context.Categories.CountAsync();
            ViewBag.ShowtimesCount = await _context.Showtimes.CountAsync();
            ViewBag.BookingsCount = await _context.Bookings.CountAsync();

            var latestMovies = await _context.Movies
                .Include(m => m.Category)
                .Include(m => m.Cinema)
                .OrderByDescending(m => m.Id)
                .Take(5)
                .ToListAsync();

            return View(latestMovies);
        }
    }
}

