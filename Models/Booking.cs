
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace CinemaBookingSystem.Models
{
    public class Booking
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string CustomerName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Range(1, 10)]
        public int NumberOfTickets { get; set; } = 1;

        [Column(TypeName = "decimal(10,2)")]
        [Range(typeof(decimal), "0.01", "100000")]
        public decimal TotalPrice { get; set; }

        public DateTime BookingDate { get; set; } = DateTime.UtcNow;

        [Required]
        public string Status { get; set; } = "Pending";

        public int ShowtimeId { get; set; }

        public Showtime? Showtime { get; set; }

        // Identity user who made the booking
        public string? UserId { get; set; }

        public IdentityUser? User { get; set; }
    }
}
