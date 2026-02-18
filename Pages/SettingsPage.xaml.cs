using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace OpenClawGUI.Pages
{
    public partial class SettingsPage : UserControl
    {
        public SettingsPage()
        {
            InitializeComponent();
        }

        private void SaveGateway_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            mainWindow?.SaveGatewaySettings(GatewayUrlInput.Text, AuthTokenInput.Text);
            MessageBox.Show("Gateway settings saved!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SavePaths_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            mainWindow?.SavePathSettings(OpenClawPathInput.Text, PnpmPathInput.Text);
            MessageBox.Show("Path settings saved!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BrowseOpenClaw_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFolderDialog
            {
                Title = "Select OpenClaw Directory"
            };
            if (dialog.ShowDialog() == true)
            {
                OpenClawPathInput.Text = dialog.FolderName;
            }
        }

        private void BrowsePnpm_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Select pnpm.cmd",
                Filter = "Command files (*.cmd)|*.cmd|All files (*.*)|*.*"
            };
            if (dialog.ShowDialog() == true)
            {
                PnpmPathInput.Text = dialog.FileName;
            }
        }

        private void OpenConfig_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            mainWindow?.OpenConfigFile();
        }

        private void OpenWebUI_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            mainWindow?.OpenWebUI();
        }
    }
}
