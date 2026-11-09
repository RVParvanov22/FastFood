using System.ComponentModel.DataAnnotations;

namespace FastFoodMVC.Models
{
    public class FoodItem
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Please enter a name.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 50 characters long.")]
        public string Name { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Please enter a description.")]
        [StringLength(200)]
        
        public string Description { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Please enter a price.")]
        [Range(0.1, 100, ErrorMessage = "Price must be between 0.1 and 100.")]
        
        public decimal Price { get; set; }
        
        [Required(ErrorMessage = "Please enter a food type.")]
        public int FoodTypeId { get; set; }
        
        public bool IsAvailable { get; set; }

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        
        public string? ImagePath { get; set; }
        
    }
}
