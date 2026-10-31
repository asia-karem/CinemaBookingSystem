using CinemaBookingSystem.Data;
using CinemaBookingSystem.ViewModels;
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
            var model = new DashboardViewModel
            {
                MoviesCount = await _context.Movies.CountAsync(),
                CinemasCount = await _context.Cinemas.CountAsync(),
                CategoriesCount = await _context.Categories.CountAsync(),
                ShowtimesCount = await _context.Showtimes.CountAsync(),
                BookingsCount = await _context.Bookings.CountAsync(),

                LatestMovies = await _context.Movies
                    .Include(m => m.Category)
                    .Include(m => m.Cinema)
                    .OrderByDescending(m => m.Id)
                    .Take(5)
                    .ToListAsync()
            };

            return View(model);
        }
    }
}
