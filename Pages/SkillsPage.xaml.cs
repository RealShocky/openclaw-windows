using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json.Linq;
using OpenClawGUI.Dialogs;

namespace OpenClawGUI.Pages
{
    public class SkillInfo
    {
        public string Id { get; set; } = "";
        public string Icon { get; set; } = "";
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public bool IsEnabled { get; set; } = true;
        public string StatusText => IsEnabled ? "ENABLED" : "DISABLED";
        public string StatusColor => IsEnabled ? "#238636" : "#7D8590";
        public bool NeedsConfig { get; set; } = false;
        public Visibility ConfigureVisibility => NeedsConfig ? Visibility.Visible : Visibility.Collapsed;
        public string ConfigKey { get; set; } = "";
    }

    public partial class SkillsPage : UserControl
    {
        private readonly ObservableCollection<SkillInfo> _allSkills = new();
        private readonly ObservableCollection<SkillInfo> _filteredSkills = new();
        private readonly string _configPath;

        public SkillsPage()
        {
            InitializeComponent();
            _configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".openclaw", "openclaw.json");
            SkillsList.ItemsSource = _filteredSkills;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadSkills();
            ApplyFilter();
        }

        private void LoadSkills()
        {
            _allSkills.Clear();

            // Load config to check which skills are enabled
            JObject? config = null;
            try
            {
                if (File.Exists(_configPath))
                {
                    var json = File.ReadAllText(_configPath);
                    config = JObject.Parse(json);
                }
            }
            catch { }

            var commands = config?["commands"] as JObject;
            var native = commands?["native"]?.ToString() ?? "auto";
            var nativeSkills = commands?["nativeSkills"]?.ToString() ?? "auto";

            // Add all available skills
            _allSkills.Add(new SkillInfo
            {
                Id = "file_ops",
                Icon = "📁",
                Name = "File Operations",
                Description = "Read, write, create, delete, and manage files and directories on the system",
                IsEnabled = native == "auto" || native == "true",
                ConfigKey = "commands.native"
            });

            _allSkills.Add(new SkillInfo
            {
                Id = "code_exec",
                Icon = "💻",
                Name = "Code Execution",
                Description = "Execute shell commands, run scripts, and interact with the terminal",
                IsEnabled = native == "auto" || native == "true",
                ConfigKey = "commands.native"
            });

            _allSkills.Add(new SkillInfo
            {
                Id = "web_search",
                Icon = "🔍",
                Name = "Web Search",
                Description = "Search the web using various search engines for information",
                IsEnabled = true,
                NeedsConfig = true,
                ConfigKey = "skills.webSearch"
            });

            _allSkills.Add(new SkillInfo
            {
                Id = "web_browse",
                Icon = "🌐",
                Name = "Web Browsing",
                Description = "Navigate, read, and interact with web pages using a headless browser",
                IsEnabled = true,
                NeedsConfig = true,
                ConfigKey = "skills.webBrowse"
            });

            _allSkills.Add(new SkillInfo
            {
                Id = "memory",
                Icon = "🧠",
                Name = "Memory",
                Description = "Store and recall information across sessions using persistent memory",
                IsEnabled = true,
                ConfigKey = "skills.memory"
            });

            _allSkills.Add(new SkillInfo
            {
                Id = "image_gen",
                Icon = "🎨",
                Name = "Image Generation",
                Description = "Generate images using AI models like DALL-E, Stable Diffusion, or local models",
                IsEnabled = config?["skills"]?["imageGeneration"]?["enabled"]?.ToObject<bool>() ?? false,
                NeedsConfig = true,
                ConfigKey = "skills.imageGeneration"
            });

            _allSkills.Add(new SkillInfo
            {
                Id = "mcp",
                Icon = "🔌",
                Name = "MCP Tools",
                Description = "Model Context Protocol integrations for external tool servers",
                IsEnabled = config?["mcp"] != null,
                NeedsConfig = true,
                ConfigKey = "mcp"
            });

            _allSkills.Add(new SkillInfo
            {
                Id = "git",
                Icon = "📦",
                Name = "Git Operations",
                Description = "Clone, commit, push, pull, and manage Git repositories",
                IsEnabled = nativeSkills == "auto" || nativeSkills == "true",
                ConfigKey = "commands.nativeSkills"
            });

            _allSkills.Add(new SkillInfo
            {
                Id = "vision",
                Icon = "👁️",
                Name = "Vision / Image Analysis",
                Description = "Analyze images and screenshots using vision-capable models",
                IsEnabled = true,
                ConfigKey = "skills.vision"
            });

            _allSkills.Add(new SkillInfo
            {
                Id = "audio",
                Icon = "🎤",
                Name = "Audio / Speech",
                Description = "Speech-to-text and text-to-speech capabilities",
                IsEnabled = config?["skills"]?["audio"]?["enabled"]?.ToObject<bool>() ?? false,
                NeedsConfig = true,
                ConfigKey = "skills.audio"
            });
        }

        private void ApplyFilter()
        {
            _filteredSkills.Clear();
            var searchText = SearchBox.Text?.ToLower() ?? "";

            foreach (var skill in _allSkills)
            {
                if (string.IsNullOrEmpty(searchText) ||
                    skill.Name.ToLower().Contains(searchText) ||
                    skill.Description.ToLower().Contains(searchText))
                {
                    _filteredSkills.Add(skill);
                }
            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadSkills();
            ApplyFilter();
            MessageBox.Show("Skills refreshed from configuration", "Refresh", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsLoaded) ApplyFilter();
        }

        private void ToggleSkill_Click(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.Tag is string skillId)
            {
                var skill = _allSkills.FirstOrDefault(s => s.Id == skillId);
                if (skill != null)
                {
                    skill.IsEnabled = checkBox.IsChecked ?? false;
                    SaveSkillState(skill);
                    ApplyFilter(); // Refresh to update status colors
                }
            }
        }

        private void SaveSkillState(SkillInfo skill)
        {
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

                // Handle different config keys
                switch (skill.Id)
                {
                    case "file_ops":
                    case "code_exec":
                        if (config["commands"] == null) config["commands"] = new JObject();
                        config["commands"]!["native"] = skill.IsEnabled ? "auto" : "false";
                        break;

                    case "git":
                        if (config["commands"] == null) config["commands"] = new JObject();
                        config["commands"]!["nativeSkills"] = skill.IsEnabled ? "auto" : "false";
                        break;

                    case "image_gen":
                        if (config["skills"] == null) config["skills"] = new JObject();
                        if (config["skills"]!["imageGeneration"] == null) config["skills"]!["imageGeneration"] = new JObject();
                        config["skills"]!["imageGeneration"]!["enabled"] = skill.IsEnabled;
                        break;

                    case "audio":
                        if (config["skills"] == null) config["skills"] = new JObject();
                        if (config["skills"]!["audio"] == null) config["skills"]!["audio"] = new JObject();
                        config["skills"]!["audio"]!["enabled"] = skill.IsEnabled;
                        break;
                }

                File.WriteAllText(_configPath, config.ToString(Newtonsoft.Json.Formatting.Indented));

                MessageBox.Show(
                    $"{skill.Name} {(skill.IsEnabled ? "enabled" : "disabled")}.\n\nRestart the gateway for changes to take effect.",
                    "Skill Updated",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving config: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ConfigureSkill_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string skillId)
            {
                var skill = _allSkills.FirstOrDefault(s => s.Id == skillId);
                if (skill == null) return;

                // Open the configuration dialog for configurable skills
                switch (skillId)
                {
                    case "web_search":
                    case "web_browse":
                    case "image_gen":
                    case "mcp":
                    case "audio":
                        OpenConfigDialog(skillId);
                        break;

                    default:
                        MessageBox.Show(
                            $"Configure {skill.Name} in openclaw.json under {skill.ConfigKey}",
                            $"Configure {skill.Name}",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                        break;
                }
            }
        }

        private void OpenConfigDialog(string skillId)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            var dialog = new SkillConfigDialog(skillId)
            {
                Owner = mainWindow
            };
            
            if (dialog.ShowDialog() == true)
            {
                // Reload skills to reflect any changes
                LoadSkills();
                ApplyFilter();
            }
        }
    }
}
