using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using BudgetPlanner.WPF.ViewModels.Base;
using BudgetPlanner.WPF.ViewModels.Items;

namespace BudgetPlanner.WPF.ViewModels.Summaries
{
    public class SummariesOverviewViewModel : ViewModelBase
    {
        public MonthSummaryViewModel MonthSummary { get; }
        public YearSummaryViewModel YearSummary { get; }
        public MonthlyForecastViewModel MonthlyForecast { get; }

        public SummariesOverviewViewModel(ICollectionView transactionsView, ObservableCollection<TransactionItemViewModel> allItems)
        {
            int currentMonth = DateTime.Today.Month;
            int currentYear = DateTime.Today.Year;

            MonthSummary = new MonthSummaryViewModel(transactionsView, currentMonth, currentYear);
            YearSummary = new YearSummaryViewModel(allItems);
            MonthlyForecast = new MonthlyForecastViewModel(transactionsView);
        }

        public void RaiseAll()
        {
            MonthSummary.RaiseAll();
            YearSummary.RaiseAll();
            MonthlyForecast.RaiseAll();
        }
    }
}
