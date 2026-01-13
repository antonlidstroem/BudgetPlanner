using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using BudgetPlanner.DAL.Data;
using BudgetPlanner.DAL.Interfaces;
using BudgetPlanner.DAL.Models;
using BudgetPlanner.DAL.Repositories;
using BudgetPlanner.WPF.Commands;

namespace BudgetPlanner.WPF.ViewModels
{
    public class BudgetTransactionsViewModel : ViewModelBase
    {
        #region Fields and Repositories
        private readonly IBudgetTransactionRepository repository;
        private BudgetTransactionItemsViewModel? selectedTransaction;
        private bool _isInEditMode;
        private bool _isExitingEditMode = false;
        private int? _transactionMonth = null;
        private bool _showOneTime = true;
        private bool _showMonthly = true;
        private bool _showYearly = true;
        private bool _showIncome = true;
        private bool _showExpense = true;
        private Category? _selectedFilterCategory;
        #endregion

        #region Collections and Views
        public ObservableCollection<BudgetTransactionItemsViewModel> BudgetTransactions { get; set; } = new();
        public ObservableCollection<Category> Categories { get; set; } = new();
        public ICollectionView TransactionsView { get; }
        public List<int> Months { get; } = Enumerable.Range(1, 12).ToList();
        #endregion

        public bool IsGrossIncome { get; set; } = false;
        public decimal TaxRate { get; set; } = 30;
        

   

        // När AnnualHours ändras, raise PropertyChanged
        private decimal _annualHours = 1600;
        public decimal AnnualHours
        {
            get => _annualHours;
            set
            {
                if (_annualHours != value)
                {
                    _annualHours = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(HourlyRate));
                    RaisePropertyChanged(nameof(MonthlyIncomeFromHours));
                }
            }
        }



        #region Commands
        public DelegateCommand AddCommand { get; }
        public DelegateCommand DeleteCommand { get; }
        public DelegateCommand ClearCategoryFilterCommand { get; }
        public DelegateCommand UpdateCommand { get; }
        public DelegateCommand CancelEditCommand { get; }
        #endregion

        #region New Transaction Properties
        public DateTime TransactionDate { get; set; } = DateTime.Today;
        




        public Category? TransactionCategory
        {
            get => _transactionCategory;
            set
            {
                if (_transactionCategory != value)
                {
                    _transactionCategory = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(IsIncomeCategorySelected));
                    RaisePropertyChanged(nameof(MonthVisibility));
                    RaisePropertyChanged(nameof(ShowIncomeOptions));
                }
            }
        }
        private Category? _transactionCategory;

        public string TransactionDescription { get; set; } = string.Empty;
        public int? TransactionMonth
        {
            get => _transactionMonth;
            set
            {
                if (_transactionMonth != value)
                {
                    _transactionMonth = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region Filter Properties
        public bool ShowOneTime { get => _showOneTime; set { _showOneTime = value; RefreshFilteredSummaries(); } }
        public bool ShowMonthly { get => _showMonthly; set { _showMonthly = value; RefreshFilteredSummaries(); } }
        public bool ShowYearly { get => _showYearly; set { _showYearly = value; RefreshFilteredSummaries(); } }
        public bool ShowIncome { get => _showIncome; set { _showIncome = value; TransactionsView.Refresh(); RaisePropertyChanged(); } }
        public bool ShowExpense { get => _showExpense; set { _showExpense = value; TransactionsView.Refresh(); RaisePropertyChanged(); } }
        public Category? SelectedFilterCategory { get => _selectedFilterCategory; set { _selectedFilterCategory = value; RefreshFilteredSummaries(); } }
        #endregion

        #region Edit Mode Property
        public bool IsInEditMode
        {
            get => _isInEditMode;
            set
            {
                if (_isInEditMode != value)
                {
                    _isInEditMode = value;
                    RaisePropertyChanged();
                    UpdateCommand.RaiseCanExecuteChanged();
                    CancelEditCommand.RaiseCanExecuteChanged();
                    AddCommand.RaiseCanExecuteChanged();
                }
            }
        }
        #endregion

        #region Selected Transaction
        public BudgetTransactionItemsViewModel? SelectedTransaction
        {
            get => selectedTransaction;
            set
            {
                if (selectedTransaction == value) return;
                selectedTransaction = value;
                RaisePropertyChanged();
                DeleteCommand.RaiseCanExecuteChanged();
                if (value != null) EnterEditMode(value);
                else ExitEditMode();
            }
        }
        #endregion

        #region Computed Summaries
        public decimal TotalIncome => BudgetTransactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
        public decimal TotalExpense => BudgetTransactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);
        public decimal TotalResult => TotalIncome - TotalExpense;
        public decimal MonthlyForecast => BudgetTransactions
            .Where(t => t.IsActive)
            .Sum(t => t.Recurrence switch
            {
                Recurrence.Monthly => t.Amount,
                Recurrence.Yearly => t.Amount / 12,
                _ => 0
            });

        public decimal FilteredTotalIncome => TransactionsView.Cast<BudgetTransactionItemsViewModel>().Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
        public decimal FilteredTotalExpense => TransactionsView.Cast<BudgetTransactionItemsViewModel>().Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);
        public decimal FilteredTotalResult => FilteredTotalIncome - FilteredTotalExpense;
        public decimal FilteredMonthlyForecast => TransactionsView.Cast<BudgetTransactionItemsViewModel>()
            .Where(t => t.IsActive)
            .Sum(t => (t.Type == TransactionType.Income ? 1 : -1) * (t.Recurrence switch
            {
                Recurrence.Monthly => t.Amount,
                Recurrence.Yearly => t.Amount / 12,
                _ => 0
            }));
        #endregion

        #region Constructor
        public BudgetTransactionsViewModel()
        {
            var db = new BudgetDbContext();
            repository = new BudgetTransactionRepository(db);

            AddCommand = new DelegateCommand(AddTransaction, _ => !IsInEditMode);
            DeleteCommand = new DelegateCommand(DeleteTransaction, _ => SelectedTransaction != null);
            UpdateCommand = new DelegateCommand(UpdateTransaction, _ => IsInEditMode);
            CancelEditCommand = new DelegateCommand(_ => ExitEditMode(), _ => IsInEditMode);
            ClearCategoryFilterCommand = new DelegateCommand(_ => SelectedFilterCategory = null);

            BudgetTransactions.CollectionChanged += (_, __) =>
            {
                RaisePropertyChanged(nameof(TotalIncome));
                RaisePropertyChanged(nameof(TotalExpense));
                RaisePropertyChanged(nameof(TotalResult));
                RaisePropertyChanged(nameof(MonthlyForecast));
                RaisePropertyChanged(nameof(HourlyRate));
                RaisePropertyChanged(nameof(MonthlyIncomeFromHours));
            };

            TransactionsView = CollectionViewSource.GetDefaultView(BudgetTransactions);
            TransactionsView.Filter = FilterTransactions;
        }
        #endregion

        #region Methods: Load Data
        public async Task LoadCategories()
        {
            Categories.Clear();
            var cats = await repository.GetCategoriesAsync();
            foreach (var cat in cats) Categories.Add(cat);
        }

        public async Task LoadTransactionsAsync()
        {
            BudgetTransactions.Clear();
            var transactions = await repository.GetAllAsync();
            foreach (var t in transactions) BudgetTransactions.Add(new BudgetTransactionItemsViewModel(t));
            RefreshFilteredSummaries();
        }
        #endregion

        #region Methods: Transaction Commands
        private async void AddTransaction(object? parameter)
        {
            if (TransactionCategory == null || TransactionAmount <= 0) return;

            // Beräkna belopp att spara (NETTO)
            decimal grossAmount = TransactionAmount;
            decimal netAmount = TransactionAmount;

            if (TransactionCategory.Name == "Lön" && IsGrossIncome)
            {
                netAmount = TransactionAmount * (1 - TaxRate / 100m);
            }

            var transaction = new BudgetTransaction
            {
                Date = TransactionDate,
                Amount = netAmount,
                GrossAmount = grossAmount,
                CategoryId = TransactionCategory.Id,
                Recurrence = TransactionRecurrence,
                Description = TransactionDescription,
                Month = TransactionRecurrence == Recurrence.Yearly ? TransactionMonth : null,
                IsActive = true
            };

            await repository.AddAsync(transaction);
            BudgetTransactions.Add(new BudgetTransactionItemsViewModel(transaction));
            RefreshFilteredSummaries();
            ClearTransactionForm();
        }

        private async void UpdateTransaction(object? parameter)
        {
            if (SelectedTransaction == null) return;

            // Beräkna belopp att spara (NETTO)
            decimal amountToSave = TransactionAmount;
            if (TransactionCategory?.Name == "Lön" && IsGrossIncome)
                amountToSave = TransactionAmount * (1 - TaxRate / 100m);

            var model = SelectedTransaction.Model;
            model.Date = TransactionDate;
            model.Amount = amountToSave; // sparar NETTO
            model.GrossAmount = TransactionAmount; // sparar BRUTTO
            model.CategoryId = TransactionCategory!.Id;
            model.Recurrence = TransactionRecurrence;
            model.Description = TransactionDescription;
            model.Month = TransactionRecurrence == Recurrence.Yearly ? TransactionMonth : null;

            await repository.UpdateAsync(model);
            SelectedTransaction.RefreshFromModel();
            ExitEditMode();
        }


        private async void DeleteTransaction(object? parameter)
        {
            if (SelectedTransaction == null) return;
            await repository.DeleteAsync(SelectedTransaction.Model);
            BudgetTransactions.Remove(SelectedTransaction);
            SelectedTransaction = null;
            RefreshFilteredSummaries();
        }
        #endregion

        #region Methods: Edit Mode
        private void EnterEditMode(BudgetTransactionItemsViewModel vm)
        {
            IsInEditMode = true;
            TransactionDate = vm.Date;
            TransactionAmount = vm.Amount;
            TransactionCategory = vm.Category;
            TransactionRecurrence = vm.Recurrence;
            TransactionDescription = vm.Description;
            TransactionMonth = vm.Month;
            RaiseTransactionFormPropertiesChanged();
        }

        private void ExitEditMode()
        {
            if (_isExitingEditMode) return;
            _isExitingEditMode = true;

            IsInEditMode = false;
            SelectedTransaction = null;
            ClearTransactionForm();
            RefreshFilteredSummaries();

            _isExitingEditMode = false;
        }

        private void ClearTransactionForm()
        {
            TransactionDate = DateTime.Today;
            TransactionAmount = 0;
            TransactionCategory = null;
            TransactionRecurrence = Recurrence.OneTime;
            TransactionDescription = string.Empty;
            TransactionMonth = null;
            RaiseTransactionFormPropertiesChanged();
        }

        private void RaiseTransactionFormPropertiesChanged()
        {
            RaisePropertyChanged(nameof(TransactionDate));
            RaisePropertyChanged(nameof(TransactionAmount));
            RaisePropertyChanged(nameof(TransactionCategory));
            RaisePropertyChanged(nameof(TransactionRecurrence));
            RaisePropertyChanged(nameof(TransactionDescription));
            RaisePropertyChanged(nameof(TransactionMonth));
        }
        #endregion

        #region Methods: Filter & Summaries
        private bool FilterTransactions(object obj)
        {
            if (obj is not BudgetTransactionItemsViewModel vm) return false;

            bool recurrenceMatches =
                (ShowOneTime && vm.Recurrence == Recurrence.OneTime) ||
                (ShowMonthly && vm.Recurrence == Recurrence.Monthly) ||
                (ShowYearly && vm.Recurrence == Recurrence.Yearly);

            bool categoryMatches = SelectedFilterCategory == null || vm.Category?.Id == SelectedFilterCategory.Id;
            bool typeMatches = (ShowIncome && vm.Type == TransactionType.Income) || (ShowExpense && vm.Type == TransactionType.Expense);

            return recurrenceMatches && categoryMatches && typeMatches;
        }

        private void RefreshFilteredSummaries()
        {
            TransactionsView.Refresh();
            RaisePropertyChanged(nameof(FilteredTotalIncome));
            RaisePropertyChanged(nameof(FilteredTotalExpense));
            RaisePropertyChanged(nameof(FilteredTotalResult));
            RaisePropertyChanged(nameof(FilteredMonthlyForecast));
        }
        #endregion

        #region UI Helpers
        public Visibility MonthVisibility => TransactionCategory?.Name == "Lön" ? Visibility.Collapsed :
            TransactionRecurrence == Recurrence.Yearly ? Visibility.Visible : Visibility.Collapsed;

        public Recurrence TransactionRecurrence
        {
            get => _transactionRecurrence;
            set
            {
                if (_transactionRecurrence != value)
                {
                    _transactionRecurrence = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(MonthVisibility));
                    if (_transactionRecurrence != Recurrence.Yearly) TransactionMonth = null;
                }
            }
        }
        private Recurrence _transactionRecurrence = Recurrence.OneTime;

        public bool IsIncomeCategorySelected => TransactionCategory?.Name == "Lön";
        public bool ShowIncomeOptions => TransactionCategory?.Type == TransactionType.Income;
        #endregion

        public decimal MonthlyIncomeFromAnnualSalary
        {
            get
            {
                // Nettobelopp efter skatt
                var grossIncome = BudgetTransactions
                    .Where(t => t.Type == TransactionType.Income && t.Category?.Name == "Lön")
                    .Sum(t => t.Amount);

                return grossIncome / 12;
            }
        }


        public decimal HourlyRate => AnnualHours > 0 && TransactionAmount > 0
    ? TransactionAmount / AnnualHours
    : 0;

        public decimal MonthlyIncomeFromHours => HourlyRate * (AnnualHours / 12m);



        private decimal _transactionAmount;
        public decimal TransactionAmount
        {
            get => _transactionAmount;
            set
            {
                if (_transactionAmount != value)
                {
                    _transactionAmount = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(HourlyRate));
                    RaisePropertyChanged(nameof(MonthlyIncomeFromHours));
                }
            }
        }


    }


}
