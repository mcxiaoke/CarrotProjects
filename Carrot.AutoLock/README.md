# Carrot.AutoLock

一个智能的 Windows 工作站自动锁定工具，通过监控目标设备的在线状态（Wi-Fi 和蓝牙）来自动锁定计算机。

## 功能特性

### 核心功能

- **多重设备检测机制**
  - Wi-Fi 网络检测（Ping + ARP 协议）
  - 蓝牙配对设备连接状态检测
  - BLE 低功耗广播扫描（雷达模式）

- **智能锁定逻辑**
  - 设备离线检测（可配置连续离线次数阈值）
  - 用户活动检测（键盘鼠标空闲时间判断）
  - 双重条件验证：仅在设备离线且用户无活动时才锁定

- **多渠道通知**
  - 企业微信机器人通知
  - Telegram Bot 通知
  - 支持同时配置多个通知渠道
  - Markdown 格式消息，包含设备信息和时间

- **系统托盘集成**
  - 最小化到系统托盘运行
  - 托盘图标实时显示运行状态
  - 右键菜单快速操作

- **日志查看器**
  - 实时查看运行日志
  - 自动刷新功能（3秒间隔）
  - 支持复制和清空日志
  - 自动定位最新日志条目

- **开机自启**
  - 支持设置开机自动启动
  - 注册表自动管理

### 技术亮点

- **蓝牙检测方案**
  - 方案一：查询已配对设备的连接状态（最准确）
  - 方案二：BLE 广播扫描（雷达模式，支持距离估算）
  - 基于信号强度（RSSI）的距离判断

- **用户活动监控**
  - 使用 Windows API `GetLastInputInfo` 获取系统空闲时间
  - 无需全局键盘鼠标 Hook，更轻量更安全

- **单实例应用**
  - 基于全局 Mutex 确保单实例运行
  - IPC 命名管道通信，支持唤醒已运行实例

## 使用场景

适用于以下场景：

1. **离开座位自动锁定**：当你带着手机离开工位时，检测到手机蓝牙/Wi-Fi 离线，自动锁定电脑
2. **防止未授权访问**：确保离开时电脑总是处于锁定状态
3. **隐私保护**：无需手动 Win+L，自动完成锁定操作

## 系统要求

- Windows 10 或更高版本
- .NET 10.0 Runtime
- 蓝牙适配器（如需使用蓝牙检测功能）

## 安装与运行

### 编译

```bash
# 克隆仓库
git clone <repository-url>
cd CarrotProjects

# 编译项目
dotnet build Carrot.AutoLock
```

### 运行

```bash
dotnet run --project Carrot.AutoLock
```

或直接运行编译后的可执行文件。

## 配置

### 基本配置

应用程序配置存储在 JSON 文件中，位于 `%LocalAppData%\Carrot\Carrot.AutoLock\config.json`

**配置项说明：**

| 配置项 | 说明 | 示例值 |
|--------|------|--------|
| `TargetIP` | 目标设备 IP 地址 | `192.168.1.100` |
| `TargetBluetoothName` | 目标蓝牙设备名称 | `"My Phone"` |
| `WeChatWebhookKey` | 企业微信机器人 Key | `"xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"` |
| `TelegramBotToken` | Telegram Bot Token | `"123456789:ABCdefGHIjklMNOpqrsTUVwxyz"` |
| `TelegramChatId` | Telegram Chat ID | `"123456789"` |
| `AutoStartEnabled` | 是否开机自启 | `true` 或 `false` |

**配置示例：**

```json
{
  "TargetIP": "192.168.1.100",
  "TargetBluetoothName": "My iPhone",
  "WeChatWebhookKey": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
  "TelegramBotToken": "123456789:ABCdefGHIjklMNOpqrsTUVwxyz",
  "TelegramChatId": "123456789",
  "AutoStartEnabled": true
}
```

### 获取企业微信机器人 Key

1. 在企业微信群聊中，点击群设置 → 群机器人 → 添加机器人
2. 复制机器人的 Webhook 地址中的 `key` 参数
3. 例如：`https://qyapi.weixin.qq.com/cgi-bin/webhook/send?key=xxxxx` 中的 `xxxxx` 部分

### 获取 Telegram Bot Token 和 Chat ID

1. **获取 Bot Token**：
   - 在 Telegram 中搜索 `@BotFather`
   - 发送 `/newbot` 创建新机器人
   - 按提示设置名称，获取 Token

2. **获取 Chat ID**：
   - 向你的 Bot 发送一条消息
   - 访问 `https://api.telegram.org/bot<你的TOKEN>/getUpdates`
   - 在返回的 JSON 中找到 `chat.id` 字段

### 高级参数（需修改代码）

在 `ActiveChecker.cs` 中可调整以下参数：

```csharp
public const string DEFAULT_IP = "192.168.1.100";           // 默认 IP
public const int MAX_OFFLINE_COUNT = 6;                     // 连续离线次数阈值
public const int INACTIVE_SECONDS = 60;                     // 用户空闲时间阈值（秒）
```

在 `BluetoothDetector.cs` 中可调整：

```csharp
IsDeviceNearby(targetId, timeoutSeconds: 30, minRssi: -85); // BLE 检测参数
```

## 工作原理

### 检测流程

```
开始监控
    ↓
检测设备状态
    ├─ Wi-Fi 检测（Ping + ARP）
    │   └─ 在线 → 重置离线计数
    │   └─ 离线 → 进入蓝牙检测
    │
    ├─ 蓝牙配对状态检测
    │   └─ 已连接 → 设备在线
    │   └─ 未连接 → 进入 BLE 扫描
    │
    └─ BLE 广播扫描
        └─ 近期有广播且信号强度足够 → 设备在线
        └─ 未检测到 → 设备离线
    ↓
离线计数 +1
    ↓
检查用户活动状态
    ├─ 有活动 → 继续监控
    └─ 无活动且离线计数达到阈值 → 锁定工作站
```

### 技术架构

- **UI 层**：Windows Forms + 系统托盘
- **业务逻辑**：`ActiveChecker` 核心检测引擎
- **硬件检测**：
  - 网络检测：`System.Net.NetworkInformation.Ping` + ARP 表解析
  - 蓝牙检测：`Windows.Devices.Bluetooth` (Windows Runtime API)
- **IPC 通信**：命名管道（`Carrot.ProCom`）
- **公共组件**：`CarrotCommon` 日志和工具类

## 项目结构

```
Carrot.AutoLock/
├── Program.cs              # 程序入口，单实例管理
├── MainForm.cs             # 主窗体，托盘图标
├── MainForm.Designer.cs    # UI 设计器
├── ActiveChecker.cs        # 核心检测逻辑
├── BluetoothDetector.cs    # 蓝牙设备检测器
├── INotifier.cs            # 通知器接口
├── WeChatNotifier.cs       # 企业微信通知器
├── TelegramNotifier.cs     # Telegram 通知器
├── NotificationManager.cs  # 通知管理器
├── MonitorManager.cs       # 显示器管理（DDC/CI）
├── NetUtils.cs             # 网络工具类（ARP 解析）
├── AppConfig.cs            # 配置管理（JSON）
├── LogViewerForm.cs        # 日志查看器窗体
├── ShortcutHelper.cs       # 快捷方式工具
├── config.example.json     # 配置示例文件
└── Properties/
    ├── Resources.resx      # 资源文件（图标等）
    └── Settings.settings   # 应用程序设置
```

## 依赖项

- **Costura.Fody** (6.0.0)：将依赖项嵌入单个可执行文件
- **MouseKeyHook** (5.7.1)：键盘鼠标钩子库（当前未使用，已被 GetLastInputInfo 替代）
- **CarrotCommon**：公共工具库
- **Carrot.ProCom**：进程间通信库

## 注意事项

1. **蓝牙检测限制**
   - 配对设备检测需要手机蓝牙保持连接状态
   - BLE 扫描需要手机开启蓝牙（无需配对）
   - 部分手机可能使用 MAC 地址随机化，影响 BLE 检测稳定性

2. **网络安全**
   - 需要确保目标设备 IP 地址固定（建议在路由器设置静态 IP）
   - 防火墙可能影响 Ping 检测

3. **性能影响**
   - 检测间隔 3 秒，CPU 占用极低
   - BLE 扫描为后台被动监听，功耗影响小

## 已知问题

- 可空引用类型警告（9个，不影响运行）
- 显示器亮度调整功能已禁用（代码中注释）

## 更新日志

### 最新改进

- ✅ 修复 ARP 解析字节序问题
- ✅ 添加蓝牙双重检测机制
- ✅ 使用 GetLastInputInfo 替代全局 Hook
- ✅ 增强错误处理和日志记录
- ✅ 添加实时 IP 地址验证
- ✅ 改进配置管理机制

详见 [IMPROVEMENTS.md](./IMPROVEMENTS.md)

## 开发计划

- [ ] 添加单元测试
- [ ] 创建设置界面
- [ ] 多语言支持
- [ ] 局域网设备自动发现
- [ ] 增强通知系统

## 许可证

本项目采用 MIT 许可证，详见 [LICENSE.txt](../LICENSE.txt)

## 相关项目

本项目是 CarrotProjects 工具集的一部分，其他项目包括：

- **Carrot.UI**：WPF UI 组件库
- **CarrotNotifier**：通知提醒工具
- **Carrot.Device**：设备管理工具
- **Carrot.Shutdown**：定时关机工具

## 贡献

欢迎提交 Issue 和 Pull Request！

## 作者

CarrotProjects Team
