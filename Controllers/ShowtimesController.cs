
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

        // GET: Showtimes
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var showtimes = await _context.Showtimes
                .AsNoTracking()
                .Include(s => s.Movie)
                .OrderBy(s => s.StartTime)
                .ToListAsync();

            return View(showtimes);
        }

        // GET: Showtimes/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadMovies();

            return View(new MovieShowtime());
        }

        // POST: Showtimes/Create
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

            if (showtime.AvailableSeats < 1 ||
                showtime.AvailableSeats > 500)
            {
                ModelState.AddModelError(
                    nameof(showtime.AvailableSeats),
                    "Available seats must be between 1 and 500.");
            }

            if (!ModelState.IsValid)
            {
                await LoadMovies(showtime.MovieId);
                return View(showtime);
            }

            try
            {
                _context.Showtimes.Add(showtime);

                await _context.SaveChangesAsync();

                TempData["Success"] =
                    "Showtime added successfully!";

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Database error: the showtime could not be saved.");

                await LoadMovies(showtime.MovieId);

                return View(showtime);
            }
        }

        // Load movies into the dropdown.
        private async Task LoadMovies(int? selectedMovie = null)
        {
            var movies = await _context.Movies
                .AsNoTracking()
                .OrderBy(m => m.Name)
                .ToListAsync();

            ViewBag.NoMovies = !movies.Any();

            ViewBag.Movies = new SelectList(
                movies,
                "Id",
                "Name",
                selectedMovie);
        }
    }
}
