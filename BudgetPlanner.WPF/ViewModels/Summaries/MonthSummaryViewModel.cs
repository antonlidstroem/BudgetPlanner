using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Data;
using BudgetPlanner.DAL.Models;
using BudgetPlanner.WPF.ViewModels.Base;
using BudgetPlanner.WPF.ViewModels.Items;

namespace BudgetPlanner.WPF.ViewModels.Summaries
{
    public class MonthSummaryViewModel : ViewModelBase
    {
        private readonly ICollectionView _view;

        public MonthSummaryViewModel(ICollectionView view)
        {
            _view = view;
            _view.CollectionChanged += (_, __) => RaiseAll();

            foreach (TransactionItemViewModel item in _view)
                item.PropertyChanged += (_, __) => RaiseAll();
        }

        private IEnumerable<TransactionItemViewModel> Items =>
            _view.Cast<TransactionItemViewModel>();

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
