
using CinemaBookingSystem.Data;
using CinemaBookingSystem.Models;
using CinemaBookingSystem.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace CinemaBookingSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class MoviesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMovieRepository _movieRepository;
        private readonly IWebHostEnvironment _environment;

        public MoviesController(
            ApplicationDbContext context,
            IMovieRepository movieRepository,
            IWebHostEnvironment environment)
        {
            _context = context;
            _movieRepository = movieRepository;
            _environment = environment;
        }

        // Display all movies - Admin only
        public async Task<IActionResult> Index()
        {
            var movies = await _movieRepository
                .GetAllMoviesWithDetailsAsync();

            var orderedMovies = movies
                .OrderBy(m => m.Name)
                .ToList();

            return View(orderedMovies);
        }

        // GET: Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadDropdowns();
            LoadMovieImages();

            return View();
        }

        // POST: Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            Movie movie,
            string? mainImagePath,
            List<string>? subImagePaths)
        {
            ModelState.Remove(nameof(movie.MainImg));

            var availableImages = GetAvailableMovieImages();

            if (!await _context.Categories
                .AnyAsync(c => c.Id == movie.CategoryId))
            {
                ModelState.AddModelError(
                    nameof(movie.CategoryId),
                    "Please select a valid category.");
            }

            if (!await _context.Cinemas
                .AnyAsync(c => c.Id == movie.CinemaId))
            {
                ModelState.AddModelError(
                    nameof(movie.CinemaId),
                    "Please select a valid cinema.");
            }

            if (string.IsNullOrWhiteSpace(mainImagePath) ||
                !availableImages.Contains(mainImagePath))
            {
                ModelState.AddModelError(
                    "mainImagePath",
                    "Please select a valid main movie image.");
            }
            else
            {
                movie.MainImg = mainImagePath;
            }

            if (movie.DateTime == default)
            {
                ModelState.AddModelError(
                    nameof(movie.DateTime),
                    "Please select a valid date and time.");
            }

            if (!ModelState.IsValid)
            {
                await LoadDropdowns(movie.CategoryId, movie.CinemaId);
                LoadMovieImages();

                return View(movie);
            }

            var validSubImages = (subImagePaths ?? new List<string>())
                .Where(path => !string.IsNullOrWhiteSpace(path))
                .Where(path => availableImages.Contains(path))
                .Where(path => path != movie.MainImg)
                .Distinct()
                .ToList();

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                await _movieRepository.AddAsync(movie);
                await _movieRepository.SaveAsync();

                foreach (var imagePath in validSubImages)
                {
                    _context.MovieImages.Add(new MovieImage
                    {
                        MovieId = movie.Id,
                        ImageUrl = imagePath
                    });
                }

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                TempData["Success"] = "Movie added successfully!";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                _context.ChangeTracker.Clear();

                ModelState.AddModelError(
                    string.Empty,
                    "The movie could not be saved. Please check your data and try again.");

                await LoadDropdowns(movie.CategoryId, movie.CinemaId);
                LoadMovieImages();

                return View(movie);
            }
        }

        // GET: Edit
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var movie = await _movieRepository.GetByIdAsync(id.Value);

            if (movie == null)
                return NotFound();

            await LoadDropdowns(movie.CategoryId, movie.CinemaId);
            LoadMovieImages();

            return View(movie);
        }

        // POST: Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Movie movie,
            string? mainImagePath)
        {
            if (id != movie.Id)
                return NotFound();

            var existingMovie =
                await _movieRepository.GetByIdAsync(id);

            if (existingMovie == null)
                return NotFound();

            ModelState.Remove(nameof(movie.MainImg));

            var availableImages = GetAvailableMovieImages();

            if (!await _context.Categories
                .AnyAsync(c => c.Id == movie.CategoryId))
            {
                ModelState.AddModelError(
                    nameof(movie.CategoryId),
                    "Please select a valid category.");
            }

            if (!await _context.Cinemas
                .AnyAsync(c => c.Id == movie.CinemaId))
            {
                ModelState.AddModelError(
                    nameof(movie.CinemaId),
                    "Please select a valid cinema.");
            }

            if (!string.IsNullOrWhiteSpace(mainImagePath) &&
                !availableImages.Contains(mainImagePath))
            {
                ModelState.AddModelError(
                    "mainImagePath",
                    "Please select a valid main movie image.");
            }

            if (movie.DateTime == default)
            {
                ModelState.AddModelError(
                    nameof(movie.DateTime),
                    "Please select a valid date and time.");
            }

            if (!ModelState.IsValid)
            {
                movie.MainImg = existingMovie.MainImg;

                await LoadDropdowns(movie.CategoryId, movie.CinemaId);
                LoadMovieImages();

                return View(movie);
            }

            existingMovie.Name = movie.Name;
            existingMovie.Des = movie.Des;
            existingMovie.Price = movie.Price;
            existingMovie.Status = movie.Status;
            existingMovie.DateTime = movie.DateTime;
            existingMovie.CategoryId = movie.CategoryId;
            existingMovie.CinemaId = movie.CinemaId;

            if (!string.IsNullOrWhiteSpace(mainImagePath))
            {
                existingMovie.MainImg = mainImagePath;
            }

            try
            {
                _movieRepository.Update(existingMovie);
                await _movieRepository.SaveAsync();

                TempData["Success"] = "Movie updated successfully!";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "The movie could not be updated. Please try again.");

                movie.MainImg = existingMovie.MainImg;

                await LoadDropdowns(movie.CategoryId, movie.CinemaId);
                LoadMovieImages();

                return View(movie);
            }
        }

        // GET: Delete
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var movie = await _movieRepository
                .GetMovieWithDetailsAsync(id.Value);

            if (movie == null)
                return NotFound();

            return View(movie);
        }

        // POST: Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var movie = await _movieRepository
                .GetMovieWithDetailsAsync(id);

            if (movie == null)
                return NotFound();

            bool hasShowtimes = await _context.Showtimes
                .AnyAsync(s => s.MovieId == id);

            if (hasShowtimes)
            {
                TempData["Error"] =
                    "Cannot delete this movie because it has showtimes.";

                return RedirectToAction(nameof(Index));
            }

            try
            {
                if (movie.SubImages != null)
                {
                    _context.MovieImages.RemoveRange(movie.SubImages);
                }

                _movieRepository.Delete(movie);

                await _movieRepository.SaveAsync();

                TempData["Success"] = "Movie deleted successfully!";
            }
            catch (Exception)
            {
                TempData["Error"] =
                    "The movie could not be deleted. Please try again.";
            }

            return RedirectToAction(nameof(Index));
        }

        // Movie details - Admin only
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var movie = await _movieRepository
                .GetMovieWithDetailsAsync(id.Value);

            if (movie == null)
                return NotFound();

            return View(movie);
        }

        // Load categories and cinemas
        private async Task LoadDropdowns(
            int? selectedCategory = null,
            int? selectedCinema = null)
        {
            ViewBag.Categories = new SelectList(
                await _context.Categories
                    .OrderBy(c => c.Name)
                    .ToListAsync(),
                "Id",
                "Name",
                selectedCategory);

            ViewBag.Cinemas = new SelectList(
                await _context.Cinemas
                    .OrderBy(c => c.Name)
                    .ToListAsync(),
                "Id",
                "Name",
                selectedCinema);
        }

        // Get available movie images
        private List<string> GetAvailableMovieImages()
        {
            var imagesFolder = Path.Combine(
                _environment.WebRootPath ?? "wwwroot",
                "images",
                "movies");

            if (!Directory.Exists(imagesFolder))
                return new List<string>();

            var allowedExtensions = new[]
            {
                ".jpg", ".jpeg", ".png", ".webp"
            };

            return Directory.GetFiles(imagesFolder)
                .Where(file => allowedExtensions.Contains(
                    Path.GetExtension(file),
                    StringComparer.OrdinalIgnoreCase))
                .Select(file =>
                    "/images/movies/" + Path.GetFileName(file))
                .ToList();
        }

        // Send available images to the view
        private void LoadMovieImages()
        {
            ViewBag.MovieImages = GetAvailableMovieImages();
        }
    }
}
