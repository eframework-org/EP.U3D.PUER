# XPuer.Gen

[![Version](https://img.shields.io/npm/v/ep.u3d.puer)](https://www.npmjs.com/package/ep.u3d.puer)
[![Downloads](https://img.shields.io/npm/dm/ep.u3d.puer)](https://www.npmjs.com/package/ep.u3d.puer)

XPuer.Gen 提供了代码生成工具，支持 PuerTS 绑定代码的生成、模块导出和自动安装等功能。

## 功能特性

- 类型绑定：支持自定义类型的动态绑定和过滤，包括 Unity 导出类型和自定义程序集类型
- 模块导出：支持通过 UPM 定义 puerExports/puerAdapter 或代码定义等方式将模块导出为 NPM 包

## 使用手册

### 1. 首选项配置

代码生成工具提供了以下配置选项：

#### 输出路径
- 配置键：`Puer/Gen/Output@Editor`
- 默认值：`Assets/Plugins/Puer/Gen/`
- 功能说明：
  - 设置生成代码的输出目录

#### 自动生成
- 配置键：`Puer/Gen/Auto/Gen@Editor`
- 默认值：`true`
- 功能说明：
  - 控制项目加载时是否自动生成代码
  - 首次加载或输出目录为空时会触发生成
  - 通过 `Tools/PuerTS/Generate (all in one)` 执行生成

#### 自动安装
- 配置键：`Puer/Gen/Auto/Install@Editor`
- 默认值：`true`
- 功能说明：
  - 控制项目加载时是否自动安装 NPM 模块
  - 检测到 package-lock.json 变化时会触发安装
  - 自动执行 `npm install` 安装依赖

以上配置项均可在 `EFramework/Preferences/Puer/Gen` 首选项编辑器中进行可视化配置。

### 2. 类型绑定

#### 2.1 类型定义
配置需要生成的类型：
```csharp
// 配置需要生成类型定义的类型
XPuer.Gen.Types = new List<Type>
{
    typeof(UnityEngine.GameObject),
    typeof(UnityEngine.Transform),
    typeof(Your.Game.PlayerController)
};

// 配置需要生成 Blittable 拷贝的类型
XPuer.Gen.Blittables = new List<Type>
{
    typeof(UnityEngine.Vector3),
    typeof(UnityEngine.Quaternion)
};

// 配置需要生成绑定代码的类型
XPuer.Gen.Bindings = new List<Type>
{
    typeof(Your.Game.NetworkManager),
    typeof(Your.Game.UIManager)
};
```

#### 2.2 类型过滤
配置需要排除的类型：
```csharp
// 配置需要排除的程序集
XPuer.Gen.ExcludeAssemblys = new List<string>
{
    "Assembly-CSharp-Editor.dll",
    "Your.Game.Editor.dll"
};

// 配置需要排除的类型
XPuer.Gen.ExcludeTypes = new List<string>
{
    "Your.Game.Internal.DebugHelper",
    "Your.Game.Editor.*"  // 使用通配符排除所有 Editor 命名空间下的类型
};

// 配置需要排除的成员
XPuer.Gen.ExcludeMembers = new List<List<string>>
{
    // 排除 GameObject 的 SetActive 方法
    new List<string> { "UnityEngine.GameObject", "SetActive", "System.Boolean" },
    
    // 排除所有参数的 Transform.Translate 方法
    new List<string> { "UnityEngine.Transform", "Translate", "*" }
};
```

### 3. 模块导出

#### 3.1 适配器

在 UPM 包中配置适配器：

```json
{
  "name": "your.package.name",
  "version": "1.0.0",
  "puerAdapter": ".puer"  // 推荐使用 .puer 目录
}
```

适配器目录结构：
```
your.package.name/
  ├── .puer/              # 推荐使用此目录名
  │   ├── package.json    # 适配器包配置
  │   ├── index.js        # JavaScript 模块入口
  │   └── index.d.ts      # TypeScript 声明文件
  └── package.json        # UPM 包配置
```

建议说明：
- 将适配器相关文件集中在 `.puer` 目录下，便于识别和管理
- 遵循 NPM 包标准结构，确保模块正确导出

#### 3.2 命名空间

##### 3.2.1 代码配置
通过 `Namespaces` 属性配置需要导出的命名空间：
```csharp
// 添加自定义命名空间
XPuer.Gen.Namespaces = new List<string>
{
    "Your.Game.Core",
    "Your.Game.UI",
    "Your.Game.Network"
};

// 获取当前配置的命名空间列表（包含 UPM 包中的配置）
var namespaces = XPuer.Gen.Namespaces;
foreach (var ns in namespaces)
{
    Debug.Log($"Namespace to export: {ns}");
}
```

##### 3.2.2 UPM 包配置
在 UPM 包的 `package.json` 中配置 puerExports 字段：
```json
{
  "name": "your.package.name",
  "version": "1.0.0",
  "puerExports": [
    "Your.Namespace.Name",
    "Your.Another.Namespace"
  ]
}
```

#### 3.3 代码生成

##### 3.3.1 生成命令
- 手动生成：通过菜单项 `Tools/PuerTS/Generate Module` 触发
- 自动生成：项目加载时根据 `Puer/Gen/Auto/Gen@Editor` 配置自动执行
- 自动安装：项目加载时根据 `Puer/Gen/Auto/Install@Editor` 配置自动执行

##### 3.3.2 输出结构
生成的模块位于 `node_modules/.puer/` 目录：
```
node_modules/.puer/<namespace>/
  ├── package.json      # NPM 包配置
  ├── index.js         # JavaScript 模块代码
  └── index.d.ts       # TypeScript 声明文件
```

##### 3.3.3 增量更新
更新检测：
- 监控 `package-lock.json` 和 `packages-lock.json` 的 MD5 值变化
- 根据变化决定是否需要重新生成和安装
- 对已存在的文件进行内容合并，保持现有导出不丢失

## 常见问题

更多问题，请查阅[问题反馈](../CONTRIBUTING.md#问题反馈)。

## 项目信息

- [更新记录](../CHANGELOG.md)
- [贡献指南](../CONTRIBUTING.md)
- [许可证](../LICENSE)