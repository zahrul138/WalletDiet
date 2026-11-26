namespace WalletDiet.ViewModels
{
    public class EditBudgetViewModel
    {
        public DateTime StartDate { get; set; }
        public DateTime TargetDate { get; set; }
        public string AllowanceMode { get; set; } = "Recommended";
        public decimal? CustomDailyAllowance { get; set; }
        public string Status { get; set; } = "Upcoming";
    }
}
