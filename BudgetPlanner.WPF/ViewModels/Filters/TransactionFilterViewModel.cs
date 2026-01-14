using System;
using System.Collections.Generic;
using System.Linq;
using BudgetPlanner.DAL.Models;
using BudgetPlanner.WPF.ViewModels.Base;
using BudgetPlanner.WPF.ViewModels.Items;

namespace BudgetPlanner.WPF.ViewModels.Filters
{
    public class TransactionFilterViewModel : ViewModelBase
    {
        // ====================================
        // Filter-egenskaper för form
        // ====================================
        private DateTime? _filterDate;
        public DateTime? FilterDate
        {
            get => _filterDate;
            set { _filterDate = value; RaisePropertyChanged(); }
        }

        private string _filterDescription = string.Empty;
        public string FilterDescription
        {
            get => _filterDescription;
            set { _filterDescription = value; RaisePropertyChanged(); }
        }

        private decimal? _filterAmount;
        public decimal? FilterAmount
        {
            get => _filterAmount;
            set { _filterAmount = value; RaisePropertyChanged(); }
        }

        private Category? _filterCategory;
        public Category? FilterCategory
        {
            get => _filterCategory;
            set { _filterCategory = value; RaisePropertyChanged(); }
        }

        private Recurrence? _filterRecurrence;
        public Recurrence? FilterRecurrence
        {
            get => _filterRecurrence;
            set { _filterRecurrence = value; RaisePropertyChanged(); }
        }

        // ====================================
        // Vilka filter som är aktiverade
        // ====================================
        private bool _filterByDate;
        public bool FilterByDate
        {
            get => _filterByDate;
            set { _filterByDate = value; RaisePropertyChanged(); }
        }

        private bool _filterByDescription;
        public bool FilterByDescription
        {
            get => _filterByDescription;
            set { _filterByDescription = value; RaisePropertyChanged(); }
        }

        private bool _filterByAmount;
        public bool FilterByAmount
        {
            get => _filterByAmount;
            set { _filterByAmount = value; RaisePropertyChanged(); }
        }

        private bool _filterByCategory;
        public bool FilterByCategory
        {
            get => _filterByCategory;
            set { _filterByCategory = value; RaisePropertyChanged(); }
        }

        private bool _filterByRecurrence;
        public bool FilterByRecurrence
        {
            get => _filterByRecurrence;
            set { _filterByRecurrence = value; RaisePropertyChanged(); }
        }

        // ====================================
        // Övriga filter
        // ====================================
        private bool _showMonthly = true;
        public bool ShowMonthly { get => _showMonthly; set { _showMonthly = value; RaisePropertyChanged(); } }

        private bool _showOneTime = true;
        public bool ShowOneTime { get => _showOneTime; set { _showOneTime = value; RaisePropertyChanged(); } }

        private bool _showYearly = true;
        public bool ShowYearly { get => _showYearly; set { _showYearly = value; RaisePropertyChanged(); } }

        private bool _showIncome = true;
        public bool ShowIncome { get => _showIncome; set { _showIncome = value; RaisePropertyChanged(); } }

        private bool _showExpense = true;
        public bool ShowExpense { get => _showExpense; set { _showExpense = value; RaisePropertyChanged(); } }

        public Category? SelectedCategory { get; set; }

        // ====================================
        // Matcher för filter
        // ====================================
        public bool Matches(BudgetTransactionItemViewModel vm)
        {
            bool recurrenceFilter =
                (!FilterByRecurrence || vm.Recurrence == FilterRecurrence) &&
                ((ShowOneTime && vm.Recurrence == Recurrence.OneTime) ||
                 (ShowMonthly && vm.Recurrence == Recurrence.Monthly) ||
                 (ShowYearly && vm.Recurrence == Recurrence.Yearly));

            bool categoryFilter = !FilterByCategory || (vm.Category?.Id == FilterCategory?.Id);

            bool typeFilter = (ShowIncome && vm.Type == TransactionType.Income) ||
                              (ShowExpense && vm.Type == TransactionType.Expense);

            bool dateFilter = !FilterByDate || vm.Date.Date == FilterDate?.Date;
            bool descriptionFilter = !FilterByDescription || vm.Description.Contains(FilterDescription, StringComparison.InvariantCultureIgnoreCase);
            bool amountFilter = !FilterByAmount || vm.Amount == FilterAmount;

            return recurrenceFilter && categoryFilter && typeFilter && dateFilter && descriptionFilter && amountFilter;
        }

        // ====================================
        // Summeringar
        // ====================================
        public decimal TotalIncome(IEnumerable<BudgetTransactionItemViewModel> items) =>
            items.Where(Matches).Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);

        public decimal TotalExpense(IEnumerable<BudgetTransactionItemViewModel> items) =>
            items.Where(Matches).Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);

        public decimal Result(IEnumerable<BudgetTransactionItemViewModel> items) =>
            TotalIncome(items) - TotalExpense(items);

        public decimal MonthlyForecast(IEnumerable<BudgetTransactionItemViewModel> items) =>
            Result(items) / 12m;
    }
}
