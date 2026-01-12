using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
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

        // Ny transaktion
        public DateTime NewTransactionDate { get; set; } = DateTime.Today;
        public decimal NewTransactionAmount { get; set; }
        public Category? NewTransactionCategory { get; set; }
        public Recurrence NewTransactionRecurrence { get; set; } = Recurrence.OneTime;


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


            BudgetTransactions.CollectionChanged += (_, __) =>
            {
                RaisePropertyChanged(nameof(TotalIncome));
                RaisePropertyChanged(nameof(TotalExpense));
                RaisePropertyChanged(nameof(TotalResult));
                RaisePropertyChanged(nameof(MonthlyForecast));
            };
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
        }

        private bool CanDelete(object? parameter) => SelectedTransaction != null;

        private async void DeleteTransaction(object? parameter)
        {
            if (SelectedTransaction != null)
            {
                await repository.DeleteAsync(SelectedTransaction.Model);
                BudgetTransactions.Remove(SelectedTransaction);
                SelectedTransaction = null;
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
            };

            await repository.AddAsync(transaction);
            BudgetTransactions.Add(new BudgetTransactionItemsViewModel(transaction));

            // Nollställ formuläret
            NewTransactionDate = DateTime.Today;
            NewTransactionAmount = 0;
            NewTransactionCategory = null;
            NewTransactionRecurrence = Recurrence.OneTime;

            RaisePropertyChanged(nameof(NewTransactionDate));
            RaisePropertyChanged(nameof(NewTransactionAmount));
            RaisePropertyChanged(nameof(NewTransactionCategory));
            RaisePropertyChanged(nameof(NewTransactionRecurrence));
        }
    }
}
