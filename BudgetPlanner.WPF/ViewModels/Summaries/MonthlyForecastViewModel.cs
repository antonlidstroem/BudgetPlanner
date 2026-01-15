using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using BudgetPlanner.WPF.ViewModels.Base;
using BudgetPlanner.WPF.ViewModels.Items;
using BudgetPlanner.DAL.Models;

namespace BudgetPlanner.WPF.ViewModels.Summaries
{
    public class MonthlyForecastViewModel : ViewModelBase
    {
        private readonly ICollectionView _transactionsView;

        public MonthlyForecastViewModel(ICollectionView transactionsView)
        {
            _transactionsView = transactionsView;

            if (_transactionsView is INotifyCollectionChanged cc)
            {
                cc.CollectionChanged += (_, e) =>
                {
                    if (e.NewItems != null)
                    {
                        foreach (TransactionItemViewModel t in e.NewItems)
                            t.PropertyChanged += (_, __) => RaiseAll();
                    }
                    RaiseAll();
                };
            }

            foreach (TransactionItemViewModel item in _transactionsView)
                item.PropertyChanged += (_, __) => RaiseAll();
        }

        public decimal AnnualIncome { get; set; }
        public decimal AnnualWorkHours { get; set; }

        public decimal HourlyRate => AnnualWorkHours == 0 ? 0 : AnnualIncome / AnnualWorkHours;
        public decimal MonthlyIncome => AnnualIncome / 12m;

        public decimal RecurringNet =>
            _transactionsView.Cast<TransactionItemViewModel>()
                             .Where(t => t.IsActive && t.Recurrence != Recurrence.OneTime)
                             .Sum(t => t.Type == TransactionType.Income ? t.NetAmount : -t.NetAmount);

        public decimal MonthlyForecast => MonthlyIncome + RecurringNet;

        public void RaiseAll()
        {
            RaisePropertyChanged(nameof(AnnualIncome));
            RaisePropertyChanged(nameof(AnnualWorkHours));
            RaisePropertyChanged(nameof(HourlyRate));
            RaisePropertyChanged(nameof(MonthlyIncome));
            RaisePropertyChanged(nameof(RecurringNet));
            RaisePropertyChanged(nameof(MonthlyForecast));
        }
    }
}
