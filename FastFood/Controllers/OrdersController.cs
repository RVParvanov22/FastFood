using System.Security.Claims;
using FastFood.Data;
using FastFoodMVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FastFood.Controllers;

[Authorize]
public class OrdersController(ApplicationDbContext context) : Controller
{
    public async Task<IActionResult> Index()
    {
        var orders = context.Orders.AsNoTracking().Include(o => o.User).Include(o => o.Items).ThenInclude(i => i.FoodItem);
        if (User.IsInRole("Admin"))
        {
            ViewBag.IsAdmin = true;
            return View(await orders.OrderByDescending(o => o.CreatedAt).ToListAsync());
        }

        if (!User.IsInRole("Client")) return Forbid();
        ViewBag.IsAdmin = false;
        return View(await orders.Where(o => o.UserId == User.FindFirstValue(ClaimTypes.NameIdentifier))
            .OrderByDescending(o => o.CreatedAt).ToListAsync());
    }

    [HttpGet]
    public async Task<IActionResult> Create() => View(await context.FoodItems.AsNoTracking().Where(f => f.IsAvailable).OrderBy(f => f.Name).ToListAsync());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int foodItemId, int quantity)
    {
        var food = await context.FoodItems.SingleOrDefaultAsync(f => f.Id == foodItemId && f.IsAvailable);
        if (food is null || quantity is < 1 or > 99)
        {
            ModelState.AddModelError(string.Empty, "Изберете наличен продукт и количество от 1 до 99.");
            return View(await context.FoodItems.AsNoTracking().Where(f => f.IsAvailable).OrderBy(f => f.Name).ToListAsync());
        }
        var order = new Order { UserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!, Items = new List<OrderItem> { new() { FoodItemId = food.Id, Quantity = quantity, UnitPrice = food.Price } } };
        context.Orders.Add(order);
        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, string status)
    {
        var allowedStatuses = new[] { "Pending", "Accepted", "Preparing", "Ready", "Completed", "Rejected" };
        if (!allowedStatuses.Contains(status)) return BadRequest("Невалиден статус.");

        var order = await context.Orders.FindAsync(id);
        if (order is null) return NotFound();

        order.Status = status;
        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
