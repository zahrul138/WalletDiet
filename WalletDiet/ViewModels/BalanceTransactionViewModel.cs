using System.ComponentModel.DataAnnotations;
using WalletDiet.Models;

namespace WalletDiet.ViewModels
{
    public class BalanceTransactionViewModel
    {
        public decimal CurrentBalance { get; set; }

        [Range(typeof(decimal), "0.01", "999999999999.99",
           ErrorMessage = "Amount must be greater than zero.",
           ParseLimitsInInvariantCulture = true)]
        public decimal Amount { get; set; }

        [StringLength(200, ErrorMessage = "Description cannot exceed 200 characters.")]
        public string? Description { get; set; }
        public List<BalanceTransaction> Transactions { get; set; } = new();
    }
}
