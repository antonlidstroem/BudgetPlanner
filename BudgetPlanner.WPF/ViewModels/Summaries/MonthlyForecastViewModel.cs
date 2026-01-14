using System;
using System.Collections.Generic;
using System.Text;
using BudgetPlanner.WPF.ViewModels.Base;

namespace BudgetPlanner.WPF.ViewModels.Summaries
{
    public class MonthlyForecastViewModel : ViewModelBase
    {
        private decimal _annualIncome;
        private decimal _annualWorkHours;

        public decimal AnnualIncome
        {
            get => _annualIncome;
            set
            {
                _annualIncome = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(MonthlyIncome));
                RaisePropertyChanged(nameof(HourlyRate));
            }
        }

        public decimal AnnualWorkHours
        {
            get => _annualWorkHours;
            set
            {
                _annualWorkHours = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(MonthlyIncome));
                RaisePropertyChanged(nameof(HourlyRate));
            }
        }

        public decimal HourlyRate =>
            AnnualWorkHours == 0 ? 0 : AnnualIncome / AnnualWorkHours;

        public decimal MonthlyIncome =>
            AnnualIncome / 12m;
    }

}
