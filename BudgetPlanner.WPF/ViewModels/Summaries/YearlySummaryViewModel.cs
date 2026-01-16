using System.Collections.ObjectModel;
using System.Collections.Specialized;
using BudgetPlanner.WPF.ViewModels.Filters;
using BudgetPlanner.WPF.ViewModels.Items;

public class YearlySummaryViewModel : SummaryBaseViewModel
{
    private readonly ObservableCollection<TransactionItemViewModel> _allItems;
    private readonly PeriodFilterViewModel _period;

    public YearlySummaryViewModel(ObservableCollection<TransactionItemViewModel> allItems, PeriodFilterViewModel period)
        : base(allItems)
    {
        _allItems = allItems;
        _period = period;
        _period.PropertyChanged += (_, __) => Refresh();
        if (_allItems is INotifyCollectionChanged incc)
            incc.CollectionChanged += (_, __) => Refresh();
    }

    protected override IEnumerable<TransactionItemViewModel> FilterItems()
    {
        return _allItems.Where(t => t.StartDate.Year == _period.Year);
    }

    public void Refresh()
    {
        RaisePropertyChanged(nameof(Income));
        RaisePropertyChanged(nameof(Expense));
        RaisePropertyChanged(nameof(Result));
    }
}