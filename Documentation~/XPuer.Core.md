# XPuer.Core

[![Version](https://img.shields.io/npm/v/ep.u3d.puer)](https://www.npmjs.com/package/ep.u3d.puer)
[![Downloads](https://img.shields.io/npm/dm/ep.u3d.puer)](https://www.npmjs.com/package/ep.u3d.puer)

XPuer.Core 提供了 JavaScript 虚拟机的运行时环境，支持事件系统管理、生命周期控制和脚本调试等功能。

## 功能特性

- 虚拟机管理：支持 JavaScript 运行时环境的启动和生命周期控制
- 跨语言交互：实现 C# 与 JavaScript 对象及函数的相互操作
- 调试器支持：提供了调试模式和调试器连接功能，可以通过配置启用

## 使用手册

### 1. 初始化流程

#### 1.1 预初始化 (OnPreInit)
- 调用 `handler.OnPreInit()`
- 触发 `XPuer.EventType.OnPreInit` 事件
- 用于准备环境和资源

#### 1.2 虚拟机启动 (OnVMStart)
- 创建并配置 JavaScript 虚拟机
- 调用 `handler.OnVMStart()`
- 触发 `XPuer.EventType.OnVMStart` 事件
- 用于虚拟机配置和初始化

#### 1.3 后初始化 (OnPostInit)
- 加载核心模块
- 调用 `handler.OnPostInit()`
- 触发 `XPuer.EventType.OnPostInit` 事件
- 用于最终配置和模块加载

示例：
```csharp
internal class MyHandler : MonoBehaviour, IHandler
{
    ILoader IHandler.Loader
    {
        get
        {
            // 返回一个实现了ILoader接口的加载器
            return new DefaultLoader();
        }
    }

    IEnumerator IHandler.OnPreInit()
    {
        // 预初始化操作
        yield return null;
    }

    IEnumerator IHandler.OnVMStart()
    {
        // 配置虚拟机
        VM.UsingAction<string, bool>();
        yield return null;
    }

    IEnumerator IHandler.OnPostInit()
    {
        // 后初始化操作
        yield return null;
    }
}

// 启动初始化
StartCoroutine(XPuer.Initialize(new MyHandler()));
```

### 2. 跨语言交互

#### 2.1 创建 JavaScript 对象

```csharp
// 获取JS模块并创建对象
var module = VM.ExecuteModule("MyModule");
var jsClass = module.Get<JSObject>("MyClass");
var jsObject = XPuer.NewObject(jsClass, new object[] { "参数1", 123 });
```

#### 2.2 调用 JavaScript 方法

```csharp
// 调用JS对象的方法
var result = XPuer.FuncApply(jsObject, "myMethod", new object[] { "参数1", 123 });
```

#### 2.3 初始化 JavaScript 字段

```csharp
// 设置JS对象的字段
XPuer.InitField(jsObject, "myField", "字段值", 1 << 2);
```

### 3. 调试器支持

#### 3.1 首选项配置

参考 [XPuer.Prefs](./XPuer.Prefs.md) 中的配置项说明

#### 3.2 VSCode 配置

`.vscode/launch.json` 配置示例：
```json
{
    "configurations": [
        {
            "name": "Attach to JVM",
            "port": 9222,
            "request": "attach",
            "type": "node",
            "pauseForSourceMap": true
        }
    ]
}
```
- 注：pauseForSourceMap 可以防止首次断点时未完整加载 sourceMap 导致无法命中的问题

## 常见问题

更多问题，请查阅[问题反馈](../CONTRIBUTING.md#问题反馈)。

## 项目信息

- [更新记录](../CHANGELOG.md)
- [贡献指南](../CONTRIBUTING.md)
- [许可证](../LICENSE)