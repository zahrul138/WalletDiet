using System.ComponentModel.DataAnnotations;

namespace WalletDiet.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string Email { get; set; }
        public string? Message { get; set; }
    }
}
