using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Data;
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
            _view.CollectionChanged += View_CollectionChanged;

            // Prenumerera på redan existerande items
            foreach (var item in Items)
                item.PropertyChanged += Item_PropertyChanged;
        }

        private IEnumerable<BudgetTransactionItemViewModel> Items =>
    _view.Cast<BudgetTransactionItemViewModel>()
         .Where(t => _view.Filter == null || _view.Filter(t));


        public decimal TotalIncome =>
            Items.Where(t => t.Type == TransactionType.Income)
                 .Sum(t => t.Amount);

        public decimal TotalExpense =>
            Items.Where(t => t.Type == TransactionType.Expense)
                 .Sum(t => t.Amount);

        public decimal Result => TotalIncome - TotalExpense;

        private void View_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // Ta bort gamla prenumerationer
            if (e.OldItems != null)
            {
                foreach (BudgetTransactionItemViewModel oldItem in e.OldItems)
                    oldItem.PropertyChanged -= Item_PropertyChanged;
            }

            // Lägg till nya prenumerationer
            if (e.NewItems != null)
            {
                foreach (BudgetTransactionItemViewModel newItem in e.NewItems)
                    newItem.PropertyChanged += Item_PropertyChanged;
            }

            RaiseAll();
        }

        private void Item_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // Om belopp eller typ ändras -> uppdatera summeringarna
            if (e.PropertyName == nameof(BudgetTransactionItemViewModel.Amount) ||
                e.PropertyName == nameof(BudgetTransactionItemViewModel.Type))
            {
                RaiseAll();
            }
        }

        public void SubscribeToItems()
        {
            foreach (var item in Items)
                item.PropertyChanged += Item_PropertyChanged;

            RaiseAll();
        }


        public void RaiseAll()
        {
            RaisePropertyChanged(nameof(TotalIncome));
            RaisePropertyChanged(nameof(TotalExpense));
            RaisePropertyChanged(nameof(Result));
        }
    }
}
