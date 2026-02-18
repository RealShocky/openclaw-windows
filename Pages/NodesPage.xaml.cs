using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json.Linq;

namespace OpenClawGUI.Pages
{
    public class ExecutionNode
    {
        public string Id { get; set; } = "";
        public string Icon { get; set; } = "🔧";
        public string ToolName { get; set; } = "";
        public string Description { get; set; } = "";
        public string Command { get; set; } = "";
        public string Status { get; set; } = "PENDING";
        public string StatusColor { get; set; } = "#D29922";
        public string StatusIcon { get; set; } = "⏳";
        public string Time { get; set; } = "";
    }

    public partial class NodesPage : UserControl
    {
        private readonly HttpClient _httpClient = new();
        private readonly ObservableCollection<ExecutionNode> _pendingNodes = new();
        private readonly ObservableCollection<ExecutionNode> _recentNodes = new();

        public NodesPage()
        {
            InitializeComponent();
            PendingList.ItemsSource = _pendingNodes;
            RecentList.ItemsSource = _recentNodes;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadNodes();
        }

        private async System.Threading.Tasks.Task LoadNodes()
        {
            _pendingNodes.Clear();
            _recentNodes.Clear();

            try
            {
                var response = await _httpClient.GetStringAsync("http://127.0.0.1:18789/api/nodes");
                var json = JObject.Parse(response);
                
                var pending = json["pending"] as JArray;
                if (pending != null)
                {
                    foreach (var node in pending)
                    {
                        _pendingNodes.Add(new ExecutionNode
                        {
                            Id = node["id"]?.ToString() ?? Guid.NewGuid().ToString(),
                            Icon = GetToolIcon(node["tool"]?.ToString()),
                            ToolName = node["tool"]?.ToString() ?? "Unknown Tool",
                            Description = node["description"]?.ToString() ?? "",
                            Command = node["command"]?.ToString() ?? "",
                            Status = "PENDING",
                            StatusColor = "#D29922"
                        });
                    }
                }
            }
            catch
            {
                LoadSampleData();
            }

            UpdateUI();
        }

        private void LoadSampleData()
        {
            // No mock data - show empty state
            // Real pending approvals come from the gateway API when running
        }

        private string GetToolIcon(string? tool)
        {
            return tool?.ToLower() switch
            {
                "shell" or "command" or "bash" => "💻",
                "file" or "read" or "write" => "📁",
                "web" or "search" or "browse" => "🌐",
                "git" => "📦",
                "memory" => "🧠",
                _ => "🔧"
            };
        }

        private void UpdateUI()
        {
            PendingCount.Text = _pendingNodes.Count.ToString();
            NoPendingText.Visibility = _pendingNodes.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private async void Refresh_Click(object sender, RoutedEventArgs e)
        {
            await LoadNodes();
        }

        private void Approve_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string nodeId)
            {
                var node = _pendingNodes.FirstOrDefault(n => n.Id == nodeId);
                if (node != null)
                {
                    _pendingNodes.Remove(node);
                    node.Status = "APPROVED";
                    node.StatusColor = "#238636";
                    node.StatusIcon = "✅";
                    node.Time = "Just now";
                    _recentNodes.Insert(0, node);
                    UpdateUI();
                }
            }
        }

        private void Deny_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string nodeId)
            {
                var node = _pendingNodes.FirstOrDefault(n => n.Id == nodeId);
                if (node != null)
                {
                    _pendingNodes.Remove(node);
                    node.Status = "DENIED";
                    node.StatusColor = "#DA3633";
                    node.StatusIcon = "❌";
                    node.Time = "Just now";
                    _recentNodes.Insert(0, node);
                    UpdateUI();
                }
            }
        }

        private void ApproveAll_Click(object sender, RoutedEventArgs e)
        {
            var toApprove = _pendingNodes.ToList();
            foreach (var node in toApprove)
            {
                _pendingNodes.Remove(node);
                node.Status = "APPROVED";
                node.StatusColor = "#238636";
                node.StatusIcon = "✅";
                node.Time = "Just now";
                _recentNodes.Insert(0, node);
            }
            UpdateUI();
            if (toApprove.Count > 0)
            {
                MessageBox.Show($"Approved {toApprove.Count} execution(s)", "Approved", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void DenyAll_Click(object sender, RoutedEventArgs e)
        {
            var toDeny = _pendingNodes.ToList();
            foreach (var node in toDeny)
            {
                _pendingNodes.Remove(node);
                node.Status = "DENIED";
                node.StatusColor = "#DA3633";
                node.StatusIcon = "❌";
                node.Time = "Just now";
                _recentNodes.Insert(0, node);
            }
            UpdateUI();
            if (toDeny.Count > 0)
            {
                MessageBox.Show($"Denied {toDeny.Count} execution(s)", "Denied", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
