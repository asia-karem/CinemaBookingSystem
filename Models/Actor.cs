
using System.ComponentModel.DataAnnotations;

namespace CinemaBookingSystem.Models
{
    public class Actor
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        public string? Bio { get; set; }

        public string? Img { get; set; }

        public ICollection<Movie> Movies { get; set; }
            = new List<Movie>();
    }
}
