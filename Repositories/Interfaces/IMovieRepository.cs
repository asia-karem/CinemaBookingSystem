using CinemaBookingSystem.Models;

namespace CinemaBookingSystem.Repositories.Interfaces
{
    public interface IMovieRepository : IGenericRepository<Movie>
    {
        Task<IEnumerable<Movie>> GetAllMoviesWithDetailsAsync();

        Task<Movie?> GetMovieWithDetailsAsync(int id);
    }
}