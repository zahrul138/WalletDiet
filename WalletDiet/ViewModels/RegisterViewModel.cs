using System.ComponentModel.DataAnnotations;

namespace WalletDiet.ViewModels
{
    public class RegisterViewModel
    {
        [Required, StringLength(20)]
        public string Username { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string Email { get; set; }

        [Required, StringLength(15, ErrorMessage = "Max 15 character"), MinLength(8, ErrorMessage = "Password must have at least 8 character.")]
        [RegularExpression(@"^(?=.*\d).+$", ErrorMessage = "Password must contain at least one number")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(Password))]
        public string ConfirmPassword { get; set; }
    }
}
