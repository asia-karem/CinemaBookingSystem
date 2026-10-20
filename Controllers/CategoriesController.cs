
using CinemaBookingSystem.Data;
using CinemaBookingSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CinemaBookingSystem.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Display all categories
        public async Task<IActionResult> Index()
        {
            var categories = await _context.Categories
                .OrderBy(c => c.Name)
                .ToListAsync();

            return View(categories);
        }

        // Display the add category form
        public IActionResult Create()
        {
            return View();
        }

        // Save the new category
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category)
        {
            if (!ModelState.IsValid)
            {
                return View(category);
            }

            bool exists = await _context.Categories
                .AnyAsync(c => c.Name == category.Name);

            if (exists)
            {
                ModelState.AddModelError(
                    "Name", "This category already exists.");

                return View(category);
            }

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Category added successfully!";

            return RedirectToAction(nameof(Index));
        }

        // Display the edit form
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var category = await _context.Categories.FindAsync(id);

            if (category == null)
                return NotFound();

            return View(category);
        }

        // Save category changes
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id, Category category)
        {
            if (id != category.Id)
                return NotFound();

            if (!ModelState.IsValid)
                return View(category);

            bool exists = await _context.Categories
                .AnyAsync(c => c.Name == category.Name
                    && c.Id != id);

            if (exists)
            {
                ModelState.AddModelError(
                    "Name", "This category already exists.");

                return View(category);
            }

            _context.Categories.Update(category);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Category updated successfully!";

            return RedirectToAction(nameof(Index));
        }

        // Delete category
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Movies)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
                return NotFound();

            if (category.Movies.Any())
            {
                TempData["Error"] =
                    "Cannot delete a category that has movies.";

                return RedirectToAction(nameof(Index));
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Category deleted successfully!";

            return RedirectToAction(nameof(Index));
        }
    }
}
