using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Newtonsoft.Json.Linq;
using OpenClawGUI.Dialogs;

namespace OpenClawGUI.Pages
{
    public class CronJob
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Schedule { get; set; } = "";
        public string Description { get; set; } = "";
        public string Status { get; set; } = "IDLE";
        public string StatusColor { get; set; } = "#7D8590";
        public string LastRun { get; set; } = "Never";
        public string NextRun { get; set; } = "Calculating...";
        public string RunCount { get; set; } = "0";
        public bool IsEnabled { get; set; } = true;
        public string EnableText => IsEnabled ? "Enabled" : "Disabled";
    }

    public partial class CronPage : UserControl
    {
        private readonly HttpClient _httpClient = new();
        private readonly ObservableCollection<CronJob> _cronJobs = new();
        private readonly string _configPath;

        public CronPage()
        {
            InitializeComponent();
            _configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".openclaw", "openclaw.json");
            CronJobsList.ItemsSource = _cronJobs;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadCronJobs();
        }

        private async System.Threading.Tasks.Task LoadCronJobs()
        {
            _cronJobs.Clear();

            // First try to load from config file
            LoadFromConfig();

            // Then try to get live status from gateway
            try
            {
                var response = await _httpClient.GetStringAsync("http://127.0.0.1:18789/api/cron");
                var json = JObject.Parse(response);
                var jobs = json["jobs"] as JArray;

                if (jobs != null)
                {
                    // Update status from gateway for existing jobs
                    foreach (var job in jobs)
                    {
                        var id = job["id"]?.ToString();
                        var existing = _cronJobs.FirstOrDefault(j => j.Id == id);
                        if (existing != null)
                        {
                            existing.Status = job["status"]?.ToString()?.ToUpper() ?? "IDLE";
                            existing.StatusColor = GetStatusColor(job["status"]?.ToString());
                            existing.LastRun = job["lastRun"]?.ToString() ?? "Never";
                            existing.NextRun = job["nextRun"]?.ToString() ?? "Calculating...";
                            existing.RunCount = job["runCount"]?.ToString() ?? "0";
                        }
                    }
                }
            }
            catch
            {
                // Gateway not available, just use config data
            }

            EmptyState.Visibility = _cronJobs.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void LoadFromConfig()
        {
            try
            {
                if (!File.Exists(_configPath)) return;

                var json = File.ReadAllText(_configPath);
                var config = JObject.Parse(json);
                var cron = config["cron"] as JObject;
                var tasks = cron?["tasks"] as JArray;

                if (tasks != null)
                {
                    foreach (var task in tasks)
                    {
                        var isEnabled = task["enabled"]?.ToObject<bool>() ?? true;
                        _cronJobs.Add(new CronJob
                        {
                            Id = task["id"]?.ToString() ?? Guid.NewGuid().ToString(),
                            Name = task["name"]?.ToString() ?? "Unnamed Task",
                            Schedule = task["schedule"]?.ToString() ?? "* * * * *",
                            Description = task["description"]?.ToString() ?? "",
                            Status = isEnabled ? "IDLE" : "DISABLED",
                            StatusColor = isEnabled ? "#7D8590" : "#7D8590",
                            LastRun = "Never",
                            NextRun = "Pending...",
                            RunCount = "0",
                            IsEnabled = isEnabled
                        });
                    }
                }
            }
            catch { }
        }

        private string GetStatusColor(string? status)
        {
            return status?.ToLower() switch
            {
                "running" => "#238636",
                "error" or "failed" => "#DA3633",
                "disabled" => "#7D8590",
                _ => "#7D8590"
            };
        }

        private async void Refresh_Click(object sender, RoutedEventArgs e)
        {
            await LoadCronJobs();
        }

        private void NewTask_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            var dialog = new CronConfigDialog()
            {
                Owner = mainWindow
            };
            
            if (dialog.ShowDialog() == true)
            {
                // Reload to show new task
                _cronJobs.Clear();
                LoadFromConfig();
                EmptyState.Visibility = _cronJobs.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void ToggleTask_Click(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleButton toggle && toggle.Tag is string taskId)
            {
                var job = _cronJobs.FirstOrDefault(j => j.Id == taskId);
                if (job != null)
                {
                    job.IsEnabled = toggle.IsChecked ?? false;
                    SaveTaskEnabled(taskId, job.IsEnabled);
                    job.Status = job.IsEnabled ? "IDLE" : "DISABLED";
                }
            }
        }

        private void SaveTaskEnabled(string taskId, bool enabled)
        {
            try
            {
                if (!File.Exists(_configPath)) return;

                var json = File.ReadAllText(_configPath);
                var config = JObject.Parse(json);
                var cron = config["cron"] as JObject;
                var tasks = cron?["tasks"] as JArray;

                if (tasks != null)
                {
                    foreach (var task in tasks)
                    {
                        if (task["id"]?.ToString() == taskId)
                        {
                            task["enabled"] = enabled;
                            break;
                        }
                    }
                    File.WriteAllText(_configPath, config.ToString(Newtonsoft.Json.Formatting.Indented));
                }
            }
            catch { }
        }

        private void RunNow_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string taskId)
            {
                var job = _cronJobs.FirstOrDefault(j => j.Id == taskId);
                if (job != null)
                {
                    job.Status = "RUNNING";
                    job.StatusColor = "#238636";
                    MessageBox.Show($"Task '{job.Name}' triggered!\n\nNote: The gateway must be running to execute scheduled tasks.",
                        "Task Triggered", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void EditTask_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string taskId)
            {
                var mainWindow = Window.GetWindow(this) as MainWindow;
                var dialog = new CronConfigDialog(taskId)
                {
                    Owner = mainWindow
                };
                
                if (dialog.ShowDialog() == true)
                {
                    // Reload to show updated task
                    _cronJobs.Clear();
                    LoadFromConfig();
                    EmptyState.Visibility = _cronJobs.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
                }
            }
        }

        private void DeleteTask_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string taskId)
            {
                var job = _cronJobs.FirstOrDefault(j => j.Id == taskId);
                if (job != null)
                {
                    var result = MessageBox.Show($"Delete task '{job.Name}'?\n\nThis will remove it from your configuration.",
                        "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (result == MessageBoxResult.Yes)
                    {
                        DeleteTaskFromConfig(taskId);
                        _cronJobs.Remove(job);
                        EmptyState.Visibility = _cronJobs.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
                    }
                }
            }
        }

        private void DeleteTaskFromConfig(string taskId)
        {
            try
            {
                if (!File.Exists(_configPath)) return;

                var json = File.ReadAllText(_configPath);
                var config = JObject.Parse(json);
                var cron = config["cron"] as JObject;
                var tasks = cron?["tasks"] as JArray;

                if (tasks != null)
                {
                    for (int i = tasks.Count - 1; i >= 0; i--)
                    {
                        if (tasks[i]["id"]?.ToString() == taskId)
                        {
                            tasks.RemoveAt(i);
                            break;
                        }
                    }
                    File.WriteAllText(_configPath, config.ToString(Newtonsoft.Json.Formatting.Indented));
                }
            }
            catch { }
        }
    }
}
