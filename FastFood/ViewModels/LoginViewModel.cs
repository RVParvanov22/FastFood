using System.ComponentModel.DataAnnotations;

namespace FastFoodMVC.ViewModels
{

    public class LoginViewModel
    {
    [Required(ErrorMessage = "Please enter an email.")]
    [EmailAddress]
    public string Email { get; set; }
    [Required(ErrorMessage = "Please enter a password.")]
    [DataType(DataType.Password)]
    public string Password { get; set; }
    
    public bool RememberMe { get; set; }
    
    }
}