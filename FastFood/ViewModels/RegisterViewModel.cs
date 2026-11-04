using System.ComponentModel.DataAnnotations;

namespace FastFoodMVC.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Please enter an email.")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Please enter a password.")]
        [StringLength(100,MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        
        [Required(ErrorMessage = "Please enter a confirm password.")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }

    }
}