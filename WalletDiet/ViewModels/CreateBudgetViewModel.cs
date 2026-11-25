
using System.ComponentModel.DataAnnotations;

namespace WalletDiet.ViewModels
{
    public class CreateBudgetViewModel
    {
        [Required(ErrorMessage = "Initial balance is required.")]
        [Range(0.01, 999999999999.99,
            ErrorMessage = "Initial balance must be greater than zero.")]
        public decimal InitialBalance { get; set; }

        [Required(ErrorMessage = "Start date is required.")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Target date is required.")]
        [DataType(DataType.Date)]
        public DateTime TargetDate { get; set; }

        [Required]
        public string AllowanceMode { get; set; } = "Recommended";

        [Range(0.01, 999999999999.99,
            ErrorMessage = "Custom daily allowance must be greater than zero.")]
        public decimal? CustomDailyAllowance { get; set; }
    }
}
