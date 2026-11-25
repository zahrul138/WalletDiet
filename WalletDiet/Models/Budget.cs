using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WalletDiet.Models
{
    public class Budget
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public User User { get; set; } = null!;

        [Column(TypeName = "decimal(18,2)")]
        public decimal InitialBalance { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime TargetDate { get; set; }

        public string AllowanceMode { get; set; } = "Recommended";

        [Column(TypeName = "decimal(18,2)")]
        public decimal? CustomDailyAllowance { get; set; }

        public string Status { get; set; } = "Active";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
        public ICollection<BalanceTransaction> BalanceTransactions { get; set; } = new List<BalanceTransaction>();
    }
}
