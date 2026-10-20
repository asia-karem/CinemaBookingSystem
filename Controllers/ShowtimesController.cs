


using CinemaBookingSystem.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MovieShowtime = CinemaBookingSystem.Models.Showtime;

namespace CinemaBookingSystem.Controllers
{
    public class ShowtimesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ShowtimesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var showtimes = await _context.Showtimes
                .Include(s => s.Movie)
                .OrderBy(s => s.StartTime)
                .ToListAsync();

            return View(showtimes);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadMovies();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MovieShowtime showtime)
        {
            bool movieExists = await _context.Movies
                .AnyAsync(m => m.Id == showtime.MovieId);

            if (!movieExists)
            {
                ModelState.AddModelError(
                    nameof(showtime.MovieId),
                    "Please select a valid movie.");
            }

            if (showtime.StartTime <= DateTime.Now)
            {
                ModelState.AddModelError(
                    nameof(showtime.StartTime),
                    "Showtime must be in the future.");
            }

            if (!ModelState.IsValid)
            {
                await LoadMovies(showtime.MovieId);
                return View(showtime);
            }

            _context.Showtimes.Add(showtime);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Showtime added successfully!";

            return RedirectToAction(nameof(Index));
        }

        private async Task LoadMovies(int? selectedMovie = null)
        {
            var movies = await _context.Movies
                .OrderBy(m => m.Name)
                .ToListAsync();

            ViewBag.Movies = new SelectList(
                movies,
                "Id",
                "Name",
                selectedMovie);
        }
    }
}

