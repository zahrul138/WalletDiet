using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WalletDiet.Models
{
    public class Expense
    {
        public int Id { get; set; }

        public int BudgetId { get; set; }

        public Budget Budget { get; set; } = null!;

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Category { get; set; } = "Other";

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
