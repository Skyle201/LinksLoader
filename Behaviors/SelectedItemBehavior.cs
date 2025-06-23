using System.Windows;
using System.Windows.Controls;

namespace LinksLoader.Behaviors
{
    public static class TreeViewSelectedItemBehavior
    {
        public static readonly DependencyProperty BindableSelectedItemProperty =
            DependencyProperty.RegisterAttached("BindableSelectedItem",
                typeof(object),
                typeof(TreeViewSelectedItemBehavior),
                new UIPropertyMetadata(null, OnBindableSelectedItemChanged));

        public static object GetBindableSelectedItem(DependencyObject obj) =>
            obj.GetValue(BindableSelectedItemProperty);

        public static void SetBindableSelectedItem(DependencyObject obj, object value) =>
            obj.SetValue(BindableSelectedItemProperty, value);

        private static void OnBindableSelectedItemChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is TreeView treeView)
            {
                treeView.SelectedItemChanged -= TreeView_SelectedItemChanged;
                treeView.SelectedItemChanged += TreeView_SelectedItemChanged;
            }
        }

        private static void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var treeView = sender as TreeView;
            SetBindableSelectedItem(treeView, e.NewValue);
        }
    }
}
