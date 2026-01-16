using System;
using System.Collections.Generic;
using System.Linq;
using BudgetPlanner.DAL.Models;
using BudgetPlanner.WPF.ViewModels.Base;

namespace BudgetPlanner.WPF.ViewModels.Forms
{
    public class FormViewModel : ViewModelBase
    {
        public DateTime TransactionDate { get; set; } = DateTime.Today;

        private decimal _transactionAmount;
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

        public string TransactionDescription { get; set; } = string.Empty;

        private Category? _transactionCategory;
        public Category? TransactionCategory
        {
            get => _transactionCategory;
            set
            {
                _transactionCategory = value;
                RaisePropertyChanged();

                RaisePropertyChanged(nameof(ShowEndDate));
                RaisePropertyChanged(nameof(ShowDescription));
                RaisePropertyChanged(nameof(ShowMonth));
                RaisePropertyChanged(nameof(ShowRate));
                RaisePropertyChanged(nameof(ShowGrossNetToggle));

                if (ShowGrossNetToggle != true)
                    IsGross = false;

                if (!ShowMonth)
                    TransactionMonth = null;
            }
        }

        private Recurrence _transactionRecurrence = Recurrence.OneTime;
        public Recurrence TransactionRecurrence
        {
            get => _transactionRecurrence;
            set
            {
                if (_transactionRecurrence != value)
                {
                    _transactionRecurrence = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(ShowMonth));

                    if (!ShowMonth)
                        TransactionMonth = null;
                }
            }
        }

        private int? _transactionMonth;
        public int? TransactionMonth
        {
            get => _transactionMonth;
            set { _transactionMonth = value; RaisePropertyChanged(); }
        }

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

        public decimal HourlyRate => AnnualHours > 0 && TransactionAmount > 0 ? TransactionAmount / AnnualHours : 0;
        public decimal MonthlyIncomeFromHours => HourlyRate * (AnnualHours / 12m);

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

        public IEnumerable<Recurrence> RecurrenceValues => Enum.GetValues(typeof(Recurrence)).Cast<Recurrence>();
        public bool ShowMonth => TransactionRecurrence == Recurrence.Yearly;
        public IEnumerable<int> Months => Enumerable.Range(1, 12);

        public bool ShowEndDate => TransactionCategory?.HasEndDate == true;
        public bool ShowDescription => !string.IsNullOrWhiteSpace(TransactionCategory?.Description);
        public bool ShowRate => TransactionCategory?.DefaultRate != null;
        public bool ShowGrossNetToggle => TransactionCategory?.ToggleGrossNet == true;

        private bool _isGross;
        public bool IsGross
        {
            get => _isGross;
            set { _isGross = value; RaisePropertyChanged(); RaisePropertyChanged(nameof(EffectiveAmount)); }
        }

        private decimal? _rate;
        public decimal? Rate
        {
            get => _rate;
            set { _rate = value; RaisePropertyChanged(); RaisePropertyChanged(nameof(EffectiveAmount)); }
        }

        public decimal EffectiveAmount
        {
            get
            {
                if (TransactionCategory?.ToggleGrossNet != true || !IsGross || Rate == null)
                    return TransactionAmount;

                var factor = Rate.Value / 100m;
                return TransactionCategory.AdjustmentType == AdjustmentType.Deduction
                    ? TransactionAmount * (1 - factor)
                    : TransactionAmount * (1 + factor);
            }
        }

        private DateTime? _endDate;
        public DateTime? EndDate
        {
            get => _endDate;
            set { _endDate = value; RaisePropertyChanged(); }
        }

        // --- Edit mode (för knappar i UI) ---
        private bool _isInEditMode;
        public bool IsInEditMode
        {
            get => _isInEditMode;
            set { _isInEditMode = value; RaisePropertyChanged(); }
        }

        public void SetEditMode(bool editMode)
        {
            IsInEditMode = editMode;
        }
    }
}
