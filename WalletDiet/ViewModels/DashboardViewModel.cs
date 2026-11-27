namespace WalletDiet.ViewModels
{
    public class DashboardViewModel
    {
        // Budget Information
        public decimal TotalBalance { get; set; }
        public decimal InitialBalance { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime TargetDate { get; set; }

        public int RemainingDays { get; set; }
        public decimal BudgetUsage { get; set; }

        // Daily Allowance
        public decimal DailyAllowance { get; set; }
        public decimal DailySpent { get; set; }
        public decimal DailyRemaining { get; set; }
        public decimal TotalExpense { get; set; }

        // Expense History
        public List<ExpenseViewModel> Expenses { get; set; } = new();


    }
}
