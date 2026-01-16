using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using BudgetPlanner.DAL.Data;
using BudgetPlanner.DAL.Interfaces;
using BudgetPlanner.DAL.Models;
using BudgetPlanner.DAL.Repositories;
using BudgetPlanner.WPF.Commands;
using BudgetPlanner.WPF.ViewModels.Base;
using BudgetPlanner.WPF.ViewModels.Filters;
using BudgetPlanner.WPF.ViewModels.Forms;
using BudgetPlanner.WPF.ViewModels.Items;

namespace BudgetPlanner.WPF.ViewModels
{
    public class TransactionsViewModel : ViewModelBase
    {
        private readonly IBudgetTransactionRepository repository;

        public ObservableCollection<TransactionItemViewModel> Transactions { get; } = new();
        public ObservableCollection<Category> Categories { get; } = new();

        public FormViewModel Form { get; } = new();
        public FilterViewModel Filter { get; } = new();

        public ICollectionView TransactionsView { get; }

        public DelegateCommand AddCommand { get; }
        public DelegateCommand DeleteCommand { get; }
        public DelegateCommand UpdateCommand { get; }
        public DelegateCommand CancelEditCommand { get; }
        public DelegateCommand ClearFilterCommand { get; }

        public ObservableCollection<TransactionItemViewModel> SelectedItems { get; } = new();

        public bool IsInEditMode => SelectedItems.Any();

        public string SubmitButtonText => IsInEditMode ? "Uppdatera" : "Lägg till";

        public TransactionsViewModel()
        {
            repository = new BudgetTransactionRepository(new BudgetDbContext());
            TransactionsView = CollectionViewSource.GetDefaultView(Transactions);
            TransactionsView.Filter = o => Filter.Matches((TransactionItemViewModel)o);

            AddCommand = new DelegateCommand(AddTransaction);
            UpdateCommand = new DelegateCommand(UpdateTransaction, _ => SelectedItems.Any());
            DeleteCommand = new DelegateCommand(DeleteTransaction, _ => SelectedItems.Any());
            CancelEditCommand = new DelegateCommand(_ => { SelectedItems.Clear(); Form.Clear(); });
            ClearFilterCommand = new DelegateCommand(_ => { /* ... */ });

            SelectedItems.CollectionChanged += (_, __) =>
            {
                UpdateFormFromSelection();
                UpdateCommand.RaiseCanExecuteChanged();
                DeleteCommand.RaiseCanExecuteChanged();
            };

            Form.PropertyChanged += Form_PropertyChanged;
            Filter.PropertyChanged += Filter_PropertyChanged;

            _ = LoadAsync();
        }

        private async Task LoadAsync()
        {
            var categories = await repository.GetCategoriesAsync();
            foreach (var c in categories) Categories.Add(c);

            var transactions = await repository.GetAllAsync();
            foreach (var t in transactions) Transactions.Add(new TransactionItemViewModel(t));
        }

        private void UpdateFormFromSelection()
        {
            if (!SelectedItems.Any())
            {
                Form.Clear();
                return;
            }

            T? Common<T>(Func<TransactionItemViewModel, T> selector)
            {
                var first = selector(SelectedItems[0]);
                return SelectedItems.All(i => EqualityComparer<T>.Default.Equals(selector(i), first))
                    ? first
                    : default;
            }

            Form.TransactionDate = Common(t => (DateTime?)t.StartDate) ?? default;
            Form.TransactionDescription = Common(t => t.Description) ?? string.Empty;
            Form.TransactionCategory = Common(t => t.Category);
            Form.TransactionRecurrence = Common(t => t.Recurrence);
            Form.TransactionMonth = Common(t => t.Month);
            Form.Rate = Common(t => t.Rate);
            Form.TransactionAmount = Common(t => (decimal?)t.NetAmount) ?? 0m;
            Form.IsGross = SelectedItems.All(i => i.GrossAmount.HasValue && i.GrossAmount != i.NetAmount);
        }

        private void Form_PropertyChanged(object? sender, PropertyChangedEventArgs e) { /* ... */ }
        private void Filter_PropertyChanged(object? sender, PropertyChangedEventArgs e) { /* ... */ }

        private async void AddTransaction(object? _) { /* ... */ }
        private async void UpdateTransaction(object? _) { /* ... */ }
        private async void DeleteTransaction(object? _) { /* ... */ }
    }
}
