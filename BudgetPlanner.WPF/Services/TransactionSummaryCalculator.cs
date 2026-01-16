using System.Collections.Generic;
using System.Linq;
using BudgetPlanner.DAL.Models;
using BudgetPlanner.WPF.ViewModels.Items;

namespace BudgetPlanner.WPF.Services
{
    public static class TransactionSummaryCalculator
    {
        public static SummaryResult Calculate(IEnumerable<TransactionItemViewModel> items)
        {
            var income = items
                .Where(t => t.Type == TransactionType.Income)
                .Sum(t => t.NetAmount);

            var expense = items
                .Where(t => t.Type == TransactionType.Expense)
                .Sum(t => t.NetAmount);

            return new SummaryResult(income, expense);
        }
    }

    public record SummaryResult(decimal Income, decimal Expense)
    {
        public decimal Result => Income - Expense;
    }
}
