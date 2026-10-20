
using System.ComponentModel.DataAnnotations;

namespace CinemaBookingSystem.Models
{
    public class MovieImage
    {
        public int Id { get; set; }

        [Required]
        public string ImageUrl { get; set; } = string.Empty;

        public int MovieId { get; set; }
        public Movie? Movie { get; set; }
    }
}
