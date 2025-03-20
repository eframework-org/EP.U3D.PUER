# XPuer.Comp

[![Version](https://img.shields.io/npm/v/ep.u3d.puer)](https://www.npmjs.com/package/ep.u3d.puer)
[![Downloads](https://img.shields.io/npm/dm/ep.u3d.puer)](https://www.npmjs.com/package/ep.u3d.puer)

XPuer.Comp 提供了 MonoBehaviour 的 TypeScript 扩展实现，用于管理组件的生命周期、序列化和事件系统。

## 功能特性

- 支持 TypeScript 类的序列化：通过 `Field` 类提供字段序列化能力
- 实现 Unity 生命周期管理：支持 `Awake`、`Start`、`Update` 等标准事件
- 提供物理事件系统：支持 `OnTrigger` 和 `OnCollision` 系列事件
- 支持动态组件添加：通过 `Add` 方法实现运行时组件创建
- 实现组件查找机制：支持在父子层级中查找指定类型组件

## 使用手册

### 1. 组件管理

#### 1.1 添加组件
通过 `Add` 方法在运行时动态添加组件到游戏对象。

```csharp
// parentObj: 父节点对象
// path: 节点路径，空字符串表示当前节点
// type: TypeScript 类型对象
var comp = PuerBehaviour.Add(parentObj, path, type);
```

#### 1.2 获取组件
提供多种方式获取组件实例：

```csharp
// 获取当前节点组件
PuerBehaviour comp = PuerBehaviour.Get(gameObject, type);

// 获取父节点组件（包含自身）
PuerBehaviour parent = PuerBehaviour.GetInParent(gameObject, type, includeInactive);

// 获取子节点组件（包含自身）
PuerBehaviour child = PuerBehaviour.GetInChildren(gameObject, type, includeInactive);

// 获取多个组件
PuerBehaviour[] comps = PuerBehaviour.Gets(gameObject, type);
PuerBehaviour[] parents = PuerBehaviour.GetsInParent(gameObject, type, includeInactive);
PuerBehaviour[] children = PuerBehaviour.GetsInChildren(gameObject, type, includeInactive);
```

### 2. 序列化系统

#### 2.1 字段序列化
支持以下类型的序列化：

```csharp
public class Field
{
    public string Key;                     // 字段名称
    public string Type;                    // 字段类型
    public UnityEngine.Object OValue;      // Object 类型值
    public byte[] BValue;                  // 值类型数据
    public List<UnityEngine.Object> LOValue; // Object 数组
    public List<Byte> LBValue;            // 值类型数组
}
```

### 3. 生命周期

#### 3.1 标准事件
自动检测并调用 TypeScript 类中定义的生命周期方法：

- `Awake`：组件初始化时
- `Start`：首次启用时
- `OnEnable`：启用时
- `OnDisable`：禁用时
- `Update`：每帧更新时
- `LateUpdate`：所有更新完成后
- `FixedUpdate`：固定时间间隔
- `OnDestroy`：销毁时

#### 3.2 物理事件
自动包装并转发物理碰撞事件：

- `OnTriggerEnter`：触发器进入
- `OnTriggerStay`：触发器停留
- `OnTriggerExit`：触发器退出
- `OnCollisionEnter`：碰撞开始
- `OnCollisionStay`：碰撞持续
- `OnCollisionExit`：碰撞结束

## 常见问题

更多问题，请查阅[问题反馈](../CONTRIBUTING.md#问题反馈)。

## 项目信息

- [更新记录](../CHANGELOG.md)
- [贡献指南](../CONTRIBUTING.md)
- [许可证](../LICENSE)