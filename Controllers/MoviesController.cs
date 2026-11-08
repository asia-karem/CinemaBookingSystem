
using CinemaBookingSystem.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CinemaBookingSystem.Controllers
{
    public class MoviesController : Controller
    {
        private readonly IMovieRepository _movieRepository;

        public MoviesController(IMovieRepository movieRepository)
        {
            _movieRepository = movieRepository;
        }

        // Display all movies
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var movies = await _movieRepository
                .GetAllMoviesWithDetailsAsync();

            var orderedMovies = movies
                .OrderBy(m => m.Name)
                .ToList();

            return View(orderedMovies);
        }

        // Display movie details
        [AllowAnonymous]
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
    }
}
