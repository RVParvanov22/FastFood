using Microsoft.AspNetCore.Identity;

namespace FastFoodMVC.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? ProfileImagePath { get; set; }
    }
}
