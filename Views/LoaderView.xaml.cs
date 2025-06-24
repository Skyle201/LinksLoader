using System.Windows;
using LinksLoader.ViewModels;

namespace LinksLoader
{
    public partial class LoaderView : Window
    {
        public LoaderView()
        {
            InitializeComponent();

            this.DataContext = new WindowViewModel();
        }

        public WindowViewModel ViewModel => this.DataContext as WindowViewModel;

        private void LoadSelected_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
    }
}
