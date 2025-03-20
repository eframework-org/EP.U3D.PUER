# XPuer.Prefs

[![Version](https://img.shields.io/npm/v/ep.u3d.puer)](https://www.npmjs.com/package/ep.u3d.puer)
[![Downloads](https://img.shields.io/npm/dm/ep.u3d.puer)](https://www.npmjs.com/package/ep.u3d.puer)

XPuer.Prefs 提供了运行时的首选项管理，用于控制运行模式、调试选项和资源路径等配置项。

## 功能特性

- 运行模式配置：提供发布模式和调试模式的切换
- 调试选项管理：支持调试等待和端口设置
- 资源路径配置：支持配置内置、本地和远端资源路径
- 可视化配置界面：在 Unity 编辑器中提供直观的设置面板

## 使用手册

### 1. 运行模式

| 配置项 | 配置键 | 默认值 | 功能说明 |
|--------|--------|--------|----------|
| 发布模式 | `Puer/ReleaseMode` | `false` | 控制是否启用发布模式，启用后将禁用所有调试相关功能，用于生产环境部署 |
| 调试模式 | `Puer/DebugMode` | `false` | 控制是否启用调试模式，仅在非发布模式下可用，启用后可连接调试器进行调试 |

### 2. 调试选项

| 配置项 | 配置键 | 默认值 | 功能说明 |
|--------|--------|--------|----------|
| 调试等待 | `Puer/DebugWait` | `true` | 控制是否等待调试器连接，仅在调试模式下可用，启用后程序将等待调试器连接后再继续执行 |
| 调试端口 | `Puer/DebugPort` | `9222` | 设置调试器连接的端口号，仅在调试模式下可用，确保端口未被其他程序占用 |

### 3. 资源路径

| 配置项 | 配置键 | 默认值 | 功能说明 |
|--------|--------|--------|----------|
| 内置资源 | `Puer/AssetUri` | `Patch@Scripts@TS.zip` | 设置脚本包的内置路径，用于打包时将资源内置于安装包内 |
| 本地资源 | `Puer/LocalUri` | `Scripts/TS` | 设置脚本包的本地路径，用于运行时的加载 |
| 远端资源 | `Puer/RemoteUri` | `${Prefs.Update/PatchUri}/Scripts/TS` | 设置脚本包的远端路径，用于资源的下载 |

以上配置项均可在 `EFramework/Preferences/Puer` 首选项编辑器中进行可视化配置。

## 常见问题

更多问题，请查阅[问题反馈](../CONTRIBUTING.md#问题反馈)。

## 项目信息

- [更新记录](../CHANGELOG.md)
- [贡献指南](../CONTRIBUTING.md)
- [许可证](../LICENSE)