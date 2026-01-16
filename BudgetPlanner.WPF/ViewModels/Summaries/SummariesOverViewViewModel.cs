using System.Collections.ObjectModel;
using System.Collections.Specialized;
using BudgetPlanner.WPF.ViewModels.Base;
using BudgetPlanner.WPF.ViewModels.Filters;
using BudgetPlanner.WPF.ViewModels.Items;

public class SummariesOverviewViewModel : ViewModelBase
{
    public PeriodFilterViewModel Period { get; } = new PeriodFilterViewModel();

    public MonthlySummaryViewModel Monthly { get; }
    public YearlySummaryViewModel Yearly { get; }
    public MonthlyForecastViewModel Forecast { get; }

    public SummariesOverviewViewModel(ObservableCollection<TransactionItemViewModel> items)
    {
        Monthly = new MonthlySummaryViewModel(items, Period);
        Yearly = new YearlySummaryViewModel(items, Period);
        Forecast = new MonthlyForecastViewModel(items);

        if (items is INotifyCollectionChanged incc)
        {
            incc.CollectionChanged += (_, __) =>
            {
                Monthly.Refresh();
                Yearly.Refresh();
                Forecast.Refresh();
            };
        }

        Period.PropertyChanged += (_, __) =>
        {
            Monthly.Refresh();
            Yearly.Refresh();
        };
    }
}
