using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WalletDiet.Models
{
    public class BalanceTransaction
    {
        public int Id { get; set; }

        public int BudgetId { get; set; }

        public Budget Budget { get; set; } = null!;

        public BalanceTransactionType Type { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
