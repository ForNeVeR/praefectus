using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Taskomatic.Core;

namespace Taskomatic.Gui
{
    public class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            App.AttachDevTools(this);
        }

        private async void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            var config = await ConfigService.LoadConfig();
            DataContext = new ApplicationViewModel(config);
        }
    }
}
