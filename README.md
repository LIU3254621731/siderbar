<h1 align="center">🚀 FlowDock</h1>

<p align="center">
  <strong>Windows 11 Workflow Dock</strong> — Scene-based Desktop Organization with Fluent Design
</p>

<p align="center">
  <img src="https://img.shields.io/badge/.NET-9-512BD4?logo=.net" alt=".NET 9">
  <img src="https://img.shields.io/badge/WinUI-3-0078D4?logo=windows" alt="WinUI 3">
  <img src="https://img.shields.io/badge/Windows-11-0078D6?logo=windows11" alt="Windows 11">
  <img src="https://img.shields.io/badge/license-MIT-green" alt="License">
</p>

---

## 📖 Overview

FlowDock is a Windows 11 desktop workflow dock bar that helps you organize your desktop around "work states" and "usage scenarios". It slides out from the screen edge to provide categorized quick-access panels for apps, files, URLs, and multi-step workflows.

Built with **WinUI 3** and **Windows App SDK**, featuring native **Fluent Design** with acrylic/mica transparency effects, global hotkeys, and automated workflow execution.

---

## ✨ Features

### 🖥️ Screen-Edge Dock
- Slide-out panels from left or right screen edge
- Always-on-top with adjustable transparency
- Acrylic / Mica blur effects (Windows 11 Fluent Design)

### 📂 Categorized Resources
- Organize apps, files, folders, URLs, and system actions
- Drag-and-drop resource management
- Favorites for quick switching

### ⌨️ Global Hotkeys
- Configurable hotkey to summon/dismiss dock
- Per-category shortcut keys

### ⚡ Multi-Step Workflow Engine
- Define sequential workflows: "Open VS Code → Wait for window → Open project URL"
- Window detection for conditional step execution
- Process tracking and status monitoring

### 🎨 Modern Architecture
- **MVVM** pattern with CommunityToolkit.Mvvm
- **Repository pattern** with JSON persistence (SQLite planned)
- Dependency injection throughout

---

## 🏛️ Architecture

```
┌──────────────────────────────────────────┐
│             FlowDock.App (WinUI 3)        │
│  ┌─────────┐ ┌──────────┐ ┌──────────┐  │
│  │  Views   │ │ViewModels│ │  Models  │  │
│  └────┬─────┘ └────┬─────┘ └────┬─────┘  │
│       │             │            │        │
│  ┌────▼─────────────▼────────────▼─────┐  │
│  │            Services                 │  │
│  │  Dock │ Hotkey │ Workflow │ Theme   │  │
│  │  Process Tracker │ Launch │ Detect  │  │
│  └────────────────┬───────────────────┘  │
│                   │                       │
│  ┌────────────────▼───────────────────┐  │
│  │         Data (Repository)          │  │
│  │   JSON Store → SQLite (planned)    │  │
│  └────────────────────────────────────┘  │
└──────────────────────────────────────────┘
         │                        │
    ┌────▼────┐            ┌─────▼──────┐
    │ Win32   │            │ Windows    │
    │ Hotkeys │            │ App SDK    │
    └─────────┘            └────────────┘
```

---

## 🚀 Quick Start

### Prerequisites
- Windows 11 (10 supported)
- .NET 9 SDK
- Visual Studio 2022 with Windows App SDK workload

### Build & Run

```powershell
dotnet restore src/FlowDock.App/FlowDock.App.csproj
dotnet build src/FlowDock.App/FlowDock.App.csproj -c Release
dotnet run --project src/FlowDock.App/FlowDock.App.csproj
```

---

## 📁 Project Structure

```
siderbar/
├── src/
│   ├── FlowDock.App/
│   │   ├── Views/          # XAML pages and controls
│   │   ├── ViewModels/     # MVVM view models
│   │   ├── Models/         # Domain models
│   │   ├── Services/       # Business logic services
│   │   ├── Data/           # Repository implementations
│   │   ├── Converters/     # XAML value converters
│   │   └── Helpers/Win32/  # P/Invoke and native interop
│   └── FlowDock.Tests/     # Unit test project
├── docs/
│   └── architecture.md     # Design documentation
└── FlowDock.sln
```

---

## 🔧 Tech Stack

| Layer | Technology |
|-------|-----------|
| UI Framework | WinUI 3, Windows App SDK |
| Language | C# (.NET 9) |
| Architecture | MVVM (CommunityToolkit.Mvvm) |
| Serialization | System.Text.Json |
| Logging | Serilog |
| Testing | xUnit |
| Native Interop | P/Invoke (Win32 Hotkey, Process APIs) |

---

## 📝 License

MIT
