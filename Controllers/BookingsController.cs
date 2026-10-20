
using CinemaBookingSystem.Data;
using CinemaBookingSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CinemaBookingSystem.Controllers
{
    public class BookingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BookingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Create(int? showtimeId)
        {
            await LoadShowtimes(showtimeId);

            var booking = new Booking();

            if (showtimeId.HasValue)
            {
                var showtime = await _context.Showtimes
                    .Include(s => s.Movie)
                    .FirstOrDefaultAsync(s => s.Id == showtimeId.Value);

                if (showtime == null)
                    return NotFound();

                if (showtime.AvailableSeats < 1)
                {
                    TempData["Error"] = "This show is fully booked.";
                    return RedirectToAction("Index", "Movies");
                }

                booking.ShowtimeId = showtime.Id;
            }

            return View(booking);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Booking booking)
        {
            ModelState.Remove(nameof(Booking.TotalPrice));
            ModelState.Remove(nameof(Booking.Status));
            ModelState.Remove(nameof(Booking.BookingDate));
            ModelState.Remove(nameof(Booking.Showtime));

            var showtime = await _context.Showtimes
                .Include(s => s.Movie)
                .FirstOrDefaultAsync(s => s.Id == booking.ShowtimeId);

            if (showtime == null)
                ModelState.AddModelError(
                    nameof(Booking.ShowtimeId),
                    "Please select a valid showtime.");
            else if (booking.NumberOfTickets > showtime.AvailableSeats)
                ModelState.AddModelError(
                    nameof(Booking.NumberOfTickets),
                    "Not enough seats are available.");

            if (!ModelState.IsValid)
            {
                await LoadShowtimes(booking.ShowtimeId);
                return View(booking);
            }

            booking.TotalPrice =
                showtime!.Movie!.Price * booking.NumberOfTickets;

            booking.Status = "Pending";
            booking.BookingDate = DateTime.UtcNow;

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                showtime.AvailableSeats -= booking.NumberOfTickets;

                _context.Bookings.Add(booking);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["Success"] =
                    "Your booking was created successfully!";

                return RedirectToAction(nameof(Confirmation),
                    new { id = booking.Id });
            }
            catch
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError(
                    string.Empty,
                    "The booking could not be saved. Please try again.");

                await LoadShowtimes(booking.ShowtimeId);
                return View(booking);
            }
        }

        public async Task<IActionResult> Confirmation(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.Showtime)
                .ThenInclude(s => s!.Movie)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null)
                return NotFound();

            return View(booking);
        }

        private async Task LoadShowtimes(int? selectedId = null)
        {
            var showtimes = await _context.Showtimes
                .Include(s => s.Movie)
                .Where(s => s.AvailableSeats > 0)
                .OrderBy(s => s.StartTime)
                .ToListAsync();

            ViewBag.Showtimes = new SelectList(
                showtimes.Select(s => new
                {
                    s.Id,
                    Display = (s.Movie != null
                        ? s.Movie.Name
                        : "Movie") + " — " +
                        s.StartTime.ToString("dd MMM yyyy, hh:mm tt") +
                        " — Seats: " + s.AvailableSeats
                }),
                "Id",
                "Display",
                selectedId);
        }
    }
}
