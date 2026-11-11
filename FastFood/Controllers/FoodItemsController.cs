using FastFood.Data;
using FastFoodMVC.Models;
using FastFood.Services;
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
        
        private readonly FileStorageService _fileStorage;
        public FoodItemsController(ApplicationDbContext context, FileStorageService fileStorage)
        {
            _context = context;
            _fileStorage = fileStorage;
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
        public async Task<IActionResult> Create(FoodItem foodItem, IFormFile? imageFile)
        {
            if (!await _context.FoodCategories.AnyAsync(c => c.Id == foodItem.FoodTypeId))
                ModelState.AddModelError(nameof(foodItem.FoodTypeId), "Изберете валидна категория.");

            if (imageFile is not null)
            {
                var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };

                if (!allowedExtensions.Contains(extension))
                    ModelState.AddModelError("imageFile", "Позволени са JPG, PNG и WEBP файлове.");

                if (imageFile.Length == 0)
                    ModelState.AddModelError("imageFile", "Файлът е празен.");

                if (imageFile.Length > 5 * 1024 * 1024)
                    ModelState.AddModelError("imageFile", "Максималният размер е 5 MB.");
            }

            if (ModelState.IsValid)
            {
                string? savedImagePath = null;

                try
                {
                    if (imageFile is not null && imageFile.Length > 0)
                    {
                        savedImagePath = await _fileStorage.SaveFoodImageAsync(imageFile);
                        foodItem.ImagePath = savedImagePath;
                    }

                    _context.Add(foodItem);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch (InvalidDataException ex)
                {
                    ModelState.AddModelError("imageFile", ex.Message);
                }
                catch
                {
                    await _fileStorage.DeleteFoodImageAsync(savedImagePath);
                    throw;
                }
            }

            await LoadCategoriesAsync(foodItem.FoodTypeId);
            return View(foodItem);

            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot",
                        "uploads",
                        "fooditems");

                    Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);

                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(fileStream);
                    }

                    foodItem.ImagePath = "/uploads/fooditems/" + uniqueFileName;
                }

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
                var hasOrders = await _context.OrderItems.AnyAsync(item => item.FoodItemId == id);
                if (hasOrders)
                {
                    foodItem.IsAvailable = false;
                    await _context.SaveChangesAsync();
                    TempData["Message"] = "Продуктът е използван в поръчки и не може да бъде изтрит. Премахнат е от менюто, а историята на поръчките е запазена.";
                    return RedirectToAction(nameof(Index));
                }

                _context.FoodItems.Remove(foodItem);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Продуктът е изтрит.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
