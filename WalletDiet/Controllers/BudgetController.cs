using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WalletDiet.Data;
using WalletDiet.Models;
using WalletDiet.ViewModels;
using System;

namespace WalletDiet.Controllers
{
    public class BudgetController : Controller
    {
/*-------------------------- Dependency Injection (DI) --------------------------------------*/
        private readonly AppDbContext _context;

        public BudgetController(AppDbContext context)
        {
            _context = context;
        }

/*-------------------------- Method Create Budget ------------------------------------------*/
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateBudgetViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (!string.Equals(model.AllowanceMode, "Recommended", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(model.AllowanceMode, "Custom", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(nameof(model.AllowanceMode),"Please select a valid allowance mode.");
                return View(model);
            }

            if (string.Equals(model.AllowanceMode, "Custom", StringComparison.OrdinalIgnoreCase) && 
                (model.CustomDailyAllowance == null || model.CustomDailyAllowance <= 0))
            {
                ModelState.AddModelError(nameof(model.CustomDailyAllowance), "Custom daily allowance must be greater than zero.");
                return View(model);
            }

            if (model.TargetDate <= model.StartDate)
            {
                ModelState.AddModelError(nameof(model.TargetDate), "Target date must be later than the start date.");
                return View(model);
            }

            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdValue, out var userId))
            {
                return Unauthorized();
            }

            var latestBudget = await _context.Budgets
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.StartDate)
                .FirstOrDefaultAsync();

            if (latestBudget != null && model.StartDate <= latestBudget.TargetDate)
            {
                ModelState.AddModelError(nameof(model.StartDate), "The start date must be later than the previous budget's end date.");
                return View(model);
            }

            var today = DateTime.Today;

            string budgetStatus;

            if (model.TargetDate.Date < today)
            {
                budgetStatus = "Completed";
            }
            else if (model.StartDate.Date > today)
            {
                budgetStatus = "Upcoming";
            }
            else
            {
                budgetStatus = "Active";
            }

            var budget = new Budget
            {
                UserId = userId,
                InitialBalance = model.InitialBalance,
                StartDate = model.StartDate,
                TargetDate = model.TargetDate,
                AllowanceMode = model.AllowanceMode,
                CustomDailyAllowance = string.Equals(model.AllowanceMode, "Custom", StringComparison.OrdinalIgnoreCase) 
                                        ? model.CustomDailyAllowance 
                                        : null,
                Status = budgetStatus,
            };

            _context.Budgets.Add(budget);
            await _context.SaveChangesAsync();
            return RedirectToAction("MyBudgets", "Budget");
        }

/*-------------------------- Method Manage Budget ------------------------------------------*/
        [HttpGet]
        public async Task<IActionResult> MyBudgets()
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdValue, out var userId))
            {
                return Unauthorized();
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var budget = await _context.Budgets
                .Where(b => b.UserId == userId)
                .ToListAsync();

            return View(budget);
        }
    }
}
