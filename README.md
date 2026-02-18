# 🦞 OpenClaw Windows GUI

**The Official Windows Desktop Client for OpenClaw** - A fully-featured native Windows application for managing your AI assistant with Ollama and LMStudio support.

![OpenClaw GUI](https://img.shields.io/badge/Platform-Windows-blue) ![.NET](https://img.shields.io/badge/.NET-9.0-purple) ![WPF](https://img.shields.io/badge/UI-WPF-green) ![Version](https://img.shields.io/badge/Version-0.5.0-green)

## 📥 Download

**[⬇️ Download OpenClawGUI v0.5.0](https://github.com/RealShocky/openclaw-windows/releases/tag/v0.5.0)** - Self-contained executable (no .NET install required)

1. Download `OpenClawGUI-v0.5.0-win-x64.zip`
2. Extract to any folder
3. Run `OpenClawGUI.exe`

## ✨ Features

### Core Features
- **🎨 Modern Dark Theme** - Beautiful VS Code-inspired interface with ModernWPF
- **💬 Chat Interface** - Clean, intuitive chat with selectable text
- **🚀 Gateway Control** - Start/Stop/Restart OpenClaw gateway from the GUI
- **🤖 Model Management** - Browse Ollama/LMStudio models, set primary model
- **📊 Live Status** - Real-time gateway, Ollama, and LMStudio status
- **⚙️ Full Configuration** - Configure everything from the GUI, no manual JSON editing

### 14 Fully Functional Pages
| Page | Description |
|------|-------------|
| 📊 Overview | Dashboard with status, quick actions, system info |
| 💬 Chat | Send messages, view responses |
| 📂 Sessions | Create, load, delete sessions from ~/.openclaw/sessions |
| 🤖 Agents | View running agents and status |
| 🛠️ Skills | Configure MCP, Web Search, Image Gen, Audio with dialogs |
| 📡 Channels | Configure Discord, Slack, Telegram, WhatsApp, etc. |
| � Usage | Token usage and cost tracking |
| 🧠 Models | Browse models, set as primary |
| ⚙️ Settings | Gateway URL, paths, browse folders |
| 📝 Logs | Real log viewer with filtering |
| ⏰ Cron | Create/edit scheduled tasks |
| 🔗 Nodes | Execution approvals |
| 🖥️ Instances | Connected clients |
| 🐛 Debug | System diagnostics |

### Configuration Dialogs
- **Channel Config** - Enter credentials for Discord, Slack, Telegram, etc.
- **Skill Config** - MCP with 10 templates, Web Search, Image Gen, Audio
- **Cron Config** - Create scheduled tasks with presets

## 📋 Prerequisites

- **Windows 10/11**
- **.NET 9.0 Runtime** (included with SDK)
- **OpenClaw** installed at `P:\jarvis\openclaw`
- **pnpm** installed
- **Ollama** (optional) - For local models
- **LMStudio** (optional) - For local models

## 🚀 Quick Start

### Build from Source

```powershell
cd P:\jarvis\OpenClawGUI
dotnet restore
dotnet build
dotnet run
```

### Run the Executable

```powershell
cd P:\jarvis\OpenClawGUI\bin\Debug\net9.0-windows
.\OpenClawGUI.exe
```

## 🎯 Usage

### Starting the Gateway

1. Click **▶️ Start Gateway** button in the sidebar
2. Wait for status to change to **● Online** (green)
3. Gateway will start in a separate terminal window

### Sending Messages

1. Type your message in the input box at the bottom
2. Press **Ctrl+Enter** or click **📤 Send**
3. Wait for the AI response to appear

### Managing Models

- **Model Selector** (top right) - Switch between available models
- **📊 Ollama Models** - View installed Ollama models
- **🖥️ LMStudio Models** - View loaded LMStudio models

### Sessions

- **➕ New Session** - Start a fresh conversation
- Sessions are isolated - each has its own context

### Quick Actions

- **📂 Open Config** - Opens `~/.openclaw/openclaw.json` in your editor
- **🌐 Open Web UI** - Opens the OpenClaw web interface in your browser

## 🛠️ Configuration

The GUI automatically loads configuration from:
```
C:\Users\<YourUsername>\.openclaw\openclaw.json
```

### Supported Models

The GUI reads from your OpenClaw config and displays:
- **Ollama models** from `models.providers.ollama`
- **LMStudio models** from `models.providers.lmstudio`
- Any other configured providers

## 🎨 UI Features

### Dark Theme
- Modern dark color scheme matching VS Code
- Syntax-highlighted message bubbles
- Smooth animations and transitions

### Message Types
- **User messages** - Blue bubbles on the right
- **AI responses** - Gray bubbles on the left
- **System messages** - Centered gray notifications

### Status Indicators
- **● Online** (Green) - Gateway is running
- **● Offline** (Red) - Gateway is stopped

## 🔧 Troubleshooting

### Gateway Won't Start
- Check that OpenClaw is installed at `P:\jarvis\openclaw`
- Verify pnpm is installed: `pnpm --version`
- Check the terminal window for error messages

### Models Not Showing
- Ensure `openclaw.json` exists and is valid JSON
- Check that models are defined in the config
- Restart the GUI after config changes

### Can't Send Messages
- Make sure the gateway is **● Online**
- Check that a model is selected
- Verify the session ID is valid

### LMStudio/Ollama Not Detected
- **Ollama**: Ensure it's running on `http://127.0.0.1:11434`
- **LMStudio**: Ensure the server is running on `http://127.0.0.1:1234`

## 📁 Project Structure

```
OpenClawGUI/
├── MainWindow.xaml          # UI layout (WPF XAML)
├── MainWindow.xaml.cs       # Backend logic (C#)
├── App.xaml                 # Application resources
├── OpenClawGUI.csproj       # Project configuration
└── README.md                # This file
```

## 🔌 Dependencies

- **ModernWpfUI** - Modern Windows 11 styling
- **Newtonsoft.Json** - JSON parsing
- **System.Net.WebSockets.Client** - WebSocket support (future)

## 🚧 Future Features

- [ ] WebSocket live connection to gateway
- [ ] Real-time message streaming
- [ ] Settings dialog
- [ ] System tray support
- [ ] Auto-start with Windows
- [ ] Message history persistence
- [ ] File attachment support
- [ ] Voice input/output
- [ ] Multi-language support

## 🤝 Contributing

This is a community project! Feel free to:
- Report bugs
- Suggest features
- Submit pull requests
- Improve documentation

## 📄 License

MIT License - Same as OpenClaw

## 🙏 Credits

- **OpenClaw** - https://github.com/openclaw/openclaw
- **ModernWPF** - Modern UI library for WPF
- Built with ❤️ for the OpenClaw community

## 📞 Support

- **OpenClaw Discord**: https://discord.gg/qkhbAGHRBT
- **OpenClaw GitHub**: https://github.com/openclaw/openclaw
- **Report Issues**: https://github.com/RealShocky/openclaw-windows/issues

---

**Made with 🦞 for the OpenClaw community**
