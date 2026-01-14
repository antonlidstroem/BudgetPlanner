using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using BudgetPlanner.DAL.Models;
using BudgetPlanner.WPF.ViewModels.Base;

namespace BudgetPlanner.WPF.ViewModels.Forms
{
    public class TransactionFormViewModel : ViewModelBase
    {
        public DateTime TransactionDate { get; set; } = DateTime.Today;
        private decimal _transactionAmount;

        // SUMMA
        public decimal TransactionAmount
        {
            get => _transactionAmount;
            set
            {
                _transactionAmount = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(HourlyRate));
                RaisePropertyChanged(nameof(MonthlyIncomeFromHours));
            }
        }

        // BESKRIVNING
        public string TransactionDescription { get; set; } = string.Empty;

        private Category? _transactionCategory;
        public Category? TransactionCategory
        {
            get => _transactionCategory;
            set
            {
                _transactionCategory = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(IsIncomeCategorySelected));
                RaisePropertyChanged(nameof(MonthVisibility));
                RaisePropertyChanged(nameof(ShowIncomeOptions));
            }
        }

        // UPPREPANDE
        private Recurrence _transactionRecurrence = Recurrence.OneTime;
        public Recurrence TransactionRecurrence
        {
            get => _transactionRecurrence;
            set
            {
                _transactionRecurrence = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(MonthVisibility));
                if (value != Recurrence.Yearly) TransactionMonth = null;
            }
        }

        // TRANSAKTIONSMÅNAD
        private int? _transactionMonth;
        public int? TransactionMonth
        {
            get => _transactionMonth;
            set { _transactionMonth = value; RaisePropertyChanged(); }
        }

        // BRUTTOINKOMST
        public bool IsGrossIncome { get; set; }

        // SKATTESATS
        public decimal TaxRate { get; set; } = 30;

        // ÅRSARBETSTID
        private decimal _annualHours = 1600;
        public decimal AnnualHours
        {
            get => _annualHours;
            set
            {
                _annualHours = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(HourlyRate));
                RaisePropertyChanged(nameof(MonthlyIncomeFromHours));
            }
        }

        // TIMLÖN
        public decimal HourlyRate =>
       AnnualHours > 0 && TransactionAmount > 0
           ? TransactionAmount / AnnualHours
           : 0;

        // MÅNADSINKOMST FRÅN TIMMAR
        public decimal MonthlyIncomeFromHours => HourlyRate * (AnnualHours / 12m);

        // MONTH VISIBILITY
        public Visibility MonthVisibility =>
        TransactionCategory?.Name == "Lön"
            ? Visibility.Collapsed
            : TransactionRecurrence == Recurrence.Yearly
                ? Visibility.Visible
                : Visibility.Collapsed;

        // LÖNKATEGORI VALD
        public bool IsIncomeCategorySelected => TransactionCategory?.Name == "Lön";

        public bool ShowIncomeOptions => TransactionCategory?.Type == TransactionType.Income;


        // RENSA FORMULÄRET
        public void Clear()
        {
            TransactionDate = DateTime.Today;
            TransactionAmount = 0;
            TransactionCategory = null;
            TransactionRecurrence = Recurrence.OneTime;
            TransactionDescription = string.Empty;
            TransactionMonth = null;
            RaisePropertyChanged(string.Empty);


        }

        public IEnumerable<Recurrence> RecurrenceValues { get; } =
    Enum.GetValues(typeof(Recurrence)).Cast<Recurrence>();

        public bool IsMonthVisible =>
    TransactionRecurrence == Recurrence.Yearly;


        // MÅNAD
        public IEnumerable<int> Months => Enumerable.Range(1, 12);

    }
}