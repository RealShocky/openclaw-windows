using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json.Linq;

namespace OpenClawGUI.Pages
{
    public class SessionInfo
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Created { get; set; } = "";
        public string MessageCount { get; set; } = "";
        public string Model { get; set; } = "";
    }

    public partial class SessionsPage : UserControl
    {
        private readonly ObservableCollection<SessionInfo> _sessions = new();
        private readonly string _sessionsPath;

        public SessionsPage()
        {
            InitializeComponent();
            _sessionsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".openclaw", "sessions");
            SessionsList.ItemsSource = _sessions;
            Loaded += Page_Loaded;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadSessions();
        }

        private void LoadSessions()
        {
            _sessions.Clear();

            try
            {
                if (Directory.Exists(_sessionsPath))
                {
                    var sessionDirs = Directory.GetDirectories(_sessionsPath)
                        .OrderByDescending(d => Directory.GetCreationTime(d))
                        .Take(20);

                    foreach (var dir in sessionDirs)
                    {
                        var sessionId = Path.GetFileName(dir);
                        var messagesFile = Path.Combine(dir, "messages.json");
                        var messageCount = 0;
                        var model = "Unknown";
                        var created = Directory.GetCreationTime(dir);

                        if (File.Exists(messagesFile))
                        {
                            try
                            {
                                var json = File.ReadAllText(messagesFile);
                                var messages = JArray.Parse(json);
                                messageCount = messages.Count;
                                
                                var firstMsg = messages.FirstOrDefault(m => m["model"] != null);
                                if (firstMsg != null)
                                {
                                    model = firstMsg["model"]?.ToString() ?? "Unknown";
                                }
                            }
                            catch { }
                        }

                        _sessions.Add(new SessionInfo
                        {
                            Id = sessionId,
                            Name = sessionId,
                            Created = $"Created: {created:yyyy-MM-dd HH:mm}",
                            MessageCount = $"{messageCount} messages • {model}",
                            Model = model
                        });
                    }
                }

                // Always show current GUI session
                if (!_sessions.Any(s => s.Id == "gui-session"))
                {
                    _sessions.Insert(0, new SessionInfo
                    {
                        Id = "gui-session",
                        Name = "Current Session",
                        Created = $"Created: {DateTime.Now:yyyy-MM-dd HH:mm}",
                        MessageCount = "Active session",
                        Model = "Active"
                    });
                }
            }
            catch (Exception ex)
            {
                _sessions.Add(new SessionInfo
                {
                    Id = "error",
                    Name = "Error loading sessions",
                    Created = ex.Message,
                    MessageCount = "",
                    Model = ""
                });
            }
        }

        private void NewSession_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            mainWindow?.CreateNewSession();
            LoadSessions();
        }

        private void LoadSession_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string sessionId)
            {
                var mainWindow = Window.GetWindow(this) as MainWindow;
                mainWindow?.LoadSession(sessionId);
            }
        }

        private void DeleteSession_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string sessionId)
            {
                if (sessionId == "gui-session")
                {
                    MessageBox.Show("Cannot delete the current active session.", "Cannot Delete", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var result = MessageBox.Show($"Delete session '{sessionId}'?\n\nThis will permanently delete all messages in this session.", 
                    "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        var sessionDir = Path.Combine(_sessionsPath, sessionId);
                        if (Directory.Exists(sessionDir))
                        {
                            Directory.Delete(sessionDir, true);
                        }
                        LoadSessions();
                        MessageBox.Show("Session deleted.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting session: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadSessions();
        }
    }
}
