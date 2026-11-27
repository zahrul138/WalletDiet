using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WalletDiet.Data;
using WalletDiet.ViewModels;
using WalletDiet.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace WalletDiet.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdValue, out var userId))
            {
                return Unauthorized();
            }

            var budget = await _context.Budgets.FirstOrDefaultAsync(b => b.UserId == userId && b.Status == "Active");
            if (budget == null)
            {
                return View();
            }

            var expenses = await _context.Expenses
                .Where(e => e.BudgetId == budget.Id)
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();

            var balanceTransactions = await _context.BalanceTransactions
                .Where(t => t.BudgetId == budget.Id)
                .ToListAsync();

            var totalAdded = balanceTransactions
                .Where(t => t.Type == BalanceTransactionType.Add)
                .Sum(t => t.Amount);

            var totalRemoved = balanceTransactions
                .Where(t => t.Type == BalanceTransactionType.Remove)
                .Sum(t => t.Amount);

            var totalExpense = expenses.Sum(e => e.Amount);

            var totalBalance = budget.InitialBalance
                + totalAdded
                - totalRemoved
                - totalExpense;

            var budgetUsage = budget.InitialBalance > 0
                ? (totalExpense / budget.InitialBalance) * 100 
                : 0;

            var remainingDays = Math.Max(1, (budget.TargetDate.Date - DateTime.Today).Days + 1);

            var dailyAllowance = budget.AllowanceMode == "Custom"
                ? budget.CustomDailyAllowance ?? 0
                : totalBalance / remainingDays;

            var dailySpent = expenses
                .Where(e => e.CreatedAt.Date == DateTime.Today) 
                .Sum(e => e.Amount);

            var dailyRemaining = dailyAllowance - dailySpent;

            var viewModel = new DashboardViewModel
            {
                TotalBalance = totalBalance,
                TotalExpense = totalExpense,
                InitialBalance = budget.InitialBalance,
                StartDate = budget.StartDate,
                TargetDate = budget.TargetDate,
                RemainingDays = remainingDays,
                BudgetUsage = budgetUsage,
                DailyAllowance = dailyAllowance,
                DailySpent = dailySpent,
                DailyRemaining = dailyRemaining,

                Expenses = expenses.Select(e => new ExpenseViewModel
                {
                    Id = e.Id,
                    Title = e.Title,
                    Category = e.Category,
                    Amount = e.Amount,
                    CreatedAt = e.CreatedAt,
                }).ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteExpense(int id, DateTime? returnDate)
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdValue, out var userId))
            {
                return Unauthorized();
            }

            var expense = await _context.Expenses
                .Include(e => e.Budget)
                .FirstOrDefaultAsync(e =>
                    e.Id == id &&
                    e.Budget.UserId == userId &&
                    e.Budget.Status == "Active");

            if (expense == null)
            {
                return NotFound();
            }

            _context.Expenses.Remove(expense);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Expense deleted successfully!";

            if (returnDate.HasValue)
            {
                return RedirectToAction("DailyDetails", new { date = returnDate.Value });
            }

            return RedirectToAction("Dashboard");
        }

        [HttpGet]
        public async Task<IActionResult> DailyDetails(DateTime date)
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdValue, out var userId))
            {
                return Unauthorized();
            }

            var budget = await _context.Budgets.FirstOrDefaultAsync(b => b.UserId == userId && b.Status == "Active");

            if (budget == null)
            {
                return NotFound();
            }

            if (date.Date < budget.StartDate.Date || date.Date > budget.TargetDate.Date)
            {
                return NotFound();
            }

            var expenses = await _context.Expenses
                .Where(e => e.BudgetId == budget.Id && e.CreatedAt.Date == date.Date)
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();

            var dailySpent = expenses.Sum(e => e.Amount);

            var remainingDays = Math.Max(1,(budget.TargetDate.Date - date.Date).Days + 1);

            var totalExpense = await _context.Expenses
                .Where(e => e.BudgetId == budget.Id &&
                            e.CreatedAt.Date <= date.Date)
                .SumAsync(e => (decimal?)e.Amount) ?? 0;

            var balanceTransactions = await _context.BalanceTransactions
                .Where(t => t.BudgetId == budget.Id &&
                            t.CreatedAt.Date <= date.Date)
                .ToListAsync();

            var totalAdded = balanceTransactions
                .Where(t => t.Type == BalanceTransactionType.Add)
                .Sum(t => t.Amount);

            var totalRemoved = balanceTransactions
                .Where(t => t.Type == BalanceTransactionType.Remove)
                .Sum(t => t.Amount);

            var totalBalance = budget.InitialBalance
                + totalAdded
                - totalRemoved
                - totalExpense;

            var dailyAllowance = budget.AllowanceMode == "Custom"
                ? budget.CustomDailyAllowance ?? 0
                : totalBalance / remainingDays;

            var dailyRemaining = dailyAllowance - dailySpent;

            var viewModel = new DailyDetailsViewModel
            {
                Date = date,
                DailyAllowance = dailyAllowance,
                DailySpent = dailySpent,
                DailyRemaining = dailyRemaining,

                Expenses = expenses.Select(e => new ExpenseViewModel
                {
                    Id = e.Id,
                    Title = e.Title,
                    Category = e.Category,
                    Amount = e.Amount,
                    CreatedAt = e.CreatedAt
                }).ToList()
            };

            return View(viewModel);
        }

        /*-------------------------- Method Add Expense ------------------------------------------*/
        [HttpPost]
        public async Task<IActionResult> AddExpense(AddExpenseViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["TitleError"] = ModelState["Title"]?.Errors.FirstOrDefault()?.ErrorMessage;
                TempData["AmountError"] = ModelState["Amount"]?.Errors.FirstOrDefault()?.ErrorMessage;
                TempData["CategoryError"] = ModelState["Category"]?.Errors.FirstOrDefault()?.ErrorMessage;

                return RedirectToAction("Dashboard", "Dashboard");
            }

            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdValue, out var userId))
            {
                return Unauthorized();
            }

            var budget = await _context.Budgets.FirstOrDefaultAsync(b => b.UserId == userId && b.Status == "Active");
            if (budget == null) 
            {
                TempData["ErrorMessage"] = "You don't have an active budget.";
                return RedirectToAction("Dashboard", "Dashboard");
            }

            var expense = new Expense()
            {
                BudgetId = budget.Id,
                Title = model.Title,
                Category = model.Category,  
                Amount = model.Amount,
            };

            _context.Expenses.Add(expense);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Expense saved successfully!";
            return RedirectToAction("Dashboard", "Dashboard");
        }
    }
}
