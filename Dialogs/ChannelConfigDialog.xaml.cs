using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using Newtonsoft.Json.Linq;

namespace OpenClawGUI.Dialogs
{
    public partial class ChannelConfigDialog : Window
    {
        private readonly string _channelType;
        private readonly string _configPath;
        private string _helpUrl = "https://discord.com/developers/applications";

        public ChannelConfigDialog(string channelType)
        {
            InitializeComponent();
            _channelType = channelType;
            _configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".openclaw", "openclaw.json");
            
            SetupForChannel(channelType);
            LoadExistingConfig();
        }

        private void SetupForChannel(string channel)
        {
            // Hide all field panels first
            DiscordFields.Visibility = Visibility.Collapsed;
            SlackFields.Visibility = Visibility.Collapsed;
            TelegramFields.Visibility = Visibility.Collapsed;
            GenericFields.Visibility = Visibility.Collapsed;

            switch (channel.ToLower())
            {
                case "discord":
                    Title = "Configure Discord";
                    ChannelIcon.Text = "💬";
                    ChannelTitle.Text = "Configure Discord";
                    ChannelSubtitle.Text = "Connect OpenClaw to your Discord server";
                    DiscordFields.Visibility = Visibility.Visible;
                    HelpText.Text = "Create a bot at Discord Developer Portal, enable Message Content Intent, and invite it to your server.";
                    _helpUrl = "https://discord.com/developers/applications";
                    break;

                case "slack":
                    Title = "Configure Slack";
                    ChannelIcon.Text = "📱";
                    ChannelTitle.Text = "Configure Slack";
                    ChannelSubtitle.Text = "Connect OpenClaw to your Slack workspace";
                    SlackFields.Visibility = Visibility.Visible;
                    HelpText.Text = "Create a Slack app, add bot scopes, and install to your workspace.";
                    _helpUrl = "https://api.slack.com/apps";
                    break;

                case "telegram":
                    Title = "Configure Telegram";
                    ChannelIcon.Text = "✈️";
                    ChannelTitle.Text = "Configure Telegram";
                    ChannelSubtitle.Text = "Connect OpenClaw to Telegram";
                    TelegramFields.Visibility = Visibility.Visible;
                    HelpText.Text = "Message @BotFather on Telegram to create a bot and get your token.";
                    _helpUrl = "https://t.me/BotFather";
                    break;

                case "whatsapp":
                    Title = "Configure WhatsApp";
                    ChannelIcon.Text = "📞";
                    ChannelTitle.Text = "Configure WhatsApp";
                    ChannelSubtitle.Text = "Connect OpenClaw to WhatsApp Business";
                    GenericFields.Visibility = Visibility.Visible;
                    HelpText.Text = "Set up WhatsApp Business API through Meta for Developers.";
                    _helpUrl = "https://developers.facebook.com/docs/whatsapp/cloud-api/get-started";
                    break;

                case "signal":
                    Title = "Configure Signal";
                    ChannelIcon.Text = "🔒";
                    ChannelTitle.Text = "Configure Signal";
                    ChannelSubtitle.Text = "Connect OpenClaw to Signal";
                    GenericFields.Visibility = Visibility.Visible;
                    HelpText.Text = "Signal requires signal-cli to be installed and configured.";
                    _helpUrl = "https://github.com/AsamK/signal-cli";
                    break;

                case "nostr":
                    Title = "Configure Nostr";
                    ChannelIcon.Text = "⚡";
                    ChannelTitle.Text = "Configure Nostr";
                    ChannelSubtitle.Text = "Connect OpenClaw to Nostr";
                    GenericFields.Visibility = Visibility.Visible;
                    HelpText.Text = "Enter your nsec (private key) and relay URLs.";
                    _helpUrl = "https://nostr.com/";
                    break;

                case "googlechat":
                    Title = "Configure Google Chat";
                    ChannelIcon.Text = "🟢";
                    ChannelTitle.Text = "Configure Google Chat";
                    ChannelSubtitle.Text = "Connect OpenClaw to Google Chat";
                    GenericFields.Visibility = Visibility.Visible;
                    HelpText.Text = "Create a Google Cloud project and enable the Chat API.";
                    _helpUrl = "https://console.cloud.google.com/";
                    break;

                default:
                    Title = $"Configure {channel}";
                    GenericFields.Visibility = Visibility.Visible;
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
                var channels = config["channels"] as JObject;
                if (channels == null) return;

                var channelConfig = channels[_channelType.ToLower()] as JObject;
                if (channelConfig == null) return;

                switch (_channelType.ToLower())
                {
                    case "discord":
                        BotTokenInput.Text = channelConfig["token"]?.ToString() ?? "";
                        AppIdInput.Text = channelConfig["applicationId"]?.ToString() ?? "";
                        GuildIdInput.Text = channelConfig["guildId"]?.ToString() ?? "";
                        break;

                    case "slack":
                        SlackBotTokenInput.Text = channelConfig["botToken"]?.ToString() ?? "";
                        SlackSigningSecretInput.Text = channelConfig["signingSecret"]?.ToString() ?? "";
                        SlackAppTokenInput.Text = channelConfig["appToken"]?.ToString() ?? "";
                        break;

                    case "telegram":
                        TelegramTokenInput.Text = channelConfig["token"]?.ToString() ?? "";
                        var allowedUsers = channelConfig["allowedUsers"] as JArray;
                        if (allowedUsers != null)
                        {
                            TelegramAllowedUsersInput.Text = string.Join(", ", allowedUsers);
                        }
                        break;

                    default:
                        GenericTokenInput.Text = channelConfig["token"]?.ToString() ?? "";
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading existing config: {ex.Message}", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void SaveAndRestart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Load existing config
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

                // Ensure channels section exists
                if (config["channels"] == null)
                {
                    config["channels"] = new JObject();
                }
                var channels = config["channels"] as JObject;

                // Build channel config based on type
                JObject channelConfig = new JObject();

                switch (_channelType.ToLower())
                {
                    case "discord":
                        if (string.IsNullOrWhiteSpace(BotTokenInput.Text))
                        {
                            MessageBox.Show("Bot Token is required!", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                        channelConfig["enabled"] = true;
                        channelConfig["token"] = BotTokenInput.Text.Trim();
                        if (!string.IsNullOrWhiteSpace(AppIdInput.Text))
                            channelConfig["applicationId"] = AppIdInput.Text.Trim();
                        if (!string.IsNullOrWhiteSpace(GuildIdInput.Text))
                            channelConfig["guildId"] = GuildIdInput.Text.Trim();
                        break;

                    case "slack":
                        if (string.IsNullOrWhiteSpace(SlackBotTokenInput.Text) || string.IsNullOrWhiteSpace(SlackSigningSecretInput.Text))
                        {
                            MessageBox.Show("Bot Token and Signing Secret are required!", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                        channelConfig["enabled"] = true;
                        channelConfig["botToken"] = SlackBotTokenInput.Text.Trim();
                        channelConfig["signingSecret"] = SlackSigningSecretInput.Text.Trim();
                        if (!string.IsNullOrWhiteSpace(SlackAppTokenInput.Text))
                            channelConfig["appToken"] = SlackAppTokenInput.Text.Trim();
                        break;

                    case "telegram":
                        if (string.IsNullOrWhiteSpace(TelegramTokenInput.Text))
                        {
                            MessageBox.Show("Bot Token is required!", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                        channelConfig["enabled"] = true;
                        channelConfig["token"] = TelegramTokenInput.Text.Trim();
                        if (!string.IsNullOrWhiteSpace(TelegramAllowedUsersInput.Text))
                        {
                            var users = new JArray();
                            foreach (var user in TelegramAllowedUsersInput.Text.Split(','))
                            {
                                var trimmed = user.Trim();
                                if (!string.IsNullOrEmpty(trimmed))
                                    users.Add(trimmed);
                            }
                            channelConfig["allowedUsers"] = users;
                        }
                        break;

                    default:
                        if (string.IsNullOrWhiteSpace(GenericTokenInput.Text))
                        {
                            MessageBox.Show("API Token is required!", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                        channelConfig["enabled"] = true;
                        channelConfig["token"] = GenericTokenInput.Text.Trim();
                        if (!string.IsNullOrWhiteSpace(GenericConfigInput.Text))
                        {
                            try
                            {
                                var extraConfig = JObject.Parse(GenericConfigInput.Text);
                                foreach (var prop in extraConfig.Properties())
                                {
                                    channelConfig[prop.Name] = prop.Value;
                                }
                            }
                            catch
                            {
                                MessageBox.Show("Invalid JSON in Additional Config", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                                return;
                            }
                        }
                        break;
                }

                // Save to config
                channels![_channelType.ToLower()] = channelConfig;

                // Write config file
                File.WriteAllText(_configPath, config.ToString(Newtonsoft.Json.Formatting.Indented));

                // Ask to restart gateway
                var result = MessageBox.Show(
                    $"{_channelType} configuration saved!\n\nRestart the gateway now to apply changes?",
                    "Configuration Saved",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    // Find MainWindow and restart gateway
                    if (Owner is MainWindow mainWindow)
                    {
                        DialogResult = true;
                        Close();
                        
                        // Trigger gateway restart
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

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void OpenHelpLink_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo(_helpUrl) { UseShellExecute = true });
        }
    }
}
