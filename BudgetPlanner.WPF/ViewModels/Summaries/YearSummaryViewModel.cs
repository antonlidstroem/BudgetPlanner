using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using BudgetPlanner.DAL.Models;
using BudgetPlanner.WPF.ViewModels.Base;
using BudgetPlanner.WPF.ViewModels.Items;

namespace BudgetPlanner.WPF.ViewModels.Summaries
{
    public class YearSummaryViewModel : ViewModelBase
    {
        private readonly ObservableCollection<TransactionItemViewModel> _items;

        public YearSummaryViewModel(ObservableCollection<TransactionItemViewModel> items)
        {
            _items = items;
            _items.CollectionChanged += (_, __) => RaiseAll();

            foreach (var item in _items)
                item.PropertyChanged += (_, __) => RaiseAll();
        }

        public decimal TotalIncome =>
            _items.Where(t => t.Type == TransactionType.Income)
                  .Sum(t => t.Amount);

        public decimal TotalExpense =>
            _items.Where(t => t.Type == TransactionType.Expense)
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
