using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Taskomatic.Core;

namespace Taskomatic.Gui
{
    public class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
            App.AttachDevTools(this);
            DataContext = new IssueListViewModel();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
