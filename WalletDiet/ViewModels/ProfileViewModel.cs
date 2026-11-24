using System.ComponentModel.DataAnnotations;

namespace WalletDiet.ViewModels
{
    public class ProfileViewModel
    {
        [StringLength(20, MinimumLength = 5, ErrorMessage = "Username must be have 5 to 20 character.")]
        public string? Username { get; set; }

        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string? Email { get; set; }

        [StringLength(15, ErrorMessage = "Max 15 character"), MinLength(8, ErrorMessage = "Password must have at least 8 character.")]
        [RegularExpression(@"^(?=.*\d).+$", ErrorMessage = "Password must contain at least one number")]
        public string? NewPassword { get; set; }
        public string? CurrentPassword { get; set; }
        public string? ProfilePicture { get; set; }
        public string? ConfirmPassword { get; set; }
        public IFormFile? PictureFile { get; set; }
    }
}
