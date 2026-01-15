using System;
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
                RaisePropertyChanged(nameof(MonthlyForecast));
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
                RaisePropertyChanged(nameof(MonthlyForecast));
            }
        }

        public decimal HourlyRate => AnnualWorkHours == 0 ? 0 : AnnualIncome / AnnualWorkHours;
        public decimal MonthlyIncome => AnnualIncome / 12m;
        public decimal MonthlyForecast => MonthlyIncome;

        public MonthlyForecastViewModel()
        {
        }
    }
}
