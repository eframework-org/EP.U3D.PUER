# XPuer.Const

[![Version](https://img.shields.io/npm/v/ep.u3d.puer)](https://www.npmjs.com/package/ep.u3d.puer)
[![Downloads](https://img.shields.io/npm/dm/ep.u3d.puer)](https://www.npmjs.com/package/ep.u3d.puer)

XPuer.Const 提供了一些常量定义和运行时环境控制，包括运行配置、资源路径和标签生成等功能。

## 功能特性

- 运行配置：提供发布模式、调试模式、资源路径等配置
- 标签生成：提供资源路径的标准化处理和标签生成

## 使用手册

### 1. 运行配置

#### 1.1 发布模式
- 配置说明：控制是否启用发布模式
- 访问方式：
```csharp
bool isRelease = XPuer.Const.ReleaseMode;
```

#### 1.2 调试模式
- 配置说明：控制是否启用调试模式
- 依赖条件：仅在非发布模式下可用
- 访问方式：
```csharp
bool isDebug = XPuer.Const.DebugMode;
```

#### 1.3 调试等待
- 配置说明：控制是否等待调试器连接
- 依赖条件：仅在调试模式下可用
- 访问方式：
```csharp
bool shouldWait = XPuer.Const.DebugWait;
```

#### 1.4 调试端口
- 配置说明：设置调试器连接端口
- 依赖条件：仅在调试模式下有效，否则返回 -1
- 访问方式：
```csharp
int port = XPuer.Const.DebugPort;
```

#### 1.5 本地路径
- 配置说明：获取脚本资源的本地路径
- 访问方式：
```csharp
string localPath = XPuer.Const.LocalPath;
```

### 2. 标签生成

#### 2.1 默认扩展名
```csharp
public const string Extension = ".jsc";
```

#### 2.2 生成规则
生成资源的标签名称，规则如下：
1. 如果路径不包含斜杠，返回 "default.jsc"
2. 否则使用目录名生成标签：
   - 将路径分隔符替换为下划线
   - 移除特殊字符
   - 转换为小写
   - 添加 .jsc 扩展名

使用示例：
```csharp
string assetPath = "Scripts/Example/Test.ts";
string tag = XPuer.Const.GenTag(assetPath);
// 输出: scripts_example.jsc
```

特殊字符处理规则：
```csharp
{
    "_" -> "_"  // 保留
    " " -> ""   // 移除
    "#" -> ""   // 移除
    "[" -> ""   // 移除
    "]" -> ""   // 移除
}
```

## 常见问题

更多问题，请查阅[问题反馈](../CONTRIBUTING.md#问题反馈)。

## 项目信息

- [更新记录](../CHANGELOG.md)
- [贡献指南](../CONTRIBUTING.md)
- [许可证](../LICENSE)