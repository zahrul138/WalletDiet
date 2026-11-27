using System.ComponentModel.DataAnnotations;

namespace WalletDiet.ViewModels
{
    public class AddExpenseViewModel
    {
        [Required(ErrorMessage = "Expense title is required.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select an expense category.")]
        public string Category { get; set; } = "Other";

        [Required(ErrorMessage = "Expense amount is required.")]
        [Range(0.01, 999999999999.99,
            ErrorMessage = "Expense amount must be greater than zero.")]
        public decimal Amount { get; set; }
    }
}
