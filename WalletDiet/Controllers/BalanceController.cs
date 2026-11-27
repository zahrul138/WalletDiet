using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WalletDiet.Data;
using WalletDiet.Models;
using WalletDiet.ViewModels;

namespace WalletDiet.Controllers
{
    public class BalanceController : Controller
    {
        private readonly AppDbContext _context;
        public BalanceController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Management()
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

            var viewModel = new BalanceTransactionViewModel();

            var totalAdded = await _context.BalanceTransactions
                .Where(t => t.BudgetId == budget.Id &&
                            t.Type == BalanceTransactionType.Add)
                .SumAsync(t => (decimal?)t.Amount) ?? 0;

            var totalRemoved = await _context.BalanceTransactions
                .Where(t => t.BudgetId == budget.Id &&
                            t.Type == BalanceTransactionType.Remove)
                .SumAsync(t => (decimal?)t.Amount) ?? 0;
            var totalExpense = await _context.Expenses
                .Where(e => e.BudgetId == budget.Id)
                .SumAsync(e => (decimal?)e.Amount) ?? 0;

            viewModel.CurrentBalance = budget.InitialBalance
                + totalAdded
                - totalRemoved
                - totalExpense;

            viewModel.Transactions = await _context.BalanceTransactions
                .Where(t => t.BudgetId == budget.Id)
                .OrderByDescending(t => t.CreatedAt)
                .Take(5)
                .ToListAsync();

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(BalanceTransactionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Management));
            }

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

            var transaction = new BalanceTransaction()
            {
                BudgetId = budget.Id,
                Type = BalanceTransactionType.Add,
                Amount = model.Amount,
                Description = model.Description,
            };

            _context.BalanceTransactions.Add(transaction);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Balance added successfully!";
            return RedirectToAction("Management", "Balance");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(BalanceTransactionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Management));
            }

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

            var totalAdded = await _context.BalanceTransactions
                .Where(t => t.BudgetId == budget.Id &&
                            t.Type == BalanceTransactionType.Add)
                .SumAsync(t => (decimal?)t.Amount) ?? 0;

            var totalRemoved = await _context.BalanceTransactions
                .Where(t => t.BudgetId == budget.Id &&
                            t.Type == BalanceTransactionType.Remove)
                .SumAsync(t => (decimal?)t.Amount) ?? 0;

            var totalExpense = await _context.Expenses
                .Where(e => e.BudgetId == budget.Id)
                .SumAsync(e => (decimal?)e.Amount) ?? 0;

            var currentBalance = budget.InitialBalance
                + totalAdded
                - totalRemoved
                - totalExpense;

            if (currentBalance <= 0 || model.Amount > currentBalance)
            {
                TempData["ErrorMessage"] = "Insufficient balance!";
                return RedirectToAction(nameof(Management));
            }

            var transaction = new BalanceTransaction()
            {
                BudgetId = budget.Id,
                Type = BalanceTransactionType.Remove,
                Amount = model.Amount,
                Description = model.Description
            };

            _context.BalanceTransactions.Add(transaction);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Balance removed successfully!";
            return RedirectToAction("Management", "Balance");
        }
    }
}
