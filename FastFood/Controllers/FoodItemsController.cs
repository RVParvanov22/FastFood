using FastFood.Data;
using FastFoodMVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FastFood.Controllers
{
    [Authorize(Roles = "Admin")]
    public class FoodItemsController : Controller
    {
        private readonly ApplicationDbContext _context;
        
        public FoodItemsController(ApplicationDbContext context)
        {
            _context = context;
        }
        
        public async Task<IActionResult> Index(string? search, int? categoryId, string? status, string? sort = "name", int page = 1)
        {
            const int pageSize = 10;
            var query = _context.FoodItems.AsNoTracking().AsQueryable();
            if (!string.IsNullOrWhiteSpace(search)) query = query.Where(f => f.Name.Contains(search) || f.Description.Contains(search));
            if (categoryId.HasValue) query = query.Where(f => f.FoodTypeId == categoryId.Value);
            if (status == "available") query = query.Where(f => f.IsAvailable);
            if (status == "unavailable") query = query.Where(f => !f.IsAvailable);
            query = sort switch { "price" => query.OrderBy(f => f.Price), "price_desc" => query.OrderByDescending(f => f.Price), "date" => query.OrderByDescending(f => f.Id), "name_desc" => query.OrderByDescending(f => f.Name), _ => query.OrderBy(f => f.Name) };
            ViewBag.Search = search; ViewBag.CategoryId = categoryId; ViewBag.Status = status; ViewBag.Sort = sort;
            ViewBag.Page = page; ViewBag.TotalPages = (int)Math.Ceiling(await query.CountAsync() / (double)pageSize);
            return View(await query.Skip((Math.Max(1, page) - 1) * pageSize).Take(pageSize).ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var foodItem = await _context.FoodItems
                .FirstOrDefaultAsync(f => f.Id == id);
            if (foodItem == null)
            {
                return NotFound();
            }
            return View(foodItem);

        }

        private async Task LoadCategoriesAsync(int? selectedId = null)
        {
            ViewBag.FoodTypeId = new SelectList(await _context.FoodCategories.AsNoTracking().OrderBy(c => c.Name).ToListAsync(), "Id", "Name", selectedId);
        }

        public async Task<IActionResult> Create()
        {
            await LoadCategoriesAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FoodItem foodItem)
        {
            if (!await _context.FoodCategories.AnyAsync(c => c.Id == foodItem.FoodTypeId))
                ModelState.AddModelError(nameof(foodItem.FoodTypeId), "Изберете валидна категория.");

            if (ModelState.IsValid)
            {
                _context.Add(foodItem);
                await _context.SaveChangesAsync();
                
                return RedirectToAction(nameof(Index));
            }
            await LoadCategoriesAsync(foodItem.FoodTypeId);
            return View(foodItem);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            
            var foodItem = await _context.FoodItems.FindAsync(id);

            if (foodItem == null)
            {
                return NotFound();
            }
            await LoadCategoriesAsync(foodItem.FoodTypeId);
            return View(foodItem);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, FoodItem foodItem)
        {
            if (id != foodItem.Id)
            {
                return NotFound();
            }

            if (!await _context.FoodCategories.AnyAsync(c => c.Id == foodItem.FoodTypeId))
                ModelState.AddModelError(nameof(foodItem.FoodTypeId), "Изберете валидна категория.");

            if (ModelState.IsValid)
            {
                _context.Update(foodItem);
                await _context.SaveChangesAsync();
                
                return RedirectToAction(nameof(Index));
            }
            await LoadCategoriesAsync(foodItem.FoodTypeId);
            return View(foodItem);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            
            var foodItem = await _context.FoodItems
                .FirstOrDefaultAsync(f => f.Id == id);

            if (foodItem == null)
            {
                return NotFound();
            }
            return View(foodItem);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var foodItem = await _context.FoodItems.FindAsync(id);

            if (foodItem != null)
            {
                _context.FoodItems.Remove(foodItem);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
