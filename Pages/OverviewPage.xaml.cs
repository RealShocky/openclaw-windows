using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json.Linq;
using Path = System.IO.Path;
using File = System.IO.File;

namespace OpenClawGUI.Pages
{
    public class ActivityItem
    {
        public string Icon { get; set; } = "";
        public string Message { get; set; } = "";
        public string Time { get; set; } = "";
    }

    public partial class OverviewPage : UserControl
    {
        private readonly HttpClient _httpClient = new();
        private readonly ObservableCollection<ActivityItem> _activities = new();
        private readonly string _configPath;

        public OverviewPage()
        {
            InitializeComponent();
            _configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".openclaw", "openclaw.json");
            RecentActivity.ItemsSource = _activities;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await RefreshAllStatus();
            LoadConfigInfo();
        }

        private async System.Threading.Tasks.Task RefreshAllStatus()
        {
            // Check Gateway
            try
            {
                var response = await _httpClient.GetAsync("http://127.0.0.1:18789/health");
                if (response.IsSuccessStatusCode)
                {
                    GatewayStatus.Text = "ONLINE";
                    GatewayStatus.Foreground = new System.Windows.Media.SolidColorBrush(
                        (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#238636"));
                    AddActivity("🌐", "Gateway is online", DateTime.Now.ToString("HH:mm:ss"));
                }
                else
                {
                    GatewayStatus.Text = "OFFLINE";
                    GatewayStatus.Foreground = new System.Windows.Media.SolidColorBrush(
                        (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#DA3633"));
                }
            }
            catch
            {
                GatewayStatus.Text = "OFFLINE";
                GatewayStatus.Foreground = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#DA3633"));
            }

            // Check Ollama
            try
            {
                var response = await _httpClient.GetStringAsync("http://127.0.0.1:11434/api/tags");
                var json = JObject.Parse(response);
                var models = json["models"] as JArray;
                var count = models?.Count ?? 0;
                
                OllamaStatus.Text = "ONLINE";
                OllamaStatus.Foreground = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#238636"));
                OllamaModels.Text = $"{count} model{(count != 1 ? "s" : "")}";
                AddActivity("🦙", $"Ollama running with {count} models", DateTime.Now.ToString("HH:mm:ss"));
            }
            catch
            {
                OllamaStatus.Text = "OFFLINE";
                OllamaStatus.Foreground = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#DA3633"));
                OllamaModels.Text = "Not running";
            }

            // Check LMStudio
            try
            {
                var response = await _httpClient.GetStringAsync("http://127.0.0.1:1234/v1/models");
                var json = JObject.Parse(response);
                var models = json["data"] as JArray;
                var count = models?.Count ?? 0;
                
                LMStudioStatus.Text = "ONLINE";
                LMStudioStatus.Foreground = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#238636"));
                LMStudioModels.Text = $"{count} model{(count != 1 ? "s" : "")}";
                AddActivity("🧠", $"LMStudio running with {count} models", DateTime.Now.ToString("HH:mm:ss"));
            }
            catch
            {
                LMStudioStatus.Text = "OFFLINE";
                LMStudioStatus.Foreground = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#7D8590"));
                LMStudioModels.Text = "Not running";
            }

            NoActivityText.Visibility = _activities.Count > 0 ? Visibility.Collapsed : Visibility.Visible;
        }

        private void LoadConfigInfo()
        {
            try
            {
                ConfigPath.Text = _configPath;
                OpenClawPath.Text = @"P:\jarvis\openclaw";

                if (File.Exists(_configPath))
                {
                    var json = File.ReadAllText(_configPath);
                    var config = JObject.Parse(json);

                    var primary = config["agents"]?["defaults"]?["model"]?["primary"]?.ToString();
                    var fallback = config["agents"]?["defaults"]?["model"]?["fallback"]?[0]?.ToString();
                    var port = config["gateway"]?["port"]?.ToString() ?? "18789";

                    PrimaryModel.Text = primary ?? "Not configured";
                    FallbackModel.Text = fallback ?? "None";
                    GatewayPort.Text = port;
                    GatewayUrl.Text = $"http://127.0.0.1:{port}";
                }
            }
            catch (Exception ex)
            {
                PrimaryModel.Text = $"Error: {ex.Message}";
            }
        }

        private void AddActivity(string icon, string message, string time)
        {
            _activities.Insert(0, new ActivityItem { Icon = icon, Message = message, Time = time });
            if (_activities.Count > 10) _activities.RemoveAt(_activities.Count - 1);
        }

        private async void RefreshStatus_Click(object sender, RoutedEventArgs e)
        {
            _activities.Clear();
            AddActivity("🔄", "Refreshing status...", DateTime.Now.ToString("HH:mm:ss"));
            await RefreshAllStatus();
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

        private void OpenDataFolder_Click(object sender, RoutedEventArgs e)
        {
            var dataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".openclaw");
            Process.Start(new ProcessStartInfo(dataPath) { UseShellExecute = true });
        }
    }
}
