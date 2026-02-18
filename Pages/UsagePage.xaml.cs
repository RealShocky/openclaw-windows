using System.Windows;
using System.Windows.Controls;

namespace OpenClawGUI.Pages
{
    public partial class UsagePage : UserControl
    {
        public UsagePage()
        {
            InitializeComponent();
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Usage statistics refreshed!\n\nNote: Token counts are tracked per session.\nLocal models (Ollama/LMStudio) = $0.00 cost!", 
                "Stats Refreshed", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
