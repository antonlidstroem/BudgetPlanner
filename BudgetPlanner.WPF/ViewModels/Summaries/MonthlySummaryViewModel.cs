using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using BudgetPlanner.WPF.ViewModels.Base;
using BudgetPlanner.WPF.ViewModels.Filters;
using BudgetPlanner.WPF.ViewModels.Items;

public class MonthlySummaryViewModel : SummaryBaseViewModel
{
    private readonly ObservableCollection<TransactionItemViewModel> _allItems;
    private readonly PeriodFilterViewModel _period;

    public MonthlySummaryViewModel(ObservableCollection<TransactionItemViewModel> allItems, PeriodFilterViewModel period)
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
        return _allItems.Where(t => t.StartDate.Month == _period.Month && t.StartDate.Year == _period.Year);
    }

    public void Refresh()
    {
        RaisePropertyChanged(nameof(Income));
        RaisePropertyChanged(nameof(Expense));
        RaisePropertyChanged(nameof(Result));
    }
}