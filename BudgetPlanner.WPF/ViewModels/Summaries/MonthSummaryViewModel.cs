using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using BudgetPlanner.WPF.ViewModels.Base;
using BudgetPlanner.WPF.ViewModels.Items;
using BudgetPlanner.DAL.Models;

namespace BudgetPlanner.WPF.ViewModels.Summaries
{
    public class MonthSummaryViewModel : ViewModelBase
    {
        private readonly ICollectionView _view;
        private readonly int _month;
        private readonly int _year;

        public MonthSummaryViewModel(ICollectionView view, int month, int year)
        {
            _view = view;
            _month = month;
            _year = year;

            if (_view is INotifyCollectionChanged cc)
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

            foreach (TransactionItemViewModel item in _view)
                item.PropertyChanged += (_, __) => RaiseAll();
        }

        private IEnumerable<TransactionItemViewModel> Items =>
            _view.Cast<TransactionItemViewModel>()
                 .Where(t => t.StartDate.Month == _month && t.StartDate.Year == _year);

        public decimal TotalIncome =>
            Items.Where(t => t.Type == TransactionType.Income)
                 .Sum(t => t.NetAmount);

        public decimal TotalExpense =>
            Items.Where(t => t.Type == TransactionType.Expense)
                 .Sum(t => t.NetAmount);

        public decimal Result => TotalIncome - TotalExpense;

        public void RaiseAll()
        {
            RaisePropertyChanged(nameof(TotalIncome));
            RaisePropertyChanged(nameof(TotalExpense));
            RaisePropertyChanged(nameof(Result));
        }
    }
}
