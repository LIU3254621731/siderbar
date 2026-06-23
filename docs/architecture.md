# FlowDock — MVP 技术方案

## Context

FlowDock 是一个 Windows 平台轻量级场景化桌面工作流 Dock，帮助用户以"工作状态"和"使用场景"为中心重新组织桌面环境与高频操作流程。

- **技术栈**: C# + .NET 8 + WinUI 3 + Windows App SDK + MVVM
- **持久化**: JSON (MVP), 预留 SQLite 迁移路径
- **视觉**: Windows 11 Fluent Design, 亚克力/磨砂, 圆角, 低饱和度

---

## 1. 项目结构

```
FlowDock/
├── FlowDock.sln
├── src/
│   ├── FlowDock.App/                    # WinUI 3 主项目 (packaged desktop)
│   │   ├── FlowDock.App.csproj
│   │   ├── Package.appxmanifest
│   │   ├── App.xaml / App.xaml.cs        # DI 入口, 应用生命周期
│   │   ├── MainWindow.xaml / .cs         # 主窗口 (透明, 无任务栏图标)
│   │   │
│   │   ├── Models/
│   │   │   ├── ResourceType.cs           # 枚举: Application, Folder, URL, SystemAction, Workflow
│   │   │   ├── SystemActionType.cs       # 枚举: Lock, Sleep, Shutdown, Restart, DisplayOff...
│   │   │   ├── InteractionMode.cs        # 枚举: PureHover, HotkeyHover, ClickToggle
│   │   │   ├── WorkflowRunStatus.cs      # 枚举: NotStarted, Running, WaitingForWindow, Completed, Failed, Cancelled
│   │   │   ├── Category.cs               # 分类实体 (Id, Name, IconGlyph, SortOrder, ResourceIds[])
│   │   │   ├── ResourceItem.cs           # 资源实体 (多态: 根据 Type 使用不同字段)
│   │   │   ├── WorkflowDefinition.cs     # 工作流定义 (Id, Name, Steps[])
│   │   │   ├── WorkflowStep.cs           # 工作流步骤 (StepNumber, ResourceId, WaitForWindow, WaitTimeoutMs)
│   │   │   ├── AppSettings.cs            # 全局设置 (交互模式, 热键, 外观, 监视器)
│   │   │   ├── TrackedProcess.cs         # 运行时模型: 被追踪的进程
│   │   │   └── WorkflowRunState.cs       # 运行时模型: 工作流执行状态
│   │   │
│   │   ├── Data/                         # 数据抽象层 (仓库模式)
│   │   │   ├── ICategoryRepository.cs
│   │   │   ├── IResourceRepository.cs
│   │   │   ├── IWorkflowRepository.cs
│   │   │   ├── ISettingsRepository.cs
│   │   │   ├── IDataRepository.cs        # 顶层聚合接口
│   │   │   └── Json/
│   │   │       ├── JsonDataStore.cs      # JSON 文件读写引擎 (原子写入 + 备份轮转)
│   │   │       ├── JsonCategoryRepository.cs
│   │   │       ├── JsonResourceRepository.cs
│   │   │       ├── JsonWorkflowRepository.cs
│   │   │       └── JsonSettingsRepository.cs
│   │   │
│   │   ├── ViewModels/
│   │   │   ├── MainViewModel.cs          # 根 VM: 管理分类列表, Dock 状态, 交互模式
│   │   │   ├── DockContainerViewModel.cs # Dock 位置, 展开/收起动画状态
│   │   │   ├── CategoryViewModel.cs      # 单个分类: 名称/图标/悬停点击处理
│   │   │   ├── ResourcePanelViewModel.cs # 资源面板: 悬停展开/磁贴网格
│   │   │   ├── ResourceItemViewModel.cs  # 单个资源: 启动/状态指示
│   │   │   ├── WorkflowEditorViewModel.cs
│   │   │   └── SettingsViewModel.cs
│   │   │
│   │   ├── Views/
│   │   │   ├── MainWindow.xaml
│   │   │   ├── DockContainer.xaml
│   │   │   ├── CategoryList.xaml / CategoryTab.xaml
│   │   │   ├── ResourcePanel.xaml / ResourceTile.xaml
│   │   │   ├── SettingsWindow.xaml
│   │   │   ├── WorkflowEditor.xaml
│   │   │   └── Controls/
│   │   │       ├── AcrylicHost.cs        # 亚克力背景宿主
│   │   │       └── AnimatedPanel.cs      # 动画感知面板
│   │   │
│   │   ├── Services/
│   │   │   ├── IDockService.cs / DockService.cs           # 屏幕边缘检测与定位
│   │   │   ├── IHotkeyService.cs / HotkeyService.cs       # 全局热键注册 (Win32)
│   │   │   ├── IResourceLaunchService.cs / ResourceLaunchService.cs  # 启动资源
│   │   │   ├── IWorkflowEngine.cs / WorkflowEngine.cs     # 工作流引擎 (顺序执行/窗口等待/嵌套)
│   │   │   ├── IProcessTracker.cs / ProcessTracker.cs     # 进程追踪 (启动/关闭/清理)
│   │   │   ├── IWindowDetectionService.cs / WindowDetectionService.cs  # 窗口检测
│   │   │   ├── ISystemActionService.cs / SystemActionService.cs  # 系统操作
│   │   │   └── IThemeService.cs / ThemeService.cs         # 主题/亚克力
│   │   │
│   │   ├── Helpers/
│   │   │   ├── ObservableObject.cs       # MVVM 基类
│   │   │   ├── RelayCommand.cs
│   │   │   └── Win32/
│   │   │       ├── NativeMethods.cs      # 所有 P/Invoke 声明
│   │   │       ├── WindowStyles.cs       # WS_EX_*, GWL_* 常量
│   │   │       └── HotkeyConstants.cs    # MOD_*, VK_* 常量
│   │   │
│   │   └── Converters/
│   │       ├── BoolToVisibilityConverter.cs
│   │       └── ResourceTypeToIconConverter.cs
│   │
│   └── FlowDock.Tests/
│       ├── FlowDock.Tests.csproj
│       ├── Services/
│       ├── ViewModels/
│       └── Data/
│
└── docs/
    └── architecture.md
```

---

## 2. 数据模型

### 实体关系图

```
Category (1) ──────< (N) ResourceItem
    │                        │
    │                        │ (当 Type == Workflow 时)
    │                        ▼
    │               WorkflowDefinition (1) ──────< (N) WorkflowStep
    │                        ▲                           │
    │                        │                           │ (ResourceId 指向 Type==Workflow
    │                        │                           │  的 ResourceItem 实现嵌套)
    │                        └─── 嵌套引用 ──────────────┘
    │
    └── AppSettings (全局单例配置)
```

### Category (分类)
| 属性 | 类型 | 说明 |
|------|------|------|
| Id | string (GUID) | 主键 |
| Name | string | "开发", "论文", "游戏" |
| IconGlyph | string | Segoe Fluent Icon 字符 |
| SortOrder | int | 排序权重 |
| ColorHint | string | 可选色提示 "#4CC2FF" |
| ResourceIds | List\<string\> | 松散 FK 列表 |
| IsExpanded | bool | 运行时状态 (不持久化) |

### ResourceItem (资源 - 多态)
| 属性 | 类型 | 适用类型 |
|------|------|----------|
| Id | string (GUID) | 全部 |
| Name | string | 全部 |
| Description | string | 全部 |
| Type | ResourceType 枚举 | 全部 |
| IconGlyph | string | 全部 |
| TargetPath | string | Application(.exe), Folder(目录), URL |
| Arguments | string | Application |
| WorkingDirectory | string | Application |
| RunAsAdmin | bool | Application |
| SystemAction | SystemActionType? | SystemAction |
| WorkflowId | string | Workflow |
| CategoryId | string | 全部 |
| SortOrder | int | 全部 |

### WorkflowDefinition (工作流定义)
| 属性 | 类型 | 说明 |
|------|------|------|
| Id | string (GUID) | 主键 |
| Name | string | 工作流名称 |
| Description | string | 描述 |
| Steps | List\<WorkflowStep\> | 步骤列表 |
| StopOnFailure | bool | MVP 固定 true |
| StepTimeout | TimeSpan | 单步超时 (默认 5 分钟) |

### WorkflowStep (工作流步骤)
| 属性 | 类型 | 说明 |
|------|------|------|
| StepNumber | int | 顺序号 |
| ResourceId | string | 指向 ResourceItem |
| WaitForWindow | bool | 是否等待窗口出现 |
| WindowTitlePattern | string | 窗口标题模糊匹配 |
| WindowClassName | string | 窗口类名精确匹配 |
| WaitTimeoutMs | int | 等待超时 (默认 30000ms) |
| IsEnabled | bool | 可临时禁用 |

### AppSettings (全局设置)
| 属性 | 默认值 | 说明 |
|------|--------|------|
| InteractionMode | PureHover | 交互模式 |
| HotkeyModifiers | "Control+Alt" | 热键修饰键 |
| HotkeyKey | "D" | 热键主键 |
| DockWidth | 320 | 展开宽度 (px) |
| DockCollapsedWidth | 48 | 收起宽度 (px) |
| DockEdge | "Left" | 停靠边 |
| UseAcrylic | true | 亚克力效果 |
| BlurIntensity | 80 | 模糊强度 (0-100) |
| FollowSystemTheme | true | 跟随系统主题 |
| TargetMonitorIndex | 0 | 目标监视器 (0=主屏) |

### 运行时模型 (不持久化)

**TrackedProcess**: ProcessId, ProcessName, ResourceId, WorkflowId, StartedAt, MainWindowHandle

**WorkflowRunState**: WorkflowRunId, CurrentStepIndex, Status, LaunchedProcesses[], ErrorMessage

---

## 3. 数据抽象层

### 仓库接口

```csharp
public interface IDataRepository
{
    ICategoryRepository Categories { get; }
    IResourceRepository Resources { get; }
    IWorkflowRepository Workflows { get; }
    ISettingsRepository Settings { get; }
    Task LoadAllAsync();
    Task SaveAllAsync();
}

public interface ICategoryRepository
{
    Task<Category> GetByIdAsync(string id);
    Task<IReadOnlyList<Category>> GetAllAsync();
    Task<Category> AddAsync(Category category);
    Task<Category> UpdateAsync(Category category);
    Task<bool> DeleteAsync(string id);
}
// IResourceRepository, IWorkflowRepository 同理
// ISettingsRepository: GetSettingsAsync() / SaveSettingsAsync()
```

### JSON 持久化策略

- **存储路径**: `%APPDATA%/FlowDock/data/categories.json` (同理 resources/workflows/settings)
- **原子写入**: 写 `.tmp` → 备份旧文件到 `backups/` → `File.Move(.tmp, live)` (NTFS 原子)
- **降级加载**: 主文件 → 最新备份 → 默认空集合
- **内存缓存**: 所有 JSON 仓库维护 `List<T> _cache`, 修改后刷新

### SQLite 迁移路径 (v2)

- 接口不变, 实现 `Sqlite*Repository : I*Repository`
- 首次启动: JSON 文件存在 + 无 `.db` → 导入 SQLite → 写 `.migration_completed`
- DI 替换即可: `AddSingleton<IDataRepository, SqliteDataStore>()`

---

## 4. 工作流引擎

### 执行流程

```
验证所有资源 → 遍历步骤 (按 StepNumber):
  ├── 解析 ResourceItem → 根据 Type 分发:
  │   ├── Application → Process.Start(exe, args)
  │   ├── Folder → ShellExecute("open", path)
  │   ├── URL → ShellExecute("open", url)
  │   ├── SystemAction → LockWorkStation / SetSuspendState / ...
  │   └── Workflow → 递归调用 (防循环: HashSet visitedIds)
  │
  ├── ProcessTracker.RegisterProcess() 记录启动的进程
  │
  ├── 若 WaitForWindow:
  │   └── WindowDetectionService.WaitForWindowAsync(title, class, timeout)
  │       ├── 窗口出现 → 继续下一步
  │       └── 超时 → 终止, 返回 Failure
  │
  └── 全部完成 → 返回 Completed + 进程列表
```

### 窗口检测算法

- 每 250ms 调用 `EnumWindows` 轮询
- 对每个 HWND: `IsWindowVisible` + `GetWindowText` + `GetClassName` + `GetWindowThreadProcessId`
- 标题模糊匹配 (不区分大小写 Contains) 且 PID 属于我们追踪的进程
- 超时时间由 `WorkflowStep.WaitTimeoutMs` 控制

### 进程追踪

- `ConcurrentDictionary<int, TrackedProcess>`
- 注册时订阅 `Process.Exited` 事件
- `CloseAllForWorkflowAsync`: 发送 `WM_CLOSE` → 等 3s → `Process.Kill()` 幸存者

### 嵌套防护

- `HashSet<string> visitedWorkflowIds` 递归传递
- 检测到重复 ID → 抛出异常终止

---

## 5. 交互模式

| 模式 | 展开触发 | 收起触发 |
|------|----------|----------|
| A: PureHover | 鼠标进入边缘区 (5px) | 鼠标离开 Dock+面板 (300ms 宽限) |
| B: HotkeyHover | 热键武装 (3s) + 鼠标进入边缘区 | 鼠标离开 / 热键重按 / 武装超时 |
| C: ClickToggle | 点击收起态 Dock | 点击 Dock 外部 / 点切换按钮 |

---

## 6. 关键 Win32 API

| 用途 | API |
|------|-----|
| 全局热键 | `RegisterHotKey`, `UnregisterHotKey`, `WM_HOTKEY (0x0312)` |
| 窗口隐藏任务栏 | `SetWindowLong(GWL_EXSTYLE)`, `WS_EX_TOOLWINDOW` |
| 防焦点窃取 | `WS_EX_NOACTIVATE` |
| 亚克力效果 | `SetWindowCompositionAttribute`, `ACCENT_ENABLE_ACRYLICBLURBEHIND` |
| 屏幕定位 | `SetWindowPos(HWND_TOPMOST)`, `SystemParametersInfo(SPI_GETWORKAREA)` |
| 窗口检测 | `EnumWindows`, `GetWindowText`, `GetClassName`, `IsWindowVisible` |
| 进程关闭 | `SendMessageTimeout(WM_CLOSE)` |
| 系统动作 | `LockWorkStation`, `SetSuspendState`, `InitiateSystemShutdownEx` |
| 打开资源 | `ShellExecute` |

---

## 7. NuGet 包

| 分类 | 包 |
|------|-----|
| 框架 | `Microsoft.WindowsAppSDK`, `Microsoft.Windows.SDK.BuildTools` |
| MVVM | `CommunityToolkit.Mvvm` |
| UI | `CommunityToolkit.WinUI.UI`, `CommunityToolkit.WinUI.UI.Media` |
| DI | `Microsoft.Extensions.DependencyInjection`, `.Hosting`, `.Logging` |
| 序列化 | `System.Text.Json` |
| 托盘 | `H.NotifyIcon` |
| 日志 | `Serilog.Extensions.Logging`, `Serilog.Sinks.File` |
| 测试 | `xunit`, `FluentAssertions`, `Moq` |
| v2 预留 | `Microsoft.Data.Sqlite` |

---

## 8. 实施顺序 (9 阶段)

| 阶段 | 内容 | 依赖 |
|------|------|------|
| P1: 骨架 | 项目创建, DI, 数据模型, JsonDataStore | — |
| P2: Dock 外壳 | 边缘定位, 展开/收起动画, 亚克力, 边缘检测 | P1 |
| P3: 分类+资源 UI | CategoryList/Tab, ResourcePanel/Tile, 启动服务 | P2 |
| P4: 配置管理 | SettingsWindow, CRUD, 持久化 | P3 |
| P5: 交互模式 | Mode A/B/C 状态机 | P2 |
| P6: 全局热键 | RegisterHotKey, 唤起/关闭 | P2, P5 |
| P7: 工作流引擎 | WorkflowEditor, Engine, 窗口检测, 嵌套 | P3, P4 |
| P8: 打磨 | 动画调优, 系统托盘, 日志, 错误处理 | 全部 |
| P9: 测试 | 单元测试 + 集成测试 | 全部 |

---

## 9. 验证标准

- [ ] `dotnet build` 零错误零警告
- [ ] `dotnet test` 全部通过
- [ ] Dock 在主屏左侧/右侧驻留, 展开/收起动画流畅
- [ ] 三种交互模式可切换, 行为符合定义
- [ ] 全局热键唤起/隐藏正常
- [ ] 资源启动: 应用/文件夹/URL/系统动作
- [ ] 工作流顺序执行, 窗口等待, 失败终止
- [ ] 工作流嵌套无循环引用
- [ ] 亚克力/磨砂效果在 Win11 正常显示
