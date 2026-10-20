

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CinemaBookingSystem.Models
{
    public class Movie
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Movie name is required.")]
        [StringLength(100, MinimumLength = 2,
            ErrorMessage = "Movie name must be between 2 and 100 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required.")]
        [StringLength(2000, MinimumLength = 10,
            ErrorMessage = "Description must be between 10 and 2000 characters.")]
        public string Des { get; set; } = string.Empty;

        [Required(ErrorMessage = "Price is required.")]
        [Column(TypeName = "decimal(10,2)")]
        [Range(typeof(decimal), "0.01", "10000",
            ErrorMessage = "Price must be between 0.01 and 10000.")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Please select a status.")]
        [RegularExpression("^(Active|Inactive)$",
            ErrorMessage = "Status must be Active or Inactive.")]
        public string Status { get; set; } = "Active";

        [Required(ErrorMessage = "Show date and time are required.")]
        public DateTime DateTime { get; set; }

        [Required(ErrorMessage = "Please select a main image.")]
        public string MainImg { get; set; } = string.Empty;

        public ICollection<MovieImage> SubImages { get; set; }
            = new List<MovieImage>();

        [Range(1, int.MaxValue,
            ErrorMessage = "Please select a category.")]
        public int CategoryId { get; set; }

        public Category? Category { get; set; }

        [Range(1, int.MaxValue,
            ErrorMessage = "Please select a cinema.")]
        public int CinemaId { get; set; }

        public Cinema? Cinema { get; set; }

        public ICollection<Actor> Actors { get; set; }
            = new List<Actor>();

        public ICollection<Showtime> Showtimes { get; set; }
            = new List<Showtime>();
    }
}
