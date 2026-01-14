using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using BudgetPlanner.DAL.Models;
using BudgetPlanner.WPF.ViewModels.Base;
using BudgetPlanner.WPF.ViewModels.Items;

namespace BudgetPlanner.WPF.ViewModels.Filters
{
    public class TransactionFilterViewModel : ViewModelBase
    {


        public Category? SelectedCategory { get; set; }

        public bool Matches(BudgetTransactionItemViewModel vm)
        {
            bool recurrence =
                (ShowOneTime && vm.Recurrence == Recurrence.OneTime) ||
                (ShowMonthly && vm.Recurrence == Recurrence.Monthly) ||
                (ShowYearly && vm.Recurrence == Recurrence.Yearly);

            bool category =
                SelectedCategory == null || vm.Category?.Id == SelectedCategory.Id;

            bool type =
                (ShowIncome && vm.Type == TransactionType.Income) ||
                (ShowExpense && vm.Type == TransactionType.Expense);

            return recurrence && category && type;
        }

        private bool _showMonthly = true;
        public bool ShowMonthly
        {
            get => _showMonthly;
            set
            {
                _showMonthly = value;
                RaisePropertyChanged();
            }
        }

        private bool _showOneTime = true;
        public bool ShowOneTime
        {
            get => _showOneTime;
            set
            {
                _showOneTime = value;
                RaisePropertyChanged();
            }
        }

        private bool _showYearly = true;
        public bool ShowYearly
        {
            get => _showYearly;
            set
            {
                _showYearly = value;
                RaisePropertyChanged();
            }
        }
        private bool _showIncome = true;
        public bool ShowIncome
        {
            get => _showIncome;
            set
            {
                _showIncome = value;
                RaisePropertyChanged();
            }
        }
        private bool _showExpense = true;
        public bool ShowExpense
        {
            get => _showExpense;
            set
            {
                _showExpense = value;
                RaisePropertyChanged();
            }
        }

        public decimal TotalIncome(IEnumerable<BudgetTransactionItemViewModel> items) =>
          items.Where(Matches) 
         .Where(t => t.Type == TransactionType.Income)
         .Sum(t => t.Amount);

        public decimal TotalExpense(IEnumerable<BudgetTransactionItemViewModel> items) =>
            items.Where(Matches)
                 .Where(t => t.Type == TransactionType.Expense)
                 .Sum(t => t.Amount);

        public decimal Result(IEnumerable<BudgetTransactionItemViewModel> items) =>
            TotalIncome(items) - TotalExpense(items);

        public decimal MonthlyForecast(IEnumerable<BudgetTransactionItemViewModel> items) =>
            Result(items) / 12m; // Förenklad månadsprognos



    }

}
