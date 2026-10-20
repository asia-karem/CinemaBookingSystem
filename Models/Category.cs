
using System.ComponentModel.DataAnnotations;

namespace CinemaBookingSystem.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        [StringLength(250)]
        public string? Description { get; set; }

        public ICollection<Movie> Movies { get; set; }
            = new List<Movie>();
    }
}
