using System.ComponentModel.DataAnnotations;

namespace WalletDiet.ViewModels
{
    public class ResetPasswordViewModel
    {
        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Token { get; set; } = string.Empty;

        [Required]
        [StringLength(15, ErrorMessage = "Max 15 character")]
        [MinLength(5, ErrorMessage = "Password must have at least 5 characters.")]
        [RegularExpression(@"^(?=.*\d).+$", ErrorMessage = "Password must contain at least one number")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(NewPassword))]
        public string ConfirmPassword { get; set; } = string.Empty;

    }
}
