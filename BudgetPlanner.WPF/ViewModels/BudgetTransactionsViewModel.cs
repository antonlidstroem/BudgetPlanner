using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using BudgetPlanner.DAL.Data;
using BudgetPlanner.DAL.Interfaces;
using BudgetPlanner.DAL.Models;
using BudgetPlanner.DAL.Repositories;
using BudgetPlanner.WPF.Command;
using Microsoft.EntityFrameworkCore;

namespace BudgetPlanner.WPF.ViewModels
{


    public class BudgetTransactionsViewModel : ViewModelBase
    {
        private readonly IBudgetTransactionRepository repository;
        public ObservableCollection<BudgetTransactionItemsViewModel> BudgetTransactions { get; set; } = new();
        public ICollectionView TransactionsView { get; }

        // Ny transaktion
        public DateTime NewTransactionDate { get; set; } = DateTime.Today;
        public decimal NewTransactionAmount { get; set; }
        public Category? NewTransactionCategory { get; set; }
        public Recurrence NewTransactionRecurrence { get; set; } = Recurrence.OneTime;
        public String NewTransactionDescription { get; set; }

        // Filteregenskaper summeringar
        private bool _showOneTime = true;
        public bool ShowOneTime
        {
            get => _showOneTime;
            set
            {
                _showOneTime = value;
                RaisePropertyChanged();
                RaisePropertyChangedFilteredSummarys();
            }
        }

        private bool _showMonthly = true;
        public bool ShowMonthly
        {
            get => _showMonthly;
            set
            {
                _showMonthly = value;
                RaisePropertyChanged();
                RaisePropertyChangedFilteredSummarys();
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
                RaisePropertyChangedFilteredSummarys();
            }
        }

        //Filteregenskaper kategorier
        private Category? _selectedFilterCategory;
        public Category? SelectedFilterCategory
        {
            get => _selectedFilterCategory;
            set
            {
                _selectedFilterCategory = value;
                RaisePropertyChanged();
                RaisePropertyChangedFilteredSummarys();
            }
        }

        //Filteregenskaper utgifter inkomster
        private bool _showIncome = true;
        public bool ShowIncome
        {
            get => _showIncome;
            set
            {
                if (_showIncome != value)
                {
                    _showIncome = value;
                    RaisePropertyChanged();
                    TransactionsView.Refresh();
                }
            }
        }

        private bool _showExpense = true;
        public bool ShowExpense
        {
            get => _showExpense;
            set
            {
                if (_showExpense != value)
                {
                    _showExpense = value;
                    RaisePropertyChanged();
                    TransactionsView.Refresh();
                }
            }
        }


        private void RaisePropertyChangedFilteredSummarys()
        {
            TransactionsView.Refresh();
            RaisePropertyChanged(nameof(FilteredTotalIncome));
            RaisePropertyChanged(nameof(FilteredTotalExpense));
            RaisePropertyChanged(nameof(FilteredTotalResult));
            RaisePropertyChanged(nameof(FilteredMonthlyForecast));
        }

        // Kategorier
        public ObservableCollection<Category> Categories { get; set; } = new();

        // Vald transaktion
        private BudgetTransactionItemsViewModel? selectedTransaction;
        public BudgetTransactionItemsViewModel? SelectedTransaction
        {
            get { return selectedTransaction; }
            set
            {
                selectedTransaction = value;
                RaisePropertyChanged();
                DeleteCommand.RaiseCanExecuteChanged();
            }
        }

        public DelegateCommand AddCommand { get; }
        public DelegateCommand DeleteCommand { get; }
        public DelegateCommand ClearCategoryFilterCommand { get; }

        // Summeringar
        public decimal TotalIncome =>
         BudgetTransactions.Where(t => t.Type == TransactionType.Income)
                           .Sum(t => t.Amount);

        public decimal TotalExpense =>
            BudgetTransactions.Where(t => t.Type == TransactionType.Expense)
                              .Sum(t => t.Amount);

        public decimal TotalResult => TotalIncome - TotalExpense;

        public decimal MonthlyForecast => BudgetTransactions
            .Where(t => t.IsActive)
            .Sum(t => t.Recurrence switch
            {
                Recurrence.Monthly => t.Amount,
                Recurrence.Yearly => t.Amount / 12,
                _ => 0
            });

        public BudgetTransactionsViewModel()
        {
            var db = new BudgetDbContext();
            repository = new BudgetTransactionRepository(db);

            AddCommand = new DelegateCommand(AddTransaction);
            DeleteCommand = new DelegateCommand(DeleteTransaction, CanDelete);
            ClearCategoryFilterCommand = new DelegateCommand(_ =>
            {
                SelectedFilterCategory = null;
            });

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

        public async Task LoadCategories()
        {
            Categories.Clear();
            var cats = await repository.GetCategoriesAsync();
            foreach (var cat in cats)
            {
                Categories.Add(cat);
            }
        }

        public async Task LoadTransactionsAsync()
        {
            BudgetTransactions.Clear();
            var transactions = await repository.GetAllAsync();
            foreach (var t in transactions)
            {
                BudgetTransactions.Add(new BudgetTransactionItemsViewModel(t));
            }
            RaisePropertyChangedFilteredSummarys();
        }

        private bool CanDelete(object? parameter) => SelectedTransaction != null;

        private async void DeleteTransaction(object? parameter)
        {
            if (SelectedTransaction != null)
            {
                await repository.DeleteAsync(SelectedTransaction.Model);
                BudgetTransactions.Remove(SelectedTransaction);
                SelectedTransaction = null;
                RaisePropertyChangedFilteredSummarys();
            }
        }

        private async void AddTransaction(object? parameter)
        {
            if (NewTransactionCategory == null || NewTransactionAmount <= 0) return;

            var transaction = new BudgetTransaction
            {
                Date = NewTransactionDate,
                Amount = NewTransactionAmount,
                CategoryId = NewTransactionCategory.Id,
                Recurrence = NewTransactionRecurrence,
                Description = NewTransactionDescription,
            };

            await repository.AddAsync(transaction);
            BudgetTransactions.Add(new BudgetTransactionItemsViewModel(transaction));
            RaisePropertyChangedFilteredSummarys();

            // Nollställ formuläret
            NewTransactionDate = DateTime.Today;
            NewTransactionAmount = 0;
            NewTransactionCategory = null;
            NewTransactionRecurrence = Recurrence.OneTime;
            NewTransactionDescription = string.Empty;

            RaisePropertyChanged(nameof(NewTransactionDate));
            RaisePropertyChanged(nameof(NewTransactionAmount));
            RaisePropertyChanged(nameof(NewTransactionCategory));
            RaisePropertyChanged(nameof(NewTransactionRecurrence));
            RaisePropertyChanged(nameof(NewTransactionDescription));
        }

        private bool FilterTransactions(object obj)
        {
            if (obj is not BudgetTransactionItemsViewModel vm)
                return false;

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

        //Summeringar av filtrerade transaktioner
        public decimal FilteredTotalIncome => TransactionsView.Cast<BudgetTransactionItemsViewModel>()
            .Where(t => t.Type == TransactionType.Income)
            .Sum(t => t.Amount);

        public decimal FilteredTotalExpense => TransactionsView.Cast<BudgetTransactionItemsViewModel>()
            .Where(t => t.Type == TransactionType.Expense)
            .Sum(t => t.Amount);

        public decimal FilteredTotalResult => FilteredTotalIncome - FilteredTotalExpense;

        public decimal FilteredMonthlyForecast => TransactionsView.Cast<BudgetTransactionItemsViewModel>()
            .Where(t => t.IsActive)
            .Sum(t => (t.Type == TransactionType.Income ? 1 : -1) * (t.Recurrence switch
            {
                Recurrence.Monthly => t.Amount,
                Recurrence.Yearly => t.Amount / 12,
                _ => 0
       }));



    }
}
