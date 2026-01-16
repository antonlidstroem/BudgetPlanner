using System;
using System.ComponentModel;
using BudgetPlanner.WPF.ViewModels.Base;

namespace BudgetPlanner.WPF.ViewModels.Filters
{
    public class PeriodFilterViewModel : ViewModelBase
    {
        private int _month = DateTime.Today.Month;
        private int _year = DateTime.Today.Year;

        public int Month
        {
            get => _month;
            set { _month = value; RaisePropertyChanged(); }
        }

        public int Year
        {
            get => _year;
            set { _year = value; RaisePropertyChanged(); }
        }
    }
}
