using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json.Linq;

namespace OpenClawGUI.Pages
{
    public class ModelInfo
    {
        public string Name { get; set; } = "";
        public string Size { get; set; } = "";
        public string Modified { get; set; } = "";
        public string Provider { get; set; } = "";
        public string FullName => $"{Provider}/{Name}";
    }

    public partial class ModelsPage : UserControl
    {
        private readonly HttpClient _httpClient = new();
        private readonly ObservableCollection<ModelInfo> _ollamaModels = new();
        private readonly ObservableCollection<ModelInfo> _lmstudioModels = new();
        private readonly string _configPath;

        public ModelsPage()
        {
            InitializeComponent();
            _configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".openclaw", "openclaw.json");
            OllamaModelsList.ItemsSource = _ollamaModels;
            LMStudioModelsList.ItemsSource = _lmstudioModels;
            Loaded += Page_Loaded;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadOllamaModels();
            await LoadLMStudioModels();
        }

        private async System.Threading.Tasks.Task LoadOllamaModels()
        {
            _ollamaModels.Clear();
            try
            {
                var response = await _httpClient.GetStringAsync("http://127.0.0.1:11434/api/tags");
                var json = JObject.Parse(response);
                var models = json["models"] as JArray;

                if (models != null)
                {
                    foreach (var model in models)
                    {
                        var size = model["size"]?.ToObject<long>() ?? 0;
                        var sizeStr = size > 1_000_000_000 ? $"{size / 1_000_000_000.0:F1} GB" : $"{size / 1_000_000.0:F1} MB";
                        
                        _ollamaModels.Add(new ModelInfo
                        {
                            Name = model["name"]?.ToString() ?? "Unknown",
                            Size = sizeStr,
                            Modified = model["modified_at"]?.ToString()?.Substring(0, 10) ?? "",
                            Provider = "ollama"
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _ollamaModels.Add(new ModelInfo
                {
                    Name = "Ollama not running",
                    Size = ex.Message,
                    Modified = "",
                    Provider = "ollama"
                });
            }
        }

        private async System.Threading.Tasks.Task LoadLMStudioModels()
        {
            _lmstudioModels.Clear();
            try
            {
                var response = await _httpClient.GetStringAsync("http://127.0.0.1:1234/v1/models");
                var json = JObject.Parse(response);
                var models = json["data"] as JArray;

                if (models != null)
                {
                    foreach (var model in models)
                    {
                        _lmstudioModels.Add(new ModelInfo
                        {
                            Name = model["id"]?.ToString() ?? "Unknown",
                            Size = "Loaded",
                            Modified = "",
                            Provider = "lmstudio"
                        });
                    }
                }
            }
            catch
            {
                _lmstudioModels.Add(new ModelInfo
                {
                    Name = "LMStudio not running",
                    Size = "Start LMStudio server to see models",
                    Modified = "",
                    Provider = "lmstudio"
                });
            }
        }

        private async void RefreshOllama_Click(object sender, RoutedEventArgs e)
        {
            await LoadOllamaModels();
        }

        private async void RefreshLMStudio_Click(object sender, RoutedEventArgs e)
        {
            await LoadLMStudioModels();
        }

        private void SelectModel_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string modelName)
            {
                var mainWindow = Window.GetWindow(this) as MainWindow;
                mainWindow?.SelectModel(modelName);
            }
        }

        private void SetAsPrimary_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is ModelInfo model)
            {
                var fullModelName = model.FullName;
                
                var result = MessageBox.Show(
                    $"Set '{fullModelName}' as your primary model?\n\nThis will update your openclaw.json configuration.",
                    "Set Primary Model",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
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

                        // Ensure agents.defaults.model exists
                        if (config["agents"] == null)
                            config["agents"] = new JObject();
                        if (config["agents"]!["defaults"] == null)
                            config["agents"]!["defaults"] = new JObject();
                        if (config["agents"]!["defaults"]!["model"] == null)
                            config["agents"]!["defaults"]!["model"] = new JObject();

                        config["agents"]!["defaults"]!["model"]!["primary"] = fullModelName;

                        File.WriteAllText(_configPath, config.ToString(Newtonsoft.Json.Formatting.Indented));

                        var mainWindow = Window.GetWindow(this) as MainWindow;
                        mainWindow?.SelectModel(fullModelName);

                        MessageBox.Show(
                            $"Primary model set to '{fullModelName}'.\n\nRestart the gateway for changes to take effect.",
                            "Model Updated",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error saving config: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }
}
