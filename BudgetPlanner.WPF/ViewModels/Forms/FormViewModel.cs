using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using BudgetPlanner.DAL.Models;
using BudgetPlanner.WPF.ViewModels.Base;

namespace BudgetPlanner.WPF.ViewModels.Forms
{
    public class FormViewModel : ViewModelBase
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
                ShowAbsenceFields = _transactionCategory?.Name == "VAB/Sjukfrånvaro";
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
        private bool _isGrossIncome;
        public bool IsGrossIncome
        {
            get => _isGrossIncome;
            set
            {
                _isGrossIncome = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(EffectiveNetAmount));
            }
        }


        // SKATTESATS
        private decimal _taxRate = 30;
        public decimal TaxRate
        {
            get => _taxRate;
            set
            {
                _taxRate = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(EffectiveNetAmount));
            }
        }

        public decimal EffectiveNetAmount
        {
            get
            {
                if (TransactionCategory?.Name != "Lön")
                    return TransactionAmount;

                if (!IsGrossIncome)
                    return TransactionAmount;

                return TransactionAmount * (1 - TaxRate / 100m);
            }
        }


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


        // Extra fält för frånvaro
        private bool _showAbsenceFields;
        public bool ShowAbsenceFields
        {
            get => _showAbsenceFields;
            set { _showAbsenceFields = value; RaisePropertyChanged(); }
        }

        private DateTime _absenceDate = DateTime.Today;
        public DateTime AbsenceDate
        {
            get => _absenceDate;
            set { _absenceDate = value; RaisePropertyChanged(); }
        }

        private AbsenceType _absenceType = AbsenceType.Sick;
        public AbsenceType AbsenceType
        {
            get => _absenceType;
            set { _absenceType = value; RaisePropertyChanged(); }
        }

        private double _absenceHours = 8;
        public double AbsenceHours
        {
            get => _absenceHours;
            set { _absenceHours = value; RaisePropertyChanged(); }
        }



    }
}