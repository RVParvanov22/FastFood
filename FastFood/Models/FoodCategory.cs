using System.ComponentModel.DataAnnotations;

namespace FastFoodMVC.Models;

public class FoodCategory
{
    public int Id { get; set; }
    [Required, StringLength(50, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;
    public ICollection<FoodItem> FoodItems { get; set; } = new List<FoodItem>();
}
