using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Taskomatic.Gui
{
    public class IssueDetailsView : UserControl
    {
        public IssueDetailsView()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
