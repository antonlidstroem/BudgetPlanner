using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using BudgetPlanner.DAL.Models;
using BudgetPlanner.WPF.ViewModels.Base;
using BudgetPlanner.WPF.ViewModels.Items;

namespace BudgetPlanner.WPF.ViewModels.Filters
{
    public class TransactionSummaryViewModel : ViewModelBase
    {
        private readonly ICollectionView _view;

        public TransactionSummaryViewModel(ICollectionView view)
        {
            _view = view;
            _view.CollectionChanged += (_, __) => RaiseAll();
        }

        private IEnumerable<BudgetTransactionItemViewModel> Items =>
            _view.Cast<BudgetTransactionItemViewModel>();

        public decimal TotalIncome =>
            Items.Where(t => t.Type == TransactionType.Income)
                 .Sum(t => t.Amount);

        public decimal TotalExpense =>
            Items.Where(t => t.Type == TransactionType.Expense)
                 .Sum(t => t.Amount);

        public decimal Result => TotalIncome - TotalExpense;

        public void RaiseAll()
        {
            RaisePropertyChanged(nameof(TotalIncome));
            RaisePropertyChanged(nameof(TotalExpense));
            RaisePropertyChanged(nameof(Result));
        }
    }
}
