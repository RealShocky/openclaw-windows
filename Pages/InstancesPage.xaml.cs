using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json.Linq;

namespace OpenClawGUI.Pages
{
    public class ClientInstance
    {
        public string Icon { get; set; } = "🖥️";
        public string Name { get; set; } = "";
        public string Details { get; set; } = "";
        public string InstanceId { get; set; } = "";
        public string Status { get; set; } = "ONLINE";
        public string StatusColor { get; set; } = "#238636";
        public string LastSeen { get; set; } = "";
    }

    public partial class InstancesPage : UserControl
    {
        private readonly HttpClient _httpClient = new();
        private readonly ObservableCollection<ClientInstance> _instances = new();
        private readonly string _thisInstanceId;

        public InstancesPage()
        {
            InitializeComponent();
            InstancesList.ItemsSource = _instances;
            _thisInstanceId = $"gui-windows-{Environment.MachineName.ToLower().Substring(0, Math.Min(5, Environment.MachineName.Length))}";
            ThisInstanceId.Text = $"Instance ID: {_thisInstanceId}";
            ConnectionTime.Text = $"Since {DateTime.Now:HH:mm}";
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadInstances();
        }

        private async System.Threading.Tasks.Task LoadInstances()
        {
            _instances.Clear();

            try
            {
                var response = await _httpClient.GetStringAsync("http://127.0.0.1:18789/api/instances");
                var json = JObject.Parse(response);
                var instances = json["instances"] as JArray;

                if (instances != null)
                {
                    foreach (var instance in instances)
                    {
                        var id = instance["id"]?.ToString() ?? "";
                        if (id == _thisInstanceId) continue; // Skip this instance

                        _instances.Add(new ClientInstance
                        {
                            Icon = GetClientIcon(instance["type"]?.ToString()),
                            Name = instance["name"]?.ToString() ?? "Unknown Client",
                            Details = instance["details"]?.ToString() ?? "",
                            InstanceId = $"Instance ID: {id}",
                            Status = instance["status"]?.ToString()?.ToUpper() ?? "ONLINE",
                            StatusColor = GetStatusColor(instance["status"]?.ToString()),
                            LastSeen = instance["lastSeen"]?.ToString() ?? "Just now"
                        });
                    }
                }
            }
            catch
            {
                LoadSampleInstances();
            }

            UpdateUI();
        }

        private void LoadSampleInstances()
        {
            // No mock data - show empty state
            // Real instances come from gateway API when running
        }

        private string GetClientIcon(string? clientType)
        {
            return clientType?.ToLower() switch
            {
                "windows" => "🖥️",
                "macos" or "mac" => "🍎",
                "ios" => "📱",
                "android" => "🤖",
                "web" => "🌐",
                "cli" => "💻",
                _ => "📟"
            };
        }

        private string GetStatusColor(string? status)
        {
            return status?.ToLower() switch
            {
                "online" or "active" => "#238636",
                "idle" => "#1F6FEB",
                "offline" => "#7D8590",
                _ => "#7D8590"
            };
        }

        private void UpdateUI()
        {
            var totalCount = _instances.Count + 1; // +1 for this instance
            ConnectionCount.Text = $"{totalCount} instance{(totalCount != 1 ? "s" : "")} connected";
            NoOtherInstances.Visibility = _instances.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private async void Refresh_Click(object sender, RoutedEventArgs e)
        {
            await LoadInstances();
        }
    }
}
