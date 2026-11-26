using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WalletDiet.Data;
using WalletDiet.Models;
using WalletDiet.ViewModels;
using System;
using Microsoft.AspNetCore.Authorization;

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
        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
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
        [Authorize]
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

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdValue, out var userId))
            {
                return Unauthorized();
            }

            var budget = await _context.Budgets.FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);
            if (budget == null)
            {
                return NotFound();
            }

            if (budget.Status == "Completed")
            {
                return Forbid();
            }

            var viewModel = new EditBudgetViewModel
            {
                StartDate = budget.StartDate,
                TargetDate = budget.TargetDate,
                AllowanceMode = budget.AllowanceMode,
                CustomDailyAllowance = budget.CustomDailyAllowance,
                Status = budget.Status,
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, EditBudgetViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdValue, out var userId))
            {
                return Unauthorized();
            }

            var budget = await _context.Budgets.FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);
            if(budget == null)
            {
                return NotFound();
            }

            if (budget.Status == "Completed")
            {
                return Forbid();
            }

            var effectiveStartDate = budget.Status == "Active"
                ? budget.StartDate
                : model.StartDate;

            if (model.TargetDate <= effectiveStartDate)
            {
                ModelState.AddModelError(nameof(model.TargetDate), "Target date must be later than the start date.");
                return View(model);
            }

            if (!string.Equals(model.AllowanceMode, "Recommended", StringComparison.OrdinalIgnoreCase) &&
               !string.Equals(model.AllowanceMode, "Custom", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(nameof(model.AllowanceMode), "Please select a valid allowance mode.");
                return View(model);
            }

            if (string.Equals(model.AllowanceMode, "Custom", StringComparison.OrdinalIgnoreCase) &&
               (model.CustomDailyAllowance == null || model.CustomDailyAllowance <= 0))
            {
                ModelState.AddModelError(nameof(model.CustomDailyAllowance), "Custom daily allowance must be greater than zero.");
                return View(model);
            }

            if (budget.Status == "Upcoming")
            {
                var overlappingBudget = await _context.Budgets.FirstOrDefaultAsync(b => b.UserId == userId && b.Id != id && b.StartDate <= model.TargetDate && b.TargetDate >= model.StartDate);
                if (overlappingBudget != null)
                {
                    ModelState.AddModelError(nameof(model.StartDate), "The selected budget period overlaps with another budget.");
                    return View(model);
                }
            }
            else if (budget.Status == "Active")
            {
                var nextUpcomingBudget = await _context.Budgets
                    .Where(b => b.UserId == userId && b.Status == "Upcoming")
                    .OrderBy(b => b.StartDate)
                    .FirstOrDefaultAsync();

                if (nextUpcomingBudget != null && model.TargetDate >= nextUpcomingBudget.StartDate)
                {
                    ModelState.AddModelError(nameof(model.TargetDate),"The target date cannot overlap with the next upcoming budget.");
                    return View(model);
                }
            }

            budget.TargetDate = model.TargetDate;
            budget.AllowanceMode = model.AllowanceMode;
            budget.CustomDailyAllowance = string.Equals(model.AllowanceMode, "Custom", StringComparison.OrdinalIgnoreCase)
                                        ? model.CustomDailyAllowance
                                        : null;

            if (budget.Status == "Upcoming")
            {
                budget.StartDate = model.StartDate;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("MyBudgets", "Budget");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdValue, out var userId))
            {
                return Unauthorized();
            }

            var budget = await _context.Budgets.FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);
            if (budget == null)
            {
                return NotFound();
            }

            if (budget.Status != "Upcoming")
            {
                return Forbid();
            }

            _context.Budgets.Remove(budget);
            await _context.SaveChangesAsync();
            return RedirectToAction("MyBudgets", "Budget");
        }
    }
}
