using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using BudgetPlanner.DAL.Data;
using BudgetPlanner.DAL.Interfaces;
using BudgetPlanner.DAL.Models;
using BudgetPlanner.DAL.Repositories;
using BudgetPlanner.WPF.Commands;
using Microsoft.EntityFrameworkCore;

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

        #region Commands
        public DelegateCommand AddCommand { get; }
        public DelegateCommand DeleteCommand { get; }
        public DelegateCommand ClearCategoryFilterCommand { get; }
        public DelegateCommand UpdateCommand { get; }
        public DelegateCommand CancelEditCommand { get; }
        #endregion

        #region New Transaction Properties
        public DateTime TransactionDate { get; set; } = DateTime.Today;
        public decimal TransactionAmount { get; set; }
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
                    base.RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region Filter Properties
        public bool ShowOneTime
        {
            get => _showOneTime;
            set
            {
                _showOneTime = value;
                RefreshFilteredSummaries();
            }
        }

        public bool ShowMonthly
        {
            get => _showMonthly;
            set
            {
                _showMonthly = value;
                RefreshFilteredSummaries();
            }
        }

        public bool ShowYearly
        {
            get => _showYearly;
            set
            {
                _showYearly = value;
                RefreshFilteredSummaries();
            }
        }

        public bool ShowIncome
        {
            get => _showIncome;
            set
            {
                if (_showIncome != value)
                {
                    _showIncome = value;
                    TransactionsView.Refresh();
                    RaisePropertyChanged(nameof(ShowIncome));
                }
            }
        }

        public bool ShowExpense
        {
            get => _showExpense;
            set
            {
                if (_showExpense != value)
                {
                    _showExpense = value;
                    TransactionsView.Refresh();
                    RaisePropertyChanged(nameof(ShowExpense));
                }
            }
        }

        public Category? SelectedFilterCategory
        {
            get => _selectedFilterCategory;
            set
            {
                _selectedFilterCategory = value;
                RefreshFilteredSummaries();
            }
        }
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
                    base.RaisePropertyChanged();
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
                RaisePropertyChanged(nameof(SelectedTransaction));
                DeleteCommand.RaiseCanExecuteChanged();

                if (value != null)
                    EnterEditMode(value);
                else
                    ExitEditMode();
            }
        }
        #endregion

        #region Computed Summaries
        public decimal TotalIncome => BudgetTransactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
        public decimal TotalExpense => BudgetTransactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);
        public decimal TotalResult => TotalIncome - TotalExpense;
        public decimal MonthlyForecast => BudgetTransactions
            .Where(t => t.IsActive)
            .Sum(t =>
            {
                return t.Recurrence switch
                {
                    Recurrence.Monthly => t.Amount,
                    Recurrence.Yearly => t.Amount / 12,
                    _ => 0
                };
            });



        public decimal FilteredTotalIncome => TransactionsView.Cast<BudgetTransactionItemsViewModel>().Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
        public decimal FilteredTotalExpense => TransactionsView.Cast<BudgetTransactionItemsViewModel>().Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);
        public decimal FilteredTotalResult => FilteredTotalIncome - FilteredTotalExpense;
        public decimal FilteredMonthlyForecast =>
            TransactionsView.Cast<BudgetTransactionItemsViewModel>()
            .Where(t => t.IsActive)
            .Sum(t =>
            {
                int sign = t.Type == TransactionType.Income ? 1 : -1;

                return t.Recurrence switch
                {
                    Recurrence.Monthly => t.Amount * sign,
                    Recurrence.Yearly => (t.Amount / 12) * sign,
                    _ => 0
                };
            });

        #endregion

        #region Constructor
        public BudgetTransactionsViewModel()
        {
            var db = new BudgetDbContext();
            repository = new BudgetTransactionRepository(db);

            AddCommand = new DelegateCommand(AddTransaction, _ => !IsInEditMode);
            DeleteCommand = new DelegateCommand(DeleteTransaction, CanDelete);
            UpdateCommand = new DelegateCommand(UpdateTransaction, _ => IsInEditMode);
            CancelEditCommand = new DelegateCommand(_ => ExitEditMode(), _ => IsInEditMode);
            ClearCategoryFilterCommand = new DelegateCommand(_ => SelectedFilterCategory = null);

            BudgetTransactions.CollectionChanged += (_, __) =>
            {
                RaisePropertyChanged(nameof(TotalIncome));
                RaisePropertyChanged(nameof(TotalExpense));
                RaisePropertyChanged(nameof(TotalResult));
                RaisePropertyChanged(nameof(MonthlyForecast));
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
            foreach (var cat in cats)
                Categories.Add(cat);
        }

        public async Task LoadTransactionsAsync()
        {
            BudgetTransactions.Clear();
            var transactions = await repository.GetAllAsync();
            foreach (var t in transactions)
                BudgetTransactions.Add(new BudgetTransactionItemsViewModel(t));

            RefreshFilteredSummaries();
        }
        #endregion

        #region Methods: Transaction Commands
        private bool CanDelete(object? parameter) => SelectedTransaction != null;

        private async void DeleteTransaction(object? parameter)
        {
            try
            {
                if (SelectedTransaction != null)
                {
                    await repository.DeleteAsync(SelectedTransaction.Model);
                    BudgetTransactions.Remove(SelectedTransaction);
                    SelectedTransaction = null;
                    RefreshFilteredSummaries();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting transaction: {ex.Message}");
            }
        }


        private async void AddTransaction(object? parameter)
        {
            try
            {


                if (TransactionCategory == null || TransactionAmount <= 0) return;

                decimal finalAmount = TransactionAmount;

                if (TransactionCategory.Name == "Lön" && IsGrossIncome)
                {
                    finalAmount = TransactionAmount * (1 - (TaxRate / 100m));
                }

                var transaction = new BudgetTransaction
                {
                    Date = TransactionDate,
                    Amount = finalAmount,
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
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding transaction: {ex.Message}");
            }

        }

        private async void UpdateTransaction(object? parameter)
        {
            try
            {

                if (SelectedTransaction == null) return;

                decimal finalAmount = TransactionAmount;

                if (TransactionCategory?.Name == "Lön" && IsGrossIncome)
                {
                    finalAmount = TransactionAmount * (1 - (TaxRate / 100m));
                }

                var model = SelectedTransaction.Model;
                model.Date = TransactionDate;
                model.Amount = finalAmount;
                model.CategoryId = TransactionCategory!.Id;
                model.Recurrence = TransactionRecurrence;
                model.Description = TransactionDescription;
                model.Month = TransactionRecurrence == Recurrence.Yearly ? TransactionMonth : null;

                await repository.UpdateAsync(model);
                SelectedTransaction.RefreshFromModel();

                ExitEditMode();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating transaction: {ex.Message}");
            }
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

            // Nollställ SelectedTransaction via propertyn
            if (selectedTransaction != null)
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

            bool typeMatches =
                (ShowIncome && vm.Type == TransactionType.Income) ||
                (ShowExpense && vm.Type == TransactionType.Expense);

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


        public Visibility MonthVisibility
        {
            get
            {
                if (TransactionCategory?.Name == "Lön")
                    return Visibility.Collapsed;

                return TransactionRecurrence == Recurrence.Yearly
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
        }



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

                    // Om det inte längre är Yearly, nollställ month
                    if (_transactionRecurrence != Recurrence.Yearly)
                        TransactionMonth = null;
                }
            }
        }
        private Recurrence _transactionRecurrence = Recurrence.OneTime;




        // Skattesats, default 30%
        private decimal _taxRate = 30;
        public decimal TaxRate
        {
            get => _taxRate;
            set
            {
                if (_taxRate != value)
                {
                    _taxRate = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(MonthlyForecast));
                    RaisePropertyChanged(nameof(FilteredMonthlyForecast));
                }
            }
        }

        // Hjälpproperty för att visa/aktivera skatteinställningar
        public bool IsIncomeCategorySelected => TransactionCategory?.Name == "Lön";

        public decimal AnnualIncome =>
            BudgetTransactions
                .Where(t => t.Type == TransactionType.Income && t.Category?.Name == "Lön")
                .Sum(t => t.Recurrence switch
                {
                    Recurrence.Monthly => t.Amount * 12,
                    Recurrence.Yearly => t.Amount,
                    Recurrence.OneTime => t.Amount,
                    _ => 0
                });



        //private void UpdateTransactionAmountForGross()
        //{
        //    if (TransactionCategory?.Name == "Lön")
        //    {
        //        // Beräkna beloppet före/efter skatt utan att skriva över användarens inmatning
        //        RaisePropertyChanged(nameof(TransactionAmount));
        //        RaisePropertyChanged(nameof(AnnualIncome));
        //    }
        //}

        private bool _isGrossIncome = true;
        public bool IsGrossIncome
        {
            get => _isGrossIncome;
            set
            {
                if (_isGrossIncome != value)
                {
                    _isGrossIncome = value;
                    RaisePropertyChanged();
                    UpdateTransactionAmountForGross(); // triggar UI
                    RaisePropertyChanged(nameof(MonthlyForecast));
                    RaisePropertyChanged(nameof(FilteredMonthlyForecast));
                }
            }
        }


        public bool ShowIncomeOptions => TransactionCategory?.Type == TransactionType.Income;




    }
}
