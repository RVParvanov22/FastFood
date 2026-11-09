using FastFood.Data;
using FastFoodMVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FastFood.Controllers.Api;

[ApiController, Route("api/fooditems"), Authorize(Roles = "Admin")]
public class FoodItemsApiController(ApplicationDbContext context) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<FoodItem>>> Get()
    {
        return Ok( await context.FoodItems.AsNoTracking().ToListAsync());
    } //shte vurne task !!!
    
    [HttpGet("{id:int}")]
    public async Task<ActionResult<FoodItem>> 
        Get(int id) => await context.FoodItems.FindAsync(id) is { } item ? item : NotFound();
    [HttpPost] 
    public async Task<ActionResult<FoodItem>> Post(FoodItem item) 
    { 
        context.FoodItems.Add(item); 
        await context.SaveChangesAsync(); 
        return CreatedAtAction(nameof(Get), new { id = item.Id }, item); 
    }
    [HttpPut("{id:int}")] 
    public async Task<IActionResult> 
        Put(int id, FoodItem item) { if (id != item.Id)
        return BadRequest(); if (!await context.FoodItems.AnyAsync(f => f.Id == id))
        return NotFound(); context.Entry(item).State = EntityState.Modified; 
        await context.SaveChangesAsync(); return NoContent(); }
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> 
        Delete(int id) { var item = await context.FoodItems.FindAsync(id);
        if (item is null) return NotFound(); 
        context.FoodItems.Remove(item); 
        await context.SaveChangesAsync(); return NoContent(); }
}
