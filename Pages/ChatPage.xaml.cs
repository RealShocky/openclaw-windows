using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace OpenClawGUI.Pages
{
    public partial class ChatPage : UserControl
    {
        public ChatPage()
        {
            InitializeComponent();
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            mainWindow?.SendMessage();
        }

        private void MessageInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.Control)
            {
                var mainWindow = Window.GetWindow(this) as MainWindow;
                mainWindow?.SendMessage();
                e.Handled = true;
            }
        }
    }
}
