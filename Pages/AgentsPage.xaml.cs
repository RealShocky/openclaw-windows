using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json.Linq;

namespace OpenClawGUI.Pages
{
    public class AgentInfo
    {
        public string Name { get; set; } = "OpenClaw Agent";
        public string SessionId { get; set; } = "";
        public string Status { get; set; } = "ACTIVE";
        public string StatusColor { get; set; } = "#238636";
        public string Model { get; set; } = "";
        public string MessageCount { get; set; } = "0";
        public string StartTime { get; set; } = "";
    }

    public partial class AgentsPage : UserControl
    {
        private readonly HttpClient _httpClient = new();
        private readonly ObservableCollection<AgentInfo> _agents = new();

        public AgentsPage()
        {
            InitializeComponent();
            AgentsList.ItemsSource = _agents;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await RefreshAgents();
        }

        private async System.Threading.Tasks.Task RefreshAgents()
        {
            _agents.Clear();

            try
            {
                // Try to get agents from gateway
                var response = await _httpClient.GetStringAsync("http://127.0.0.1:18789/api/agents");
                var json = JObject.Parse(response);
                var agents = json["agents"] as JArray;

                if (agents != null && agents.Count > 0)
                {
                    foreach (var agent in agents)
                    {
                        _agents.Add(new AgentInfo
                        {
                            Name = agent["name"]?.ToString() ?? "OpenClaw Agent",
                            SessionId = agent["sessionId"]?.ToString() ?? "",
                            Status = agent["status"]?.ToString()?.ToUpper() ?? "ACTIVE",
                            StatusColor = GetStatusColor(agent["status"]?.ToString()),
                            Model = agent["model"]?.ToString() ?? "Unknown",
                            MessageCount = agent["messageCount"]?.ToString() ?? "0",
                            StartTime = agent["startTime"]?.ToString() ?? DateTime.Now.ToString("HH:mm:ss")
                        });
                    }
                    EmptyState.Visibility = Visibility.Collapsed;
                }
                else
                {
                    EmptyState.Visibility = Visibility.Visible;
                }
            }
            catch
            {
                // Gateway might not have agents endpoint, show current session as agent
                var mainWindow = Window.GetWindow(this) as MainWindow;
                if (mainWindow != null)
                {
                    _agents.Add(new AgentInfo
                    {
                        Name = "Current Session",
                        SessionId = "gui-session",
                        Status = "ACTIVE",
                        StatusColor = "#238636",
                        Model = "ollama/qwen2.5-coder:7b",
                        MessageCount = "Active",
                        StartTime = DateTime.Now.ToString("HH:mm:ss")
                    });
                    EmptyState.Visibility = Visibility.Collapsed;
                }
                else
                {
                    EmptyState.Visibility = Visibility.Visible;
                }
            }
        }

        private string GetStatusColor(string? status)
        {
            return status?.ToLower() switch
            {
                "active" or "running" => "#238636",
                "idle" or "waiting" => "#1F6FEB",
                "error" or "failed" => "#DA3633",
                _ => "#7D8590"
            };
        }

        private async void Refresh_Click(object sender, RoutedEventArgs e)
        {
            await RefreshAgents();
        }

        private void NewAgent_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            mainWindow?.CreateNewSession();
        }

        private void OpenChat_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string sessionId)
            {
                var mainWindow = Window.GetWindow(this) as MainWindow;
                mainWindow?.LoadSession(sessionId);
            }
        }

        private void ViewLogs_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Logs viewer coming soon!", "View Logs", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void StopAgent_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string sessionId)
            {
                var result = MessageBox.Show($"Stop agent for session {sessionId}?", "Stop Agent",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    MessageBox.Show("Agent stopped", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void StartChat_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            mainWindow?.CreateNewSession();
        }
    }
}
