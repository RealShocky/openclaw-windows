using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json.Linq;

namespace OpenClawGUI.Dialogs
{
    public class McpServerInfo
    {
        public string Name { get; set; } = "";
        public string Command { get; set; } = "";
    }

    public partial class SkillConfigDialog : Window
    {
        private readonly string _skillType;
        private readonly string _configPath;
        private readonly ObservableCollection<McpServerInfo> _mcpServers = new();

        public SkillConfigDialog(string skillType)
        {
            InitializeComponent();
            _skillType = skillType;
            _configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".openclaw", "openclaw.json");
            CurrentMcpServers.ItemsSource = _mcpServers;
            
            SetupForSkill(skillType);
            LoadExistingConfig();
        }

        private void SetupForSkill(string skill)
        {
            // Hide all field panels first
            McpFields.Visibility = Visibility.Collapsed;
            WebSearchFields.Visibility = Visibility.Collapsed;
            ImageGenFields.Visibility = Visibility.Collapsed;
            AudioFields.Visibility = Visibility.Collapsed;
            WebBrowseFields.Visibility = Visibility.Collapsed;
            AddServerBtn.Visibility = Visibility.Collapsed;

            switch (skill.ToLower())
            {
                case "mcp":
                    Title = "Configure MCP Tools";
                    SkillIcon.Text = "🔌";
                    SkillTitle.Text = "Configure MCP Tools";
                    SkillSubtitle.Text = "Add Model Context Protocol servers";
                    McpFields.Visibility = Visibility.Visible;
                    AddServerBtn.Visibility = Visibility.Visible;
                    break;

                case "web_search":
                    Title = "Configure Web Search";
                    SkillIcon.Text = "🔍";
                    SkillTitle.Text = "Configure Web Search";
                    SkillSubtitle.Text = "Set up search provider and API keys";
                    WebSearchFields.Visibility = Visibility.Visible;
                    break;

                case "image_gen":
                    Title = "Configure Image Generation";
                    SkillIcon.Text = "🎨";
                    SkillTitle.Text = "Configure Image Generation";
                    SkillSubtitle.Text = "Set up AI image generation";
                    ImageGenFields.Visibility = Visibility.Visible;
                    break;

                case "audio":
                    Title = "Configure Audio/Speech";
                    SkillIcon.Text = "🎤";
                    SkillTitle.Text = "Configure Audio/Speech";
                    SkillSubtitle.Text = "Set up speech-to-text and text-to-speech";
                    AudioFields.Visibility = Visibility.Visible;
                    break;

                case "web_browse":
                    Title = "Configure Web Browsing";
                    SkillIcon.Text = "🌐";
                    SkillTitle.Text = "Configure Web Browsing";
                    SkillSubtitle.Text = "Set up headless browser settings";
                    WebBrowseFields.Visibility = Visibility.Visible;
                    break;

                default:
                    Title = $"Configure {skill}";
                    break;
            }
        }

        private void LoadExistingConfig()
        {
            try
            {
                if (!File.Exists(_configPath)) return;

                var json = File.ReadAllText(_configPath);
                var config = JObject.Parse(json);

                switch (_skillType.ToLower())
                {
                    case "mcp":
                        LoadMcpConfig(config);
                        break;
                    case "web_search":
                        LoadWebSearchConfig(config);
                        break;
                    case "image_gen":
                        LoadImageGenConfig(config);
                        break;
                    case "audio":
                        LoadAudioConfig(config);
                        break;
                    case "web_browse":
                        LoadWebBrowseConfig(config);
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading config: {ex.Message}", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void LoadMcpConfig(JObject config)
        {
            _mcpServers.Clear();
            var mcp = config["mcp"] as JObject;
            var servers = mcp?["servers"] as JObject;

            if (servers != null)
            {
                foreach (var server in servers.Properties())
                {
                    var serverConfig = server.Value as JObject;
                    var command = serverConfig?["command"]?.ToString() ?? "";
                    var args = serverConfig?["args"] as JArray;
                    var argsStr = args != null ? string.Join(" ", args.Select(a => a.ToString())) : "";

                    _mcpServers.Add(new McpServerInfo
                    {
                        Name = server.Name,
                        Command = $"{command} {argsStr}".Trim()
                    });
                }
            }

            NoServersText.Visibility = _mcpServers.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void LoadWebSearchConfig(JObject config)
        {
            var skills = config["skills"] as JObject;
            var webSearch = skills?["webSearch"] as JObject;
            if (webSearch != null)
            {
                var provider = webSearch["provider"]?.ToString() ?? "duckduckgo";
                foreach (ComboBoxItem item in SearchProviderCombo.Items)
                {
                    if (item.Tag?.ToString() == provider)
                    {
                        item.IsSelected = true;
                        break;
                    }
                }
                SearchApiKey.Text = webSearch["apiKey"]?.ToString() ?? "";
                SearchEngineId.Text = webSearch["engineId"]?.ToString() ?? "";
            }
        }

        private void LoadImageGenConfig(JObject config)
        {
            var skills = config["skills"] as JObject;
            var imageGen = skills?["imageGeneration"] as JObject;
            if (imageGen != null)
            {
                var provider = imageGen["provider"]?.ToString() ?? "openai";
                foreach (ComboBoxItem item in ImageProviderCombo.Items)
                {
                    if (item.Tag?.ToString() == provider)
                    {
                        item.IsSelected = true;
                        break;
                    }
                }
                ImageApiKey.Text = imageGen["apiKey"]?.ToString() ?? "";
                ImageModel.Text = imageGen["model"]?.ToString() ?? "";
            }
        }

        private void LoadAudioConfig(JObject config)
        {
            var skills = config["skills"] as JObject;
            var audio = skills?["audio"] as JObject;
            if (audio != null)
            {
                var stt = audio["stt"] as JObject;
                if (stt != null)
                {
                    var provider = stt["provider"]?.ToString() ?? "whisper-api";
                    foreach (ComboBoxItem item in SttProviderCombo.Items)
                    {
                        if (item.Tag?.ToString() == provider)
                        {
                            item.IsSelected = true;
                            break;
                        }
                    }
                    SttApiKey.Text = stt["apiKey"]?.ToString() ?? "";
                }

                var tts = audio["tts"] as JObject;
                if (tts != null)
                {
                    var provider = tts["provider"]?.ToString() ?? "openai";
                    foreach (ComboBoxItem item in TtsProviderCombo.Items)
                    {
                        if (item.Tag?.ToString() == provider)
                        {
                            item.IsSelected = true;
                            break;
                        }
                    }
                    TtsApiKey.Text = tts["apiKey"]?.ToString() ?? "";
                    TtsVoiceId.Text = tts["voiceId"]?.ToString() ?? "";
                }
            }
        }

        private void LoadWebBrowseConfig(JObject config)
        {
            var skills = config["skills"] as JObject;
            var webBrowse = skills?["webBrowse"] as JObject;
            if (webBrowse != null)
            {
                var browser = webBrowse["browser"]?.ToString() ?? "chromium";
                foreach (ComboBoxItem item in BrowserEngineCombo.Items)
                {
                    if (item.Tag?.ToString() == browser)
                    {
                        item.IsSelected = true;
                        break;
                    }
                }
                HeadlessCheck.IsChecked = webBrowse["headless"]?.ToObject<bool>() ?? true;
                BrowserTimeout.Text = webBrowse["timeout"]?.ToString() ?? "30000";
                BrowserUserAgent.Text = webBrowse["userAgent"]?.ToString() ?? "";
            }
        }

        private void McpTemplate_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (McpTemplateCombo.SelectedItem is ComboBoxItem item && item.Tag != null)
            {
                var template = item.Tag.ToString();
                ApplyMcpTemplate(template!);
            }
        }

        private void ApplyMcpTemplate(string template)
        {
            switch (template)
            {
                case "filesystem":
                    McpServerName.Text = "filesystem";
                    McpCommand.Text = "npx";
                    McpArgs.Text = "-y @modelcontextprotocol/server-filesystem C:/Users";
                    McpEnvVars.Text = "";
                    break;

                case "github":
                    McpServerName.Text = "github";
                    McpCommand.Text = "npx";
                    McpArgs.Text = "-y @modelcontextprotocol/server-github";
                    McpEnvVars.Text = "GITHUB_PERSONAL_ACCESS_TOKEN=your-github-token";
                    break;

                case "sqlite":
                    McpServerName.Text = "sqlite";
                    McpCommand.Text = "npx";
                    McpArgs.Text = "-y @modelcontextprotocol/server-sqlite --db-path ./database.db";
                    McpEnvVars.Text = "";
                    break;

                case "postgres":
                    McpServerName.Text = "postgres";
                    McpCommand.Text = "npx";
                    McpArgs.Text = "-y @modelcontextprotocol/server-postgres";
                    McpEnvVars.Text = "POSTGRES_CONNECTION_STRING=postgresql://user:pass@localhost:5432/db";
                    break;

                case "slack":
                    McpServerName.Text = "slack";
                    McpCommand.Text = "npx";
                    McpArgs.Text = "-y @modelcontextprotocol/server-slack";
                    McpEnvVars.Text = "SLACK_BOT_TOKEN=xoxb-your-token\nSLACK_TEAM_ID=your-team-id";
                    break;

                case "brave-search":
                    McpServerName.Text = "brave-search";
                    McpCommand.Text = "npx";
                    McpArgs.Text = "-y @modelcontextprotocol/server-brave-search";
                    McpEnvVars.Text = "BRAVE_API_KEY=your-brave-api-key";
                    break;

                case "puppeteer":
                    McpServerName.Text = "puppeteer";
                    McpCommand.Text = "npx";
                    McpArgs.Text = "-y @modelcontextprotocol/server-puppeteer";
                    McpEnvVars.Text = "";
                    break;

                case "memory":
                    McpServerName.Text = "memory";
                    McpCommand.Text = "npx";
                    McpArgs.Text = "-y @modelcontextprotocol/server-memory";
                    McpEnvVars.Text = "";
                    break;

                case "time":
                    McpServerName.Text = "time";
                    McpCommand.Text = "npx";
                    McpArgs.Text = "-y @modelcontextprotocol/server-time";
                    McpEnvVars.Text = "";
                    break;

                case "custom":
                    McpServerName.Text = "";
                    McpCommand.Text = "";
                    McpArgs.Text = "";
                    McpEnvVars.Text = "";
                    break;
            }
        }

        private void AddMcpServer_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(McpServerName.Text))
            {
                MessageBox.Show("Server name is required!", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(McpCommand.Text))
            {
                MessageBox.Show("Command is required!", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Check for duplicate
            if (_mcpServers.Any(s => s.Name == McpServerName.Text.Trim()))
            {
                MessageBox.Show("A server with this name already exists!", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _mcpServers.Add(new McpServerInfo
            {
                Name = McpServerName.Text.Trim(),
                Command = $"{McpCommand.Text.Trim()} {McpArgs.Text.Trim()}".Trim()
            });

            NoServersText.Visibility = Visibility.Collapsed;

            // Clear form for next entry
            McpServerName.Text = "";
            McpCommand.Text = "";
            McpArgs.Text = "";
            McpEnvVars.Text = "";
            McpTemplateCombo.SelectedIndex = 0;

            MessageBox.Show("Server added! Click 'Save & Restart Gateway' to apply.", "Server Added", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void RemoveMcpServer_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string serverName)
            {
                var server = _mcpServers.FirstOrDefault(s => s.Name == serverName);
                if (server != null)
                {
                    _mcpServers.Remove(server);
                    NoServersText.Visibility = _mcpServers.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
                }
            }
        }

        private void SaveAndRestart_Click(object sender, RoutedEventArgs e)
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

                switch (_skillType.ToLower())
                {
                    case "mcp":
                        SaveMcpConfig(config);
                        break;
                    case "web_search":
                        SaveWebSearchConfig(config);
                        break;
                    case "image_gen":
                        SaveImageGenConfig(config);
                        break;
                    case "audio":
                        SaveAudioConfig(config);
                        break;
                    case "web_browse":
                        SaveWebBrowseConfig(config);
                        break;
                }

                File.WriteAllText(_configPath, config.ToString(Newtonsoft.Json.Formatting.Indented));

                var result = MessageBox.Show(
                    "Configuration saved!\n\nRestart the gateway now to apply changes?",
                    "Configuration Saved",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    if (Owner is MainWindow mainWindow)
                    {
                        DialogResult = true;
                        Close();
                        mainWindow.RestartGatewayFromDialog();
                    }
                }
                else
                {
                    DialogResult = true;
                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving configuration: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveMcpConfig(JObject config)
        {
            if (config["mcp"] == null) config["mcp"] = new JObject();
            var mcp = config["mcp"] as JObject;
            mcp!["servers"] = new JObject();
            var servers = mcp["servers"] as JObject;

            foreach (var server in _mcpServers)
            {
                var parts = server.Command.Split(' ', 2);
                var command = parts[0];
                var args = parts.Length > 1 ? parts[1].Split(' ') : Array.Empty<string>();

                var serverConfig = new JObject
                {
                    ["command"] = command,
                    ["args"] = new JArray(args)
                };

                // Add env vars if specified in the form
                if (!string.IsNullOrWhiteSpace(McpEnvVars.Text) && server.Name == McpServerName.Text)
                {
                    var env = new JObject();
                    foreach (var line in McpEnvVars.Text.Split('\n'))
                    {
                        var kv = line.Split('=', 2);
                        if (kv.Length == 2)
                        {
                            env[kv[0].Trim()] = kv[1].Trim();
                        }
                    }
                    if (env.Count > 0)
                    {
                        serverConfig["env"] = env;
                    }
                }

                servers![server.Name] = serverConfig;
            }
        }

        private void SaveWebSearchConfig(JObject config)
        {
            if (config["skills"] == null) config["skills"] = new JObject();
            var skills = config["skills"] as JObject;

            var provider = (SearchProviderCombo.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "duckduckgo";
            
            skills!["webSearch"] = new JObject
            {
                ["enabled"] = true,
                ["provider"] = provider
            };

            if (!string.IsNullOrWhiteSpace(SearchApiKey.Text))
            {
                skills["webSearch"]!["apiKey"] = SearchApiKey.Text.Trim();
            }

            if (!string.IsNullOrWhiteSpace(SearchEngineId.Text))
            {
                skills["webSearch"]!["engineId"] = SearchEngineId.Text.Trim();
            }
        }

        private void SaveImageGenConfig(JObject config)
        {
            if (config["skills"] == null) config["skills"] = new JObject();
            var skills = config["skills"] as JObject;

            var provider = (ImageProviderCombo.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "openai";
            var size = (ImageSizeCombo.SelectedItem as ComboBoxItem)?.Content?.ToString()?.Split(' ')[0] ?? "1024x1024";

            skills!["imageGeneration"] = new JObject
            {
                ["enabled"] = true,
                ["provider"] = provider,
                ["size"] = size
            };

            if (!string.IsNullOrWhiteSpace(ImageApiKey.Text))
            {
                skills["imageGeneration"]!["apiKey"] = ImageApiKey.Text.Trim();
            }

            if (!string.IsNullOrWhiteSpace(ImageModel.Text))
            {
                skills["imageGeneration"]!["model"] = ImageModel.Text.Trim();
            }
        }

        private void SaveAudioConfig(JObject config)
        {
            if (config["skills"] == null) config["skills"] = new JObject();
            var skills = config["skills"] as JObject;

            var sttProvider = (SttProviderCombo.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "whisper-api";
            var ttsProvider = (TtsProviderCombo.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "openai";

            var audioConfig = new JObject
            {
                ["enabled"] = true,
                ["stt"] = new JObject { ["provider"] = sttProvider },
                ["tts"] = new JObject { ["provider"] = ttsProvider }
            };

            if (!string.IsNullOrWhiteSpace(SttApiKey.Text))
            {
                audioConfig["stt"]!["apiKey"] = SttApiKey.Text.Trim();
            }

            if (!string.IsNullOrWhiteSpace(TtsApiKey.Text))
            {
                audioConfig["tts"]!["apiKey"] = TtsApiKey.Text.Trim();
            }

            if (!string.IsNullOrWhiteSpace(TtsVoiceId.Text))
            {
                audioConfig["tts"]!["voiceId"] = TtsVoiceId.Text.Trim();
            }

            skills!["audio"] = audioConfig;
        }

        private void SaveWebBrowseConfig(JObject config)
        {
            if (config["skills"] == null) config["skills"] = new JObject();
            var skills = config["skills"] as JObject;

            var browser = (BrowserEngineCombo.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "chromium";

            skills!["webBrowse"] = new JObject
            {
                ["enabled"] = true,
                ["browser"] = browser,
                ["headless"] = HeadlessCheck.IsChecked ?? true
            };

            if (int.TryParse(BrowserTimeout.Text, out int timeout))
            {
                skills["webBrowse"]!["timeout"] = timeout;
            }

            if (!string.IsNullOrWhiteSpace(BrowserUserAgent.Text))
            {
                skills["webBrowse"]!["userAgent"] = BrowserUserAgent.Text.Trim();
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
