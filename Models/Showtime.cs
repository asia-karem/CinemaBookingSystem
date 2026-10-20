
using System;
using System.ComponentModel.DataAnnotations;

namespace CinemaBookingSystem.Models
{
    public class Showtime
    {
        public int Id { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        [Range(1, 500)]
        public int AvailableSeats { get; set; } = 100;

        public int MovieId { get; set; }

        public Movie? Movie { get; set; }
    }
}
