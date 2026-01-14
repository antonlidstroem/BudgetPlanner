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
using BudgetPlanner.WPF.ViewModels.Base;
using BudgetPlanner.WPF.ViewModels.Filters;
using BudgetPlanner.WPF.ViewModels.Forms;
using BudgetPlanner.WPF.ViewModels.Items;

namespace BudgetPlanner.WPF.ViewModels
{
    public class BudgetTransactionsViewModel : ViewModelBase
    {
        private readonly IBudgetTransactionRepository repository;

        public ObservableCollection<BudgetTransactionItemViewModel> Transactions { get; } = new();
        public ObservableCollection<Category> Categories { get; } = new();

        public TransactionFormViewModel Form { get; } = new();
        public TransactionFilterViewModel Filter { get; } = new();

        public ICollectionView TransactionsView { get; }

        public DelegateCommand AddCommand { get; }
        public DelegateCommand DeleteCommand { get; }
        public DelegateCommand UpdateCommand { get; }
        public DelegateCommand CancelEditCommand { get; }
        public DelegateCommand ClearFilterCommand { get; }
        public decimal FilteredTotalIncome => Filter.TotalIncome(Transactions);
        public decimal FilteredTotalExpense => Filter.TotalExpense(Transactions);
        public decimal FilteredTotalResult => Filter.Result(Transactions);
        public decimal FilteredMonthlyForecast => Filter.MonthlyForecast(Transactions);

        private BudgetTransactionItemViewModel? _selected;
        public TransactionSummaryViewModel Summaries { get; }



        // KONSTRUKTOR
        public BudgetTransactionsViewModel()
        {
            repository = new BudgetTransactionRepository(new BudgetDbContext());

            TransactionsView = CollectionViewSource.GetDefaultView(Transactions);
            TransactionsView.Filter = o => Filter.Matches((BudgetTransactionItemViewModel)o);

            Summaries = new TransactionSummaryViewModel(TransactionsView);

            AddCommand = new DelegateCommand(AddTransaction);
            UpdateCommand = new DelegateCommand(UpdateTransaction, _ => Selected != null);
            DeleteCommand = new DelegateCommand(DeleteTransaction, _ => Selected != null);
            CancelEditCommand = new DelegateCommand(_ =>
            {
                Selected = null;
            });

            ClearFilterCommand = new DelegateCommand(_ =>
            {
                Filter.SelectedCategory = null;
                Filter.ShowIncome = true;
                Filter.ShowExpense = true;
                Filter.ShowOneTime = true;
                Filter.ShowMonthly = true;
                Filter.ShowYearly = true;

                TransactionsView.Refresh();

                RaisePropertyChanged(nameof(FilteredTotalIncome));
                RaisePropertyChanged(nameof(FilteredTotalExpense));
                RaisePropertyChanged(nameof(FilteredTotalResult));
                RaisePropertyChanged(nameof(FilteredMonthlyForecast));
            });

            // Lyssnar på ändringar i formuläret för att uppdatera filter
            Filter.PropertyChanged += Filter_PropertyChanged;

            Form.PropertyChanged += Form_PropertyChanged;



            _ = LoadAsync();
        }

        private async Task LoadAsync()
        {
            var categories = await repository.GetCategoriesAsync();
            foreach (var c in categories)
                Categories.Add(c);

            var transactions = await repository.GetAllAsync();
            foreach (var t in transactions)
                Transactions.Add(new BudgetTransactionItemViewModel(t));
        }

        private async void AddTransaction(object? _)
        {
            var net = Form.TransactionAmount;

            if (Form.TransactionCategory?.Name == "Lön" && Form.IsGrossIncome)
                net *= (1 - Form.TaxRate / 100m);

            var model = new BudgetTransaction
            {
                Date = Form.TransactionDate,
                Amount = net,
                GrossAmount = Form.TransactionAmount,
                CategoryId = Form.TransactionCategory!.Id,
                Recurrence = Form.TransactionRecurrence,
                Description = Form.TransactionDescription,
                Month = Form.TransactionRecurrence == Recurrence.Yearly ? Form.TransactionMonth : null,
                IsActive = true
            };

            await repository.AddAsync(model);
            Transactions.Add(new BudgetTransactionItemViewModel(model));
            Form.Clear();
        }

        private async void UpdateTransaction(object? _)
        {
            if (Selected == null) return;

            var net = Form.TransactionAmount;
            if (Form.TransactionCategory?.Name == "Lön" && Form.IsGrossIncome)
                net *= (1 - Form.TaxRate / 100m);

            // Uppdatera modellen med alla fält
            Selected.Model.Date = Form.TransactionDate;
            Selected.Model.Amount = net;
            Selected.Model.GrossAmount = Form.TransactionAmount;
            Selected.Model.Description = Form.TransactionDescription;
            Selected.Model.Category = Form.TransactionCategory;
            Selected.Model.CategoryId = Form.TransactionCategory?.Id ?? 0;
            Selected.Model.Recurrence = Form.TransactionRecurrence;
            Selected.Model.Month = Form.TransactionRecurrence == Recurrence.Yearly ? Form.TransactionMonth : null;
            Selected.Model.IsGrossIncome = Form.IsGrossIncome;

            // Spara till databasen
            await repository.UpdateAsync(Selected.Model);

            // Uppdatera listan
            Selected.RefreshFromModel();
        }


        private async void DeleteTransaction(object? _)
        {
            if (Selected == null) return;
            await repository.DeleteAsync(Selected.Model);
            Transactions.Remove(Selected);
        }

        private bool _isInEditMode;
        public bool IsInEditMode
        {
            get => _isInEditMode;
            set { _isInEditMode = value; RaisePropertyChanged(); }
        }

        public BudgetTransactionItemViewModel? Selected
        {
            get => _selected;
            set
            {
                _selected = value;
                RaisePropertyChanged();

                UpdateCommand.RaiseCanExecuteChanged();
                DeleteCommand.RaiseCanExecuteChanged();

                if (value == null)
                {
                    IsInEditMode = false;
                    Form.Clear();
                    return;
                }

                IsInEditMode = true;

                Form.TransactionDate = value.Date;
                Form.TransactionAmount = value.Amount;
                Form.TransactionDescription = value.Description;
                Form.TransactionCategory = value.Category;
                Form.TransactionRecurrence = value.Recurrence;
                Form.TransactionMonth = value.Month;
            }
        }

        private void ApplyFormFilters()
        {
            // Temporärt koppla bort PropertyChanged för att undvika loop
            Filter.PropertyChanged -= Filter_PropertyChanged;

            Filter.FilterDate = Filter.FilterByDate ? Form.TransactionDate : null;
            Filter.FilterDescription = Filter.FilterByDescription ? Form.TransactionDescription : string.Empty;
            Filter.FilterAmount = Filter.FilterByAmount ? Form.TransactionAmount : null;
            Filter.FilterCategory = Filter.FilterByCategory ? Form.TransactionCategory : null;
            Filter.FilterRecurrence = Filter.FilterByRecurrence ? Form.TransactionRecurrence : null;

            TransactionsView.Refresh();
            Summaries.RaiseAll();
            RaisePropertyChanged(nameof(FilteredTotalIncome));
            RaisePropertyChanged(nameof(FilteredTotalExpense));
            RaisePropertyChanged(nameof(FilteredTotalResult));
            RaisePropertyChanged(nameof(FilteredMonthlyForecast));

            // Koppla tillbaka PropertyChanged
            Filter.PropertyChanged += Filter_PropertyChanged;
        }

        private void Filter_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.StartsWith("FilterBy") || e.PropertyName.StartsWith("Filter"))
                ApplyFormFilters();
        }

        private void Form_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // Kolla om ändringen motsvarar ett filter som är aktivt
            if ((e.PropertyName == nameof(Form.TransactionDate) && Filter.FilterByDate) ||
                (e.PropertyName == nameof(Form.TransactionDescription) && Filter.FilterByDescription) ||
                (e.PropertyName == nameof(Form.TransactionAmount) && Filter.FilterByAmount) ||
                (e.PropertyName == nameof(Form.TransactionCategory) && Filter.FilterByCategory) ||
                (e.PropertyName == nameof(Form.TransactionRecurrence) && Filter.FilterByRecurrence))
            {
                ApplyFormFilters();
            }
        }




    }

}
