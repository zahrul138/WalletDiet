namespace WalletDiet.ViewModels
{
    public class DailyDetailsViewModel
    {
        public DateTime Date { get; set; }
        public decimal DailyAllowance { get; set; }
        public decimal DailySpent { get; set; }
        public decimal DailyRemaining { get; set; }
        public List<ExpenseViewModel> Expenses { get; set; } = new();
    }
}
