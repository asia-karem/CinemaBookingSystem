
using CinemaBookingSystem.Data;
using CinemaBookingSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CinemaBookingSystem.Controllers
{
    [Authorize]
    public class BookingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BookingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Bookings/Create
        [HttpGet]
        public async Task<IActionResult> Create(int? showtimeId)
        {
            await LoadShowtimes(showtimeId);

            var booking = new Booking();

            if (showtimeId.HasValue)
            {
                var showtime = await _context.Showtimes
                    .FirstOrDefaultAsync(s => s.Id == showtimeId.Value);

                if (showtime == null)
                    return NotFound();

                if (showtime.StartTime <= DateTime.Now)
                {
                    TempData["Error"] = "This showtime has already passed.";
                    return RedirectToAction("Index", "Movies");
                }

                if (showtime.AvailableSeats < 1)
                {
                    TempData["Error"] = "This show is fully booked.";
                    return RedirectToAction("Index", "Movies");
                }

                booking.ShowtimeId = showtime.Id;
            }

            return View(booking);
        }

        // POST: Bookings/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Booking booking)
        {
            ModelState.Remove(nameof(Booking.TotalPrice));
            ModelState.Remove(nameof(Booking.Status));
            ModelState.Remove(nameof(Booking.BookingDate));
            ModelState.Remove(nameof(Booking.Showtime));
            ModelState.Remove(nameof(Booking.User));
            ModelState.Remove(nameof(Booking.UserId));

            var showtime = await _context.Showtimes
                .Include(s => s.Movie)
                .FirstOrDefaultAsync(s => s.Id == booking.ShowtimeId);

            if (showtime == null)
            {
                ModelState.AddModelError(
                    nameof(Booking.ShowtimeId),
                    "Please select a valid showtime.");
            }
            else
            {
                if (showtime.Movie == null)
                {
                    ModelState.AddModelError(
                        nameof(Booking.ShowtimeId),
                        "The movie for this showtime was not found.");
                }

                if (showtime.StartTime <= DateTime.Now)
                {
                    ModelState.AddModelError(
                        nameof(Booking.ShowtimeId),
                        "Please select a future showtime.");
                }

                if (booking.NumberOfTickets < 1 ||
                    booking.NumberOfTickets > 10)
                {
                    ModelState.AddModelError(
                        nameof(Booking.NumberOfTickets),
                        "Tickets must be between 1 and 10.");
                }

                if (showtime.AvailableSeats < 1)
                {
                    ModelState.AddModelError(
                        nameof(Booking.ShowtimeId),
                        "This show is fully booked.");
                }
                else if (booking.NumberOfTickets > showtime.AvailableSeats)
                {
                    ModelState.AddModelError(
                        nameof(Booking.NumberOfTickets),
                        $"Only {showtime.AvailableSeats} seats are available.");
                }
            }

            if (!ModelState.IsValid)
            {
                await LoadShowtimes(booking.ShowtimeId);
                return View(booking);
            }

            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Challenge();

            booking.UserId = userId;
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

                return RedirectToAction(
                    nameof(Confirmation),
                    new { id = booking.Id });
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();

                ModelState.AddModelError(
                    string.Empty,
                    "The booking could not be saved. Please try again.");

                await LoadShowtimes(booking.ShowtimeId);
                return View(booking);
            }
        }

        // GET: Bookings/Confirmation/5
        [HttpGet]
        public async Task<IActionResult> Confirmation(int id)
        {
            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Challenge();

            var booking = await _context.Bookings
                .AsNoTracking()
                .Include(b => b.Showtime)
                .ThenInclude(s => s!.Movie)
                .FirstOrDefaultAsync(
                    b => b.Id == id && b.UserId == userId);

            if (booking == null)
                return NotFound();

            return View(booking);
        }

        // GET: Bookings/MyBookings
        [HttpGet]
        public async Task<IActionResult> MyBookings()
        {
            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Challenge();

            var bookings = await _context.Bookings
                .AsNoTracking()
                .Include(b => b.Showtime)
                .ThenInclude(s => s!.Movie)
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();

            return View(bookings);
        }

        // Load available showtimes into the dropdown
        private async Task LoadShowtimes(int? selectedId = null)
        {
            var now = DateTime.Now;

            var showtimes = await _context.Showtimes
                .AsNoTracking()
                .Include(s => s.Movie)
                .Where(s =>
                    s.AvailableSeats > 0 &&
                    s.StartTime > now &&
                    s.Movie != null)
                .OrderBy(s => s.StartTime)
                .ToListAsync();

            ViewBag.Showtimes = showtimes.Select(s => new SelectListItem
            {
                Value = s.Id.ToString(),
                Text = $"{s.Movie!.Name} — " +
                       $"{s.StartTime:dd MMM yyyy, hh:mm tt} — " +
                       $"Seats: {s.AvailableSeats}",
                Selected = s.Id == selectedId
            }).ToList();
        }
    }
}
