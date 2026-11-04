using System.ComponentModel.DataAnnotations;

namespace FastFoodMVC.Models;

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public Order? Order { get; set; }
    public int FoodItemId { get; set; }
    public FoodItem? FoodItem { get; set; }
    [Range(1, 99)]
    public int Quantity { get; set; }
    [Range(0.01, 1000)]
    public decimal UnitPrice { get; set; }
}
