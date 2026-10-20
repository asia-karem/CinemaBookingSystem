

using CinemaBookingSystem.Data;
using CinemaBookingSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CinemaBookingSystem.Controllers
{
    public class CinemasController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public CinemasController(
            ApplicationDbContext context,
            IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // Display all cinemas
        public async Task<IActionResult> Index()
        {
            var cinemas = await _context.Cinemas
                .OrderBy(c => c.Name)
                .ToListAsync();

            return View(cinemas);
        }

        // Display the Create form
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // Display the Edit form
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var cinema = await _context.Cinemas.FindAsync(id);

            if (cinema == null)
                return NotFound();

            return View(cinema);
        }

        // Save edited cinema
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Cinema cinema,
            IFormFile? imageFile)
        {
            if (id != cinema.Id)
                return NotFound();

            var existingCinema = await _context.Cinemas.FindAsync(id);

            if (existingCinema == null)
                return NotFound();

            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    if (!imageFile.ContentType.StartsWith("image/"))
                    {
                        ModelState.AddModelError(
                            "imageFile",
                            "Please select a valid image file.");

                        return View(cinema);
                    }

                    var uploadsFolder = Path.Combine(
                        _environment.WebRootPath, "images", "cinemas");

                    Directory.CreateDirectory(uploadsFolder);

                    var extension = Path.GetExtension(imageFile.FileName);
                    var fileName = $"{Guid.NewGuid()}{extension}";
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    // Delete the previous image if it is inside our uploads folder
                    if (!string.IsNullOrWhiteSpace(existingCinema.Img))
                    {
                        var oldFileName = Path.GetFileName(existingCinema.Img);
                        var oldPath = Path.Combine(uploadsFolder, oldFileName);

                        if (System.IO.File.Exists(oldPath))
                            System.IO.File.Delete(oldPath);
                    }

                    existingCinema.Img = $"/images/cinemas/{fileName}";
                }

                existingCinema.Name = cinema.Name;
                existingCinema.Address = cinema.Address;
                existingCinema.Description = cinema.Description;

                await _context.SaveChangesAsync();

                TempData["Success"] = "Cinema updated successfully!";

                return RedirectToAction(nameof(Index));
            }

            return View(cinema);
        }

        // Delete cinema
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var cinema = await _context.Cinemas
                .Include(c => c.Movies)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cinema == null)
                return NotFound();

            if (cinema.Movies.Any())
            {
                TempData["Error"] =
                    "Cannot delete this cinema because it has movies assigned to it.";

                return RedirectToAction(nameof(Index));
            }

            _context.Cinemas.Remove(cinema);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Cinema deleted successfully!";

            return RedirectToAction(nameof(Index));
        }

        // Save the new cinema
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            Cinema cinema,
            IFormFile? imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                ModelState.AddModelError(
                    "imageFile",
                    "Please select a cinema image.");
            }
            else if (!imageFile.ContentType.StartsWith("image/"))
            {
                ModelState.AddModelError(
                    "imageFile",
                    "Please select a valid image file.");
            }

            if (!ModelState.IsValid)
            {
                return View(cinema);
            }

            var uploadsFolder = Path.Combine(
                _environment.WebRootPath,
                "images",
                "cinemas");

            Directory.CreateDirectory(uploadsFolder);

            var extension = Path.GetExtension(imageFile!.FileName);
            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            cinema.Img = $"/images/cinemas/{fileName}";

            _context.Cinemas.Add(cinema);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Cinema added successfully!";

            return RedirectToAction(nameof(Index));
        }
    }
}

