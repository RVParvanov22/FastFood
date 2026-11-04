using FastFood.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FastFood.Controllers;

public class MenuController(ApplicationDbContext context) : Controller
{
    public async Task<IActionResult> Index(string? search, int? categoryId, string? status, string? sort = "name", int page = 1)
    {
        const int pageSize = 10;
        var query = context.FoodItems.AsNoTracking().Where(f => f.IsAvailable);
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(f => f.Name.Contains(search) || f.Description.Contains(search));
        if (categoryId.HasValue) query = query.Where(f => f.FoodTypeId == categoryId.Value);
        if (status == "unavailable") query = query.Where(f => !f.IsAvailable);
        query = sort switch { "price" => query.OrderBy(f => f.Price), "price_desc" => query.OrderByDescending(f => f.Price), "date" => query.OrderByDescending(f => f.Id), "name_desc" => query.OrderByDescending(f => f.Name), _ => query.OrderBy(f => f.Name) };
        ViewBag.Categories = await context.FoodCategories.AsNoTracking().OrderBy(c => c.Name).ToListAsync();
        ViewBag.Search = search; ViewBag.CategoryId = categoryId; ViewBag.Status = status; ViewBag.Sort = sort;
        ViewBag.Page = Math.Max(1, page); ViewBag.TotalPages = Math.Max(1, (int)Math.Ceiling(await query.CountAsync() / (double)pageSize));
        return View(await query.Skip((Math.Max(1, page) - 1) * pageSize).Take(pageSize).ToListAsync());
    }
}
