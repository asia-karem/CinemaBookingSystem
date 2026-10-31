using CinemaBookingSystem.Models;

namespace CinemaBookingSystem.ViewModels
{
    public class DashboardViewModel
    {
        public int MoviesCount { get; set; }
        public int CinemasCount { get; set; }
        public int CategoriesCount { get; set; }
        public int ShowtimesCount { get; set; }
        public int BookingsCount { get; set; }

        public List<Movie> LatestMovies { get; set; } = new();
    }
}
