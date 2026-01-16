using BudgetPlanner.WPF.Services;
using BudgetPlanner.WPF.ViewModels.Base;
using BudgetPlanner.WPF.ViewModels.Items;

public abstract class SummaryBaseViewModel : ViewModelBase
{
    protected IEnumerable<TransactionItemViewModel> AllItems { get; }

    protected SummaryBaseViewModel(IEnumerable<TransactionItemViewModel> items)
    {
        AllItems = items;
    }

    // Virtual för att tillåta filtrering i child-klasser
    protected virtual IEnumerable<TransactionItemViewModel> FilterItems() => AllItems;

    protected SummaryResult Calculate() => TransactionSummaryCalculator.Calculate(FilterItems());

    public decimal Income => Calculate().Income;
    public decimal Expense => Calculate().Expense;
    public decimal Result => Calculate().Result;
}
