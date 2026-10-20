
using System.ComponentModel.DataAnnotations;

namespace CinemaBookingSystem.Models
{
    public class Cinema
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(250)]
        public string Address { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public string? Img { get; set; }

        public ICollection<Movie> Movies { get; set; }
            = new List<Movie>();
    }
}
