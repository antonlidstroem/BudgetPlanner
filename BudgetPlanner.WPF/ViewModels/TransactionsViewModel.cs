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
using BudgetPlanner.WPF.ViewModels.Summaries;

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
        public decimal FilteredTotalIncome => Filter.TotalIncome(Transactions);
        public decimal FilteredTotalExpense => Filter.TotalExpense(Transactions);
        public decimal FilteredTotalResult => Filter.Result(Transactions);
        public decimal FilteredMonthlyForecast => Filter.MonthlyForecast(Transactions);

        private TransactionItemViewModel? _selected;
        public MonthSummaryViewModel MonthSummary { get; }
        public MonthlyForecastViewModel MonthlyForecast { get; }
        public YearSummaryViewModel YearSummary { get; }
 



        // KONSTRUKTOR
        public TransactionsViewModel()
        {
            repository = new BudgetTransactionRepository(new BudgetDbContext());

            TransactionsView = CollectionViewSource.GetDefaultView(Transactions);
            TransactionsView.Filter = o => Filter.Matches((TransactionItemViewModel)o);

            MonthSummary = new MonthSummaryViewModel(TransactionsView);
            MonthlyForecast = new MonthlyForecastViewModel();
            YearSummary = new YearSummaryViewModel(Transactions);

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
                Transactions.Add(new TransactionItemViewModel(t));
        }

        private async void AddTransaction(object? _)
        {
            if (Form.TransactionCategory == null)
                return;

            var model = new BudgetTransaction
            {
                StartDate = Form.TransactionDate,
                EndDate = Form.ShowEndDate ? Form.EndDate : null,

                NetAmount = Form.EffectiveAmount,
                GrossAmount = (Form.TransactionCategory?.ToggleGrossNet == true && Form.IsGross)
                ? Form.TransactionAmount
                : null,

                Rate = Form.ShowRate ? Form.Rate : null,
                Month = Form.ShowMonth ? Form.TransactionMonth : null,

                Description = Form.TransactionCategory.Description
                              ?? Form.TransactionDescription,

                CategoryId = Form.TransactionCategory.Id,
                Recurrence = Form.TransactionRecurrence,
                IsActive = true
            };

            await repository.AddAsync(model);
            Transactions.Add(new TransactionItemViewModel(model));

            RefreshFilter();

    

            Form.Clear();
        }

        private void RefreshFilter()
        {
            RaisePropertyChanged(nameof(FilteredTotalIncome));
            RaisePropertyChanged(nameof(FilteredTotalExpense));
            RaisePropertyChanged(nameof(FilteredTotalResult));
            RaisePropertyChanged(nameof(FilteredMonthlyForecast));
        }

        private async void UpdateTransaction(object? _)
        {
            if (Selected == null || Form.TransactionCategory == null)
                return;

            var m = Selected.Model;

            m.StartDate = Form.TransactionDate;
            m.EndDate = Form.ShowEndDate ? Form.EndDate : null;

            m.NetAmount = Form.EffectiveAmount;
            m.GrossAmount = (Form.TransactionCategory?.ToggleGrossNet == true && Form.IsGross)
                             ? Form.TransactionAmount
                             : null;

            m.Rate = Form.ShowRate ? Form.Rate : null;
            m.Month = Form.ShowMonth ? Form.TransactionMonth : null;

            m.Description = Form.TransactionCategory.Description
                            ?? Form.TransactionDescription;

            m.CategoryId = Form.TransactionCategory.Id;
            m.Category = Form.TransactionCategory;

            m.Recurrence = Form.TransactionRecurrence;

            await repository.UpdateAsync(m);

            Selected.RefreshFromModel();
        }



        private async void DeleteTransaction(object? _)
        {
            if (Selected == null) return;
            await repository.DeleteAsync(Selected.Model);
            Transactions.Remove(Selected);

            RefreshFilter();
        }

        private bool _isInEditMode;
        public bool IsInEditMode
        {
            get => _isInEditMode;
            set { _isInEditMode = value; RaisePropertyChanged(); }
        }

        public TransactionItemViewModel? Selected
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

                Form.TransactionCategory = value.Category;

                Form.IsGross = Form.TransactionCategory?.ToggleGrossNet == true && value.GrossAmount != value.NetAmount;

                Form.TransactionAmount = Form.IsGross && value.GrossAmount.HasValue
                    ? value.GrossAmount.Value
                    : value.NetAmount;


                Form.Rate = value.Rate;
                Form.TransactionDescription = value.Description;
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

            MonthSummary?.RaiseAll();
            YearSummary?.RaiseAll();

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
