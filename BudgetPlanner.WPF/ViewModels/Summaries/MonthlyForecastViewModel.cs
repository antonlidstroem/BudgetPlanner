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

        foreach (var item in items)
        {
            item.PropertyChanged += (_, __) => Refresh();
        }
       
    }

    public decimal _annualIncome { get; set; }
    public decimal AnnualIncome
    {
        get => _annualIncome;
        set
        {
            if (_annualIncome != value)
            {
                _annualIncome = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(HourlyRate));
                RaisePropertyChanged(nameof(MonthlyIncome));
                RaisePropertyChanged(nameof(MonthlyForecast));
            }
        }
    }
    private decimal _annualWorkHours;
    public decimal AnnualWorkHours
    {
        get => _annualWorkHours;
        set
        {
            if (_annualWorkHours != value)
            {
                _annualWorkHours = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(HourlyRate));
                RaisePropertyChanged(nameof(MonthlyIncome));
                RaisePropertyChanged(nameof(MonthlyForecast));
            }
        }
    }

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
