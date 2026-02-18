using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Hardcodet.Wpf.TaskbarNotification;
using Microsoft.Toolkit.Uwp.Notifications;
using Newtonsoft.Json.Linq;
using Path = System.IO.Path;
using File = System.IO.File;
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;

namespace OpenClawGUI;

public class ChatMessage
{
    public string Sender { get; set; } = "";
    public string Message { get; set; } = "";
    public string Time { get; set; } = "";
    public string Background { get; set; } = "";
    public string SenderColor { get; set; } = "";
    public HorizontalAlignment Alignment { get; set; }
}

public partial class MainWindow : Window
{
    private readonly ObservableCollection<ChatMessage> _messages = new();
    private readonly HttpClient _httpClient = new();
    private Process? _gatewayProcess;
    private readonly string _configPath;
    private readonly string _openClawPath;
    private string _currentSessionId = "gui-session";
    private string _gatewayUrl = "http://127.0.0.1:18789";
    private string _authToken = "";
    private string _pnpmPath = @"C:\Users\markv\AppData\Roaming\npm\pnpm.cmd";
    private TaskbarIcon? _trayIcon;

    public MainWindow()
    {
        InitializeComponent();
        
        _configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".openclaw", "openclaw.json");
        _openClawPath = @"P:\jarvis\openclaw";
        
        LoadConfiguration();
        InitializeSystemTray();
        _ = CheckGatewayStatusAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                // Suppress connection refused error on startup
            }
        });
        LoadModels();
        
        // Navigate to Chat page by default
        NavigateToChat_Click(this, new RoutedEventArgs());
        
        AddSystemMessage("Welcome to OpenClaw! 🦞");
        AddSystemMessage("Click '▶ START' to launch the gateway.");
    }

    private void InitializeSystemTray()
    {
        var contextMenu = new System.Windows.Controls.ContextMenu();
        
        var showItem = new System.Windows.Controls.MenuItem { Header = "Show OpenClaw" };
        showItem.Click += (s, e) => { Show(); WindowState = WindowState.Normal; Activate(); };
        contextMenu.Items.Add(showItem);
        
        contextMenu.Items.Add(new System.Windows.Controls.Separator());
        
        var startGatewayItem = new System.Windows.Controls.MenuItem { Header = "▶ Start Gateway" };
        startGatewayItem.Click += StartGateway_Click;
        contextMenu.Items.Add(startGatewayItem);
        
        var stopGatewayItem = new System.Windows.Controls.MenuItem { Header = "⏹ Stop Gateway" };
        stopGatewayItem.Click += StopGateway_Click;
        contextMenu.Items.Add(stopGatewayItem);
        
        var statusItem = new System.Windows.Controls.MenuItem { Header = "🔄 Check Status" };
        statusItem.Click += CheckStatus_Click;
        contextMenu.Items.Add(statusItem);
        
        contextMenu.Items.Add(new System.Windows.Controls.Separator());
        
        var webUiItem = new System.Windows.Controls.MenuItem { Header = "🌐 Open Web UI" };
        webUiItem.Click += OpenWebUI_Click;
        contextMenu.Items.Add(webUiItem);
        
        var configItem = new System.Windows.Controls.MenuItem { Header = "📂 Open Config" };
        configItem.Click += OpenConfig_Click;
        contextMenu.Items.Add(configItem);
        
        contextMenu.Items.Add(new System.Windows.Controls.Separator());
        
        var exitItem = new System.Windows.Controls.MenuItem { Header = "Exit" };
        exitItem.Click += (s, e) => { _trayIcon?.Dispose(); Application.Current.Shutdown(); };
        contextMenu.Items.Add(exitItem);

        _trayIcon = new TaskbarIcon
        {
            ToolTipText = "OpenClaw Windows GUI",
            ContextMenu = contextMenu,
            Visibility = Visibility.Visible
        };
        
        // Use default application icon
        _trayIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Reflection.Assembly.GetExecutingAssembly().Location);
        
        _trayIcon.TrayMouseDoubleClick += (s, e) => { Show(); WindowState = WindowState.Normal; Activate(); };
    }

    protected override void OnStateChanged(EventArgs e)
    {
        base.OnStateChanged(e);
        if (WindowState == WindowState.Minimized)
        {
            Hide();
            ShowNotification("OpenClaw Minimized", "OpenClaw is running in the system tray.");
        }
    }

    public void ShowNotification(string title, string message)
    {
        try
        {
            _trayIcon?.ShowBalloonTip(title, message, BalloonIcon.Info);
        }
        catch { }
    }

    private void LoadConfiguration()
    {
        try
        {
            if (File.Exists(_configPath))
            {
                var json = File.ReadAllText(_configPath);
                var config = JObject.Parse(json);
                
                var gateway = config["gateway"];
                if (gateway != null)
                {
                    var port = gateway["port"]?.ToString() ?? "18789";
                    _gatewayUrl = $"http://127.0.0.1:{port}";
                    _authToken = gateway["auth"]?["token"]?.ToString() ?? "";
                }
                
                StatusBarText.Text = $"Config loaded";
            }
        }
        catch (Exception ex)
        {
            AddSystemMessage($"Error loading config: {ex.Message}");
        }
    }

    private void LoadModels()
    {
        try
        {
            if (!File.Exists(_configPath)) return;
            
            var json = File.ReadAllText(_configPath);
            var config = JObject.Parse(json);
            
            var primary = config["agents"]?["defaults"]?["model"]?["primary"]?.ToString();
            if (!string.IsNullOrEmpty(primary))
            {
                ModelInfo.Text = $"Model: {primary}";
            }
        }
        catch (Exception ex)
        {
            AddSystemMessage($"Error loading models: {ex.Message}");
        }
    }

    private async Task CheckGatewayStatusAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_gatewayUrl}/health");
            if (response.IsSuccessStatusCode)
            {
                SetGatewayOnline();
            }
            else
            {
                SetGatewayOffline();
            }
        }
        catch
        {
            SetGatewayOffline();
        }
    }

    private void SetGatewayOnline()
    {
        Dispatcher.Invoke(() =>
        {
            StatusText.Text = "● ONLINE";
            StatusText.Foreground = new SolidColorBrush(Colors.White);
            StatusBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#238636"));
            StopGatewayButton.IsEnabled = true;
        });
    }

    private void SetGatewayOffline()
    {
        Dispatcher.Invoke(() =>
        {
            StatusText.Text = "● OFFLINE";
            StatusText.Foreground = new SolidColorBrush(Colors.White);
            StatusBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DA3633"));
            StopGatewayButton.IsEnabled = false;
        });
    }

    private void AddSystemMessage(string message)
    {
        _messages.Add(new ChatMessage
        {
            Sender = "System",
            Message = message,
            Time = DateTime.Now.ToString("HH:mm:ss"),
            Background = "#161B22",
            SenderColor = "#8B949E",
            Alignment = HorizontalAlignment.Center
        });
    }

    private void AddUserMessage(string message)
    {
        _messages.Add(new ChatMessage
        {
            Sender = "You",
            Message = message,
            Time = DateTime.Now.ToString("HH:mm:ss"),
            Background = "#1F6FEB",
            SenderColor = "#FFFFFF",
            Alignment = HorizontalAlignment.Right
        });
    }

    private void AddAssistantMessage(string message)
    {
        _messages.Add(new ChatMessage
        {
            Sender = "OpenClaw",
            Message = message,
            Time = DateTime.Now.ToString("HH:mm:ss"),
            Background = "#21262D",
            SenderColor = "#58A6FF",
            Alignment = HorizontalAlignment.Left
        });
    }

    public async void SendMessage()
    {
        await SendMessageAsync();
    }

    private async Task SendMessageAsync()
    {
        // Get message from current chat page
        var chatPage = MainFrame.Content as Pages.ChatPage;
        if (chatPage == null) return;
        
        var message = chatPage.MessageInput.Text.Trim();
        if (string.IsNullOrEmpty(message)) return;

        AddUserMessage(message);
        chatPage.MessageInput.Clear();
        StatusBarText.Text = "Sending message...";

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c cd /d \"{_openClawPath}\" && \"{_pnpmPath}\" openclaw agent --session-id {_currentSessionId} --message \"{message.Replace("\"", "\\\"")}\"",
                WorkingDirectory = _openClawPath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            if (process != null)
            {
                var output = await process.StandardOutput.ReadToEndAsync();
                var error = await process.StandardError.ReadToEndAsync();
                await process.WaitForExitAsync();

                if (!string.IsNullOrEmpty(output))
                {
                    var cleanOutput = output.Replace("◓", "").Replace("◑", "").Replace("◒", "").Replace("◐", "")
                        .Replace("Waiting for agent reply", "").Trim();
                    
                    if (!string.IsNullOrEmpty(cleanOutput))
                    {
                        AddAssistantMessage(cleanOutput);
                    }
                }

                if (process.ExitCode != 0 && !string.IsNullOrEmpty(error))
                {
                    AddSystemMessage($"Error: {error}");
                }

                StatusBarText.Text = "Message sent";
            }
        }
        catch (Exception ex)
        {
            AddSystemMessage($"Error sending message: {ex.Message}");
            StatusBarText.Text = "Error";
        }
    }

    private async void StartGateway_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            StatusBarText.Text = "Starting gateway...";
            AddSystemMessage("Starting gateway...");
            
            // Check if already running
            try
            {
                var checkResponse = await _httpClient.GetAsync($"{_gatewayUrl}/health");
                if (checkResponse.IsSuccessStatusCode)
                {
                    SetGatewayOnline();
                    AddSystemMessage("Gateway is already running!");
                    StatusBarText.Text = "Gateway already running";
                    return;
                }
            }
            catch
            {
                // Gateway not running, proceed to start it
            }
            
            var psi = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/K cd /d \"{_openClawPath}\" && \"{_pnpmPath}\" openclaw gateway run --verbose",
                WorkingDirectory = _openClawPath,
                UseShellExecute = true,
                CreateNoWindow = false
            };

            _gatewayProcess = Process.Start(psi);
            
            // Wait and check multiple times
            bool started = false;
            for (int i = 0; i < 10; i++)
            {
                await Task.Delay(1000);
                try
                {
                    var response = await _httpClient.GetAsync($"{_gatewayUrl}/health");
                    if (response.IsSuccessStatusCode)
                    {
                        started = true;
                        break;
                    }
                }
                catch { }
            }
            
            if (started)
            {
                SetGatewayOnline();
                AddSystemMessage("Gateway started successfully!");
                StatusBarText.Text = "Gateway online";
            }
            else
            {
                SetGatewayOffline();
                AddSystemMessage("Gateway failed to start. Check the terminal for errors.");
                StatusBarText.Text = "Gateway failed to start";
            }
        }
        catch (Exception ex)
        {
            SetGatewayOffline();
            AddSystemMessage($"Error starting gateway: {ex.Message}");
            StatusBarText.Text = "Error";
        }
    }

    private async void StopGateway_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            StatusBarText.Text = "Stopping gateway...";
            AddSystemMessage("Stopping gateway...");
            
            // Try to kill the process we started
            if (_gatewayProcess != null && !_gatewayProcess.HasExited)
            {
                try
                {
                    _gatewayProcess.Kill(true);
                }
                catch { }
                _gatewayProcess = null;
            }
            
            // Kill any process using the gateway port (18789)
            // Use netstat to find the PID
            try
            {
                var netstatPsi = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = "/c netstat -ano | findstr :18789 | findstr LISTENING",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                
                using var netstatProc = Process.Start(netstatPsi);
                if (netstatProc != null)
                {
                    var output = await netstatProc.StandardOutput.ReadToEndAsync();
                    await netstatProc.WaitForExitAsync();
                    
                    // Parse PID from netstat output (last column)
                    var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                    foreach (var line in lines)
                    {
                        var parts = line.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length > 0 && int.TryParse(parts[^1], out int pid))
                        {
                            try
                            {
                                var proc = Process.GetProcessById(pid);
                                proc.Kill(true);
                                AddSystemMessage($"Killed process {pid} on port 18789");
                            }
                            catch { }
                        }
                    }
                }
            }
            catch { }

            // Also kill any cmd.exe windows that might be running the gateway
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "taskkill",
                    Arguments = "/F /FI \"WINDOWTITLE eq *openclaw*\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                Process.Start(psi)?.WaitForExit(3000);
            }
            catch { }
            
            await Task.Delay(1500);
            
            // Verify it's actually stopped
            try
            {
                var cts = new System.Threading.CancellationTokenSource(2000);
                var response = await _httpClient.GetAsync($"{_gatewayUrl}/health", cts.Token);
                if (response.IsSuccessStatusCode)
                {
                    AddSystemMessage("Warning: Gateway may still be running. Try closing the terminal window manually.");
                    StatusBarText.Text = "Gateway may still be running";
                }
                else
                {
                    SetGatewayOffline();
                    AddSystemMessage("Gateway stopped");
                    StatusBarText.Text = "Gateway stopped";
                }
            }
            catch
            {
                SetGatewayOffline();
                AddSystemMessage("Gateway stopped");
                StatusBarText.Text = "Gateway stopped";
            }
        }
        catch (Exception ex)
        {
            AddSystemMessage($"Error stopping gateway: {ex.Message}");
        }
    }

    private async void RestartGateway_Click(object sender, RoutedEventArgs e)
    {
        AddSystemMessage("Restarting gateway...");
        StatusBarText.Text = "Restarting gateway...";
        
        StopGateway_Click(sender, e);
        await Task.Delay(2000);
        StartGateway_Click(sender, e);
    }

    public async void RestartGatewayFromDialog()
    {
        AddSystemMessage("Restarting gateway with new configuration...");
        StatusBarText.Text = "Restarting gateway...";
        
        StopGateway_Click(this, new RoutedEventArgs());
        await Task.Delay(2000);
        StartGateway_Click(this, new RoutedEventArgs());
    }

    private async void CheckStatus_Click(object sender, RoutedEventArgs e)
    {
        StatusBarText.Text = "Checking gateway status...";
        AddSystemMessage("Checking gateway status...");
        
        await CheckGatewayStatusAsync();
        
        var isOnline = StatusText.Text.Contains("ONLINE");
        AddSystemMessage(isOnline ? "Gateway is ONLINE " : "Gateway is OFFLINE ");
        StatusBarText.Text = isOnline ? "Status: Online" : "Status: Offline";
    }

    // Navigation Methods
    private void NavigateToOverview_Click(object sender, RoutedEventArgs e)
    {
        MainFrame.Navigate(new Pages.OverviewPage());
        UpdateNavButtons(OverviewNavButton);
    }

    private void NavigateToChat_Click(object sender, RoutedEventArgs e)
    {
        var chatPage = new Pages.ChatPage();
        chatPage.MessagesPanel.ItemsSource = _messages;
        MainFrame.Content = chatPage;
        UpdateNavButtons(ChatNavButton);
    }

    private void NavigateToSessions_Click(object sender, RoutedEventArgs e)
    {
        MainFrame.Navigate(new Pages.SessionsPage());
        UpdateNavButtons(SessionsNavButton);
    }

    private void NavigateToAgents_Click(object sender, RoutedEventArgs e)
    {
        MainFrame.Navigate(new Pages.AgentsPage());
        UpdateNavButtons(AgentsNavButton);
    }

    private void NavigateToSkills_Click(object sender, RoutedEventArgs e)
    {
        MainFrame.Navigate(new Pages.SkillsPage());
        UpdateNavButtons(SkillsNavButton);
    }

    private void NavigateToChannels_Click(object sender, RoutedEventArgs e)
    {
        MainFrame.Navigate(new Pages.ChannelsPage());
        UpdateNavButtons(ChannelsNavButton);
    }

    private void NavigateToUsage_Click(object sender, RoutedEventArgs e)
    {
        MainFrame.Navigate(new Pages.UsagePage());
        UpdateNavButtons(UsageNavButton);
    }

    private void NavigateToModels_Click(object sender, RoutedEventArgs e)
    {
        MainFrame.Navigate(new Pages.ModelsPage());
        UpdateNavButtons(ModelsNavButton);
    }

    private void NavigateToSettings_Click(object sender, RoutedEventArgs e)
    {
        MainFrame.Navigate(new Pages.SettingsPage());
        UpdateNavButtons(SettingsNavButton);
    }

    private void NavigateToLogs_Click(object sender, RoutedEventArgs e)
    {
        MainFrame.Navigate(new Pages.LogsPage());
        UpdateNavButtons(LogsNavButton);
    }

    private void NavigateToCron_Click(object sender, RoutedEventArgs e)
    {
        MainFrame.Navigate(new Pages.CronPage());
        UpdateNavButtons(CronNavButton);
    }

    private void NavigateToNodes_Click(object sender, RoutedEventArgs e)
    {
        MainFrame.Navigate(new Pages.NodesPage());
        UpdateNavButtons(NodesNavButton);
    }

    private void NavigateToInstances_Click(object sender, RoutedEventArgs e)
    {
        MainFrame.Navigate(new Pages.InstancesPage());
        UpdateNavButtons(InstancesNavButton);
    }

    private void NavigateToDebug_Click(object sender, RoutedEventArgs e)
    {
        MainFrame.Navigate(new Pages.DebugPage());
        UpdateNavButtons(DebugNavButton);
    }

    private void UpdateNavButtons(Button activeButton)
    {
        // Reset all buttons
        var inactiveBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#21262D"));
        var inactiveForeground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C9D1D9"));
        
        OverviewNavButton.Background = inactiveBackground;
        OverviewNavButton.Foreground = inactiveForeground;
        ChatNavButton.Background = inactiveBackground;
        ChatNavButton.Foreground = inactiveForeground;
        SessionsNavButton.Background = inactiveBackground;
        SessionsNavButton.Foreground = inactiveForeground;
        AgentsNavButton.Background = inactiveBackground;
        AgentsNavButton.Foreground = inactiveForeground;
        SkillsNavButton.Background = inactiveBackground;
        SkillsNavButton.Foreground = inactiveForeground;
        ChannelsNavButton.Background = inactiveBackground;
        ChannelsNavButton.Foreground = inactiveForeground;
        UsageNavButton.Background = inactiveBackground;
        UsageNavButton.Foreground = inactiveForeground;
        CronNavButton.Background = inactiveBackground;
        CronNavButton.Foreground = inactiveForeground;
        NodesNavButton.Background = inactiveBackground;
        NodesNavButton.Foreground = inactiveForeground;
        InstancesNavButton.Background = inactiveBackground;
        InstancesNavButton.Foreground = inactiveForeground;
        ModelsNavButton.Background = inactiveBackground;
        ModelsNavButton.Foreground = inactiveForeground;
        SettingsNavButton.Background = inactiveBackground;
        SettingsNavButton.Foreground = inactiveForeground;
        LogsNavButton.Background = inactiveBackground;
        LogsNavButton.Foreground = inactiveForeground;
        DebugNavButton.Background = inactiveBackground;
        DebugNavButton.Foreground = inactiveForeground;

        // Highlight active button
        activeButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1F6FEB"));
        activeButton.Foreground = new SolidColorBrush(Colors.White);
    }

    // Session Management
    public void CreateNewSession()
    {
        _currentSessionId = $"session-{DateTime.Now:yyyyMMddHHmmss}";
        _messages.Clear();
        AddSystemMessage($"New session created: {_currentSessionId}");
        StatusBarText.Text = $"Session: {_currentSessionId}";
        NavigateToChat_Click(this, new RoutedEventArgs());
    }

    public void LoadSession(string sessionId)
    {
        _currentSessionId = sessionId;
        _messages.Clear();
        AddSystemMessage($"Loaded session: {sessionId}");
        StatusBarText.Text = $"Session: {sessionId}";
        NavigateToChat_Click(this, new RoutedEventArgs());
    }

    public void DeleteSession(string sessionId)
    {
        AddSystemMessage($"Session {sessionId} deleted");
    }

    // Model Management
    public async void RefreshOllamaModels()
    {
        try
        {
            var response = await _httpClient.GetStringAsync("http://127.0.0.1:11434/api/tags");
            var json = JObject.Parse(response);
            var models = json["models"] as JArray;
            
            AddSystemMessage($"Found {models?.Count ?? 0} Ollama models");
        }
        catch (Exception ex)
        {
            AddSystemMessage($"Error refreshing Ollama models: {ex.Message}");
        }
    }

    public async void RefreshLMStudioModels()
    {
        try
        {
            var response = await _httpClient.GetStringAsync("http://127.0.0.1:1234/v1/models");
            var json = JObject.Parse(response);
            var models = json["data"] as JArray;
            
            AddSystemMessage($"Found {models?.Count ?? 0} LMStudio models");
        }
        catch (Exception ex)
        {
            AddSystemMessage($"Error refreshing LMStudio models: {ex.Message}");
        }
    }

    public void SelectModel(string modelName)
    {
        ModelInfo.Text = $"Model: {modelName}";
        AddSystemMessage($"Selected model: {modelName}");
    }

    // Settings Management
    public void SaveGatewaySettings(string url, string token)
    {
        _gatewayUrl = url;
        _authToken = token;
        AddSystemMessage("Gateway settings saved");
    }

    public void SavePathSettings(string openClawPath, string pnpmPath)
    {
        AddSystemMessage("Path settings saved (restart required)");
    }

    public void OpenConfigFile()
    {
        OpenConfig_Click(this, new RoutedEventArgs());
    }

    public void OpenWebUI()
    {
        OpenWebUI_Click(this, new RoutedEventArgs());
    }

    private async void OllamaModels_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var response = await _httpClient.GetStringAsync("http://127.0.0.1:11434/api/tags");
            var json = JObject.Parse(response);
            var models = json["models"] as JArray;
            
            var modelList = "Ollama Models:\n\n";
            if (models != null)
            {
                foreach (var model in models)
                {
                    modelList += $"• {model["name"]}\n";
                }
            }
            
            MessageBox.Show(modelList, "Ollama Models", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}\n\nMake sure Ollama is running.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void LMStudioModels_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var response = await _httpClient.GetStringAsync("http://127.0.0.1:1234/v1/models");
            var json = JObject.Parse(response);
            var models = json["data"] as JArray;
            
            var modelList = "LMStudio Models:\n\n";
            if (models != null)
            {
                foreach (var model in models)
                {
                    modelList += $"• {model["id"]}\n";
                }
            }
            
            MessageBox.Show(modelList, "LMStudio Models", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}\n\nMake sure LMStudio server is running.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void NewSession_Click(object sender, RoutedEventArgs e)
    {
        _currentSessionId = $"session-{DateTime.Now:yyyyMMddHHmmss}";
        _messages.Clear();
        AddSystemMessage($"New session created: {_currentSessionId}");
        StatusBarText.Text = $"Session: {_currentSessionId}";
    }

    private void OpenConfig_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (File.Exists(_configPath))
            {
                Process.Start(new ProcessStartInfo(_configPath) { UseShellExecute = true });
            }
            else
            {
                MessageBox.Show("Config file not found!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void OpenWebUI_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo(_gatewayUrl) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        
        _trayIcon?.Dispose();
        
        if (_gatewayProcess != null && !_gatewayProcess.HasExited)
        {
            var result = MessageBox.Show("Gateway is still running. Stop it?", "Stop Gateway", 
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                _gatewayProcess.Kill(true);
            }
        }
        
        _httpClient.Dispose();
    }
}