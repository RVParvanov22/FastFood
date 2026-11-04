using System.ComponentModel.DataAnnotations;

namespace FastFoodMVC.Models;

public class Order
{
    public int Id { get; set; }
    [Required]
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }
    [Required, StringLength(20)]
    public string Status { get; set; } = "Pending";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}
