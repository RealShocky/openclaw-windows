using System.Windows;
using System.Windows.Controls;
using OpenClawGUI.Dialogs;

namespace OpenClawGUI.Pages
{
    public partial class ChannelsPage : UserControl
    {
        public ChannelsPage()
        {
            InitializeComponent();
        }

        private void OpenConfigDialog(string channelType)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            var dialog = new ChannelConfigDialog(channelType)
            {
                Owner = mainWindow
            };
            dialog.ShowDialog();
        }

        private void ConfigureDiscord_Click(object sender, RoutedEventArgs e)
        {
            OpenConfigDialog("discord");
        }

        private void ConfigureSlack_Click(object sender, RoutedEventArgs e)
        {
            OpenConfigDialog("slack");
        }

        private void ConfigureTelegram_Click(object sender, RoutedEventArgs e)
        {
            OpenConfigDialog("telegram");
        }

        private void ConfigureWhatsApp_Click(object sender, RoutedEventArgs e)
        {
            OpenConfigDialog("whatsapp");
        }

        private void ConfigureSignal_Click(object sender, RoutedEventArgs e)
        {
            OpenConfigDialog("signal");
        }

        private void ConfigureNostr_Click(object sender, RoutedEventArgs e)
        {
            OpenConfigDialog("nostr");
        }

        private void ConfigureGoogleChat_Click(object sender, RoutedEventArgs e)
        {
            OpenConfigDialog("googlechat");
        }
    }
}
