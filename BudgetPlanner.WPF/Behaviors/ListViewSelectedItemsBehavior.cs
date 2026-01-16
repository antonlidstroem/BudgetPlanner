using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace BudgetPlanner.WPF.Behaviors
{
    public static class ListViewSelectedItemsBehavior
    {
        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.RegisterAttached(
                "SelectedItems",
                typeof(IList),
                typeof(ListViewSelectedItemsBehavior),
                new PropertyMetadata(null, OnSelectedItemsChanged));

        public static void SetSelectedItems(DependencyObject element, IList value)
            => element.SetValue(SelectedItemsProperty, value);

        public static IList GetSelectedItems(DependencyObject element)
            => (IList)element.GetValue(SelectedItemsProperty);

        private static void OnSelectedItemsChanged(
            DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ListView listView)
            {
                listView.SelectionChanged += (_, __) =>
                {
                    if (GetSelectedItems(listView) is IList target)
                    {
                        target.Clear();
                        foreach (var item in listView.SelectedItems)
                            target.Add(item);
                    }
                };
            }
        }
    }
}
