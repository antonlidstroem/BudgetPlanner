using System.Collections.Generic;
using System.Linq;
using BudgetPlanner.DAL.Models;
using BudgetPlanner.WPF.ViewModels.Base;
using BudgetPlanner.WPF.ViewModels.Items;

public class MonthlyForecastViewModel : ViewModelBase
{
    private readonly IEnumerable<TransactionItemViewModel> _items;

    public MonthlyForecastViewModel(IEnumerable<TransactionItemViewModel> items)
    {
        _items = items;
    }

    public decimal AnnualIncome { get; set; }
    public decimal AnnualWorkHours { get; set; }

    public decimal HourlyRate => AnnualWorkHours == 0 ? 0 : AnnualIncome / AnnualWorkHours;
    public decimal MonthlyIncome => AnnualIncome / 12m;

    public decimal RecurringIncome => _items
        .Where(t => t.IsActive && t.Recurrence != Recurrence.OneTime && t.Type == TransactionType.Income)
        .Sum(t => t.NetAmount);

    public decimal RecurringExpense => _items
        .Where(t => t.IsActive && t.Recurrence != Recurrence.OneTime && t.Type == TransactionType.Expense)
        .Sum(t => t.NetAmount);

    public decimal ForecastResult => MonthlyIncome + RecurringIncome - RecurringExpense;

    public decimal MonthlyForecast => ForecastResult; // alias för XAML

    public void Refresh()
    {
        RaisePropertyChanged(nameof(AnnualIncome));
        RaisePropertyChanged(nameof(AnnualWorkHours));
        RaisePropertyChanged(nameof(HourlyRate));
        RaisePropertyChanged(nameof(MonthlyIncome));
        RaisePropertyChanged(nameof(RecurringIncome));
        RaisePropertyChanged(nameof(RecurringExpense));
        RaisePropertyChanged(nameof(ForecastResult));
        RaisePropertyChanged(nameof(MonthlyForecast));
    }
}
