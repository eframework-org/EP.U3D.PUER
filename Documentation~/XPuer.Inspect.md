# XPuer.Inspect

[![Version](https://img.shields.io/npm/v/ep.u3d.puer)](https://www.npmjs.com/package/ep.u3d.puer)
[![Downloads](https://img.shields.io/npm/dm/ep.u3d.puer)](https://www.npmjs.com/package/ep.u3d.puer)

XPuer.Inspect 实现了 PuerBehaviour 组件的检视器界面，用于组件的可视化编辑和类型检查。

## 功能特性

- 类型支持：支持编辑 number、boolean、string、Vector2/3/4、Color、Object 等多种数据类型
- 数组管理：提供数组类型的动态添加、删除和编辑功能
- 运行时同步：实现编辑器修改与运行时脚本实例的自动同步
- 类型检查：支持组件引用的类型检查和自动匹配
- 搜索过滤：提供脚本类型的快速搜索和过滤功能

## 使用手册

### 1. 脚本选择
- 使用下拉列表选择要绑定的脚本类型
- 支持搜索框快速过滤可用的脚本类型
- 提供 "Edit" 按钮直接打开源文件进行编辑

### 2. 属性编辑

#### 2.1 基础类型
支持以下基础类型的编辑：
- `number`：数值类型，使用 DoubleField 编辑
- `boolean`：布尔类型，使用 Toggle 编辑
- `string`：字符串类型，使用 TextField 编辑

#### 2.2 Unity 类型
支持以下 Unity 内置类型的编辑：
- `Vector2`：二维向量，使用 Vector2Field 编辑
- `Vector3`：三维向量，使用 Vector3Field 编辑
- `Vector4`：四维向量，使用 Vector4Field 编辑
- `Color`：颜色类型，使用 ColorField 编辑

#### 2.3 引用类型
支持以下引用类型的编辑：
- `UnityEngine.Object`：Unity 对象引用
- `PuerBehaviour`：PuerTS 组件引用

### 3. 数组操作

#### 3.1 数组管理
- 使用折叠面板显示数组内容
- 支持通过输入框或加减按钮调整数组长度
- 提供数组元素的添加和删除功能

#### 3.2 元素编辑
- 支持所有基础类型的数组元素编辑
- 支持所有 Unity 类型的数组元素编辑
- 支持所有引用类型的数组元素编辑
- 提供每个元素的单独删除按钮

### 4. 运行时同步

#### 4.1 值类型同步
- 自动将编辑器中的修改同步到运行时实例
- 支持基础类型和 Unity 类型的实时同步
- 使用二进制序列化确保数据一致性

#### 4.2 引用类型同步
- 自动同步对象引用的修改
- 支持组件引用的动态更新
- 在类型变更时自动处理引用关系

## 常见问题

更多问题，请查阅[问题反馈](../CONTRIBUTING.md#问题反馈)。

## 项目信息

- [更新记录](../CHANGELOG.md)
- [贡献指南](../CONTRIBUTING.md)
- [许可证](../LICENSE)