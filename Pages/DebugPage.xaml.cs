using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using Path = System.IO.Path;

namespace OpenClawGUI.Pages
{
    public class ConnectionStatus
    {
        public string Name { get; set; } = "";
        public string Status { get; set; } = "UNKNOWN";
        public string StatusColor { get; set; } = "#7D8590";
        public string Url { get; set; } = "";
    }

    public partial class DebugPage : UserControl
    {
        private readonly HttpClient _httpClient = new();
        private readonly ObservableCollection<ConnectionStatus> _connections = new();
        private readonly DateTime _startTime = DateTime.Now;

        public DebugPage()
        {
            InitializeComponent();
            ConnectionsList.ItemsSource = _connections;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadSystemInfo();
            LoadConfigInfo();
            await RefreshConnections();
        }

        private void LoadSystemInfo()
        {
            OSInfo.Text = $"{Environment.OSVersion.Platform} {Environment.OSVersion.Version}";
            MachineInfo.Text = Environment.MachineName;
            DotNetInfo.Text = Environment.Version.ToString();
            
            var process = Process.GetCurrentProcess();
            var memoryMB = process.WorkingSet64 / 1024.0 / 1024.0;
            MemoryInfo.Text = $"{memoryMB:F1} MB";
            
            var uptime = DateTime.Now - _startTime;
            UptimeInfo.Text = uptime.TotalMinutes < 1 ? "Just started" : $"{uptime.TotalMinutes:F0} minutes";
        }

        private void LoadConfigInfo()
        {
            var configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".openclaw", "openclaw.json");
            ConfigPathInfo.Text = configPath;
            OpenClawPathInfo.Text = @"P:\jarvis\openclaw";
            PnpmPathInfo.Text = @"C:\Users\markv\AppData\Roaming\npm\pnpm.cmd";
            GatewayUrlInfo.Text = "http://127.0.0.1:18789";
        }

        private async System.Threading.Tasks.Task RefreshConnections()
        {
            _connections.Clear();

            // Check Gateway
            var gateway = new ConnectionStatus { Name = "OpenClaw Gateway", Url = "http://127.0.0.1:18789" };
            try
            {
                var response = await _httpClient.GetAsync("http://127.0.0.1:18789/health");
                if (response.IsSuccessStatusCode)
                {
                    gateway.Status = "ONLINE";
                    gateway.StatusColor = "#238636";
                }
                else
                {
                    gateway.Status = "ERROR";
                    gateway.StatusColor = "#DA3633";
                }
            }
            catch
            {
                gateway.Status = "OFFLINE";
                gateway.StatusColor = "#7D8590";
            }
            _connections.Add(gateway);

            // Check Ollama
            var ollama = new ConnectionStatus { Name = "Ollama", Url = "http://127.0.0.1:11434" };
            try
            {
                var response = await _httpClient.GetAsync("http://127.0.0.1:11434/api/tags");
                if (response.IsSuccessStatusCode)
                {
                    ollama.Status = "ONLINE";
                    ollama.StatusColor = "#238636";
                }
                else
                {
                    ollama.Status = "ERROR";
                    ollama.StatusColor = "#DA3633";
                }
            }
            catch
            {
                ollama.Status = "OFFLINE";
                ollama.StatusColor = "#7D8590";
            }
            _connections.Add(ollama);

            // Check LMStudio
            var lmstudio = new ConnectionStatus { Name = "LMStudio", Url = "http://127.0.0.1:1234" };
            try
            {
                var response = await _httpClient.GetAsync("http://127.0.0.1:1234/v1/models");
                if (response.IsSuccessStatusCode)
                {
                    lmstudio.Status = "ONLINE";
                    lmstudio.StatusColor = "#238636";
                }
                else
                {
                    lmstudio.Status = "ERROR";
                    lmstudio.StatusColor = "#DA3633";
                }
            }
            catch
            {
                lmstudio.Status = "OFFLINE";
                lmstudio.StatusColor = "#7D8590";
            }
            _connections.Add(lmstudio);

            // Update memory info
            var process = Process.GetCurrentProcess();
            var memoryMB = process.WorkingSet64 / 1024.0 / 1024.0;
            MemoryInfo.Text = $"{memoryMB:F1} MB";
        }

        private async void RefreshConnections_Click(object sender, RoutedEventArgs e)
        {
            await RefreshConnections();
        }

        private void CopyDebugInfo_Click(object sender, RoutedEventArgs e)
        {
            var debugInfo = $@"OpenClaw Windows GUI Debug Info
================================
Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}

System Information:
- OS: {OSInfo.Text}
- Machine: {MachineInfo.Text}
- .NET: {DotNetInfo.Text}
- Memory: {MemoryInfo.Text}
- GUI Version: v0.3.0

Configuration:
- Config: {ConfigPathInfo.Text}
- OpenClaw: {OpenClawPathInfo.Text}
- pnpm: {PnpmPathInfo.Text}
- Gateway: {GatewayUrlInfo.Text}

Connection Status:
";
            foreach (var conn in _connections)
            {
                debugInfo += $"- {conn.Name}: {conn.Status} ({conn.Url})\n";
            }

            Clipboard.SetText(debugInfo);
            MessageBox.Show("Debug info copied to clipboard!", "Copied", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ClearCache_Click(object sender, RoutedEventArgs e)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            MessageBox.Show("Cache cleared", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OpenDataFolder_Click(object sender, RoutedEventArgs e)
        {
            var dataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".openclaw");
            Process.Start(new ProcessStartInfo(dataPath) { UseShellExecute = true });
        }

        private void OpenLogsFolder_Click(object sender, RoutedEventArgs e)
        {
            var logsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".openclaw", "logs");
            if (!System.IO.Directory.Exists(logsPath))
            {
                System.IO.Directory.CreateDirectory(logsPath);
            }
            Process.Start(new ProcessStartInfo(logsPath) { UseShellExecute = true });
        }

        private void ForceGC_Click(object sender, RoutedEventArgs e)
        {
            var before = GC.GetTotalMemory(false) / 1024.0 / 1024.0;
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            var after = GC.GetTotalMemory(true) / 1024.0 / 1024.0;
            
            var process = Process.GetCurrentProcess();
            var memoryMB = process.WorkingSet64 / 1024.0 / 1024.0;
            MemoryInfo.Text = $"{memoryMB:F1} MB";
            
            MessageBox.Show($"GC completed\nBefore: {before:F1} MB\nAfter: {after:F1} MB", 
                "Garbage Collection", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
