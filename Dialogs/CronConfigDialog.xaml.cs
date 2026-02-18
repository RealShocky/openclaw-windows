using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json.Linq;

namespace OpenClawGUI.Dialogs
{
    public partial class CronConfigDialog : Window
    {
        private readonly string _configPath;
        private readonly string? _editingTaskId;

        public CronConfigDialog(string? editTaskId = null)
        {
            InitializeComponent();
            _configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".openclaw", "openclaw.json");
            _editingTaskId = editTaskId;

            if (editTaskId != null)
            {
                DialogTitle.Text = "Edit Scheduled Task";
                LoadExistingTask(editTaskId);
            }
        }

        private void LoadExistingTask(string taskId)
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
                            TaskNameInput.Text = task["name"]?.ToString() ?? "";
                            TaskDescInput.Text = task["description"]?.ToString() ?? "";
                            CronExpressionInput.Text = task["schedule"]?.ToString() ?? "";
                            ActionInput.Text = task["action"]?.ToString() ?? "";
                            EnabledCheck.IsChecked = task["enabled"]?.ToObject<bool>() ?? true;

                            var actionType = task["actionType"]?.ToString() ?? "agent";
                            foreach (ComboBoxItem item in ActionTypeCombo.Items)
                            {
                                if (item.Tag?.ToString() == actionType)
                                {
                                    item.IsSelected = true;
                                    break;
                                }
                            }
                            break;
                        }
                    }
                }
            }
            catch { }
        }

        private void SchedulePreset_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (SchedulePresetCombo.SelectedItem is ComboBoxItem item && item.Tag != null)
            {
                var tag = item.Tag.ToString();
                if (tag != "custom" && !string.IsNullOrEmpty(tag))
                {
                    CronExpressionInput.Text = tag;
                }
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(TaskNameInput.Text))
            {
                MessageBox.Show("Task name is required!", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(CronExpressionInput.Text))
            {
                MessageBox.Show("Cron expression is required!", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(ActionInput.Text))
            {
                MessageBox.Show("Action/command is required!", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                JObject config;
                if (File.Exists(_configPath))
                {
                    var json = File.ReadAllText(_configPath);
                    config = JObject.Parse(json);
                }
                else
                {
                    config = new JObject();
                }

                // Ensure cron section exists
                if (config["cron"] == null) config["cron"] = new JObject();
                var cron = config["cron"] as JObject;
                if (cron!["tasks"] == null) cron["tasks"] = new JArray();
                var tasks = cron["tasks"] as JArray;

                var actionType = (ActionTypeCombo.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "agent";

                var taskObj = new JObject
                {
                    ["id"] = _editingTaskId ?? Guid.NewGuid().ToString("N").Substring(0, 8),
                    ["name"] = TaskNameInput.Text.Trim(),
                    ["description"] = TaskDescInput.Text.Trim(),
                    ["schedule"] = CronExpressionInput.Text.Trim(),
                    ["actionType"] = actionType,
                    ["action"] = ActionInput.Text.Trim(),
                    ["enabled"] = EnabledCheck.IsChecked ?? true
                };

                if (_editingTaskId != null)
                {
                    // Update existing task
                    for (int i = 0; i < tasks!.Count; i++)
                    {
                        if (tasks[i]["id"]?.ToString() == _editingTaskId)
                        {
                            tasks[i] = taskObj;
                            break;
                        }
                    }
                }
                else
                {
                    // Add new task
                    tasks!.Add(taskObj);
                }

                File.WriteAllText(_configPath, config.ToString(Newtonsoft.Json.Formatting.Indented));

                MessageBox.Show(
                    $"Task '{TaskNameInput.Text}' saved!\n\nThe task will run according to the schedule:\n{CronExpressionInput.Text}",
                    "Task Saved",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving task: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
