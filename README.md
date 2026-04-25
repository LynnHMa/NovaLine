<div align="center">

# NovaLine

**一个基于节点图的 Unity 视觉小说 / 对话系统框架**

**A node-graph based Visual Novel / Dialogue System framework for Unity**

[简体中文](#简体中文) · [English](#english)

</div>

---

# 简体中文

## 简介

1. NovaLine 是一个专为 Unity 设计的可视化对话与剧情系统，通过节点图编辑器编排故事流程，可视化制作分支对话、角色动画、背景切换、音频控制等常见视觉小说功能。系统具备可扩展性，支持自定义 Action、Event、Condition 等逻辑。
2. 该项目仅为 Unity 初学者个人练手项目。

## 功能特性

**节点图编辑器**
- 可视化的 Flowchart / Node / Condition 流程图编辑界面
- 独立的 Command 系统，支持撤销 / 重做（Undo / Redo）
- 自定义资源文件：子父节点图的导出 / 导入

**Action（动作）系统**
- `DialogAction` — 显示 / 隐藏对话框，支持立绘、角色名、逐字打印速度
- `EntityAction` — 显示或动画驱动场景中的角色 / 物件
- `BackgroundAction` — 切换背景图片及其变换属性
- `AudioAction` — 播放、停止、暂停、恢复背景音乐或音效

**Event（事件）系统**
- `ButtonClickedEvent` — 生成选项按钮并等待玩家点击
- `GetKeyEvent` — 等待指定按键按下
- `MouseClickEvent` — 等待鼠标点击
- `WaitingEvent` — 等待指定秒数

**Condition（条件）系统**
- 每个 Node 和 Action 支持前置条件（Before Invoke）与后置条件（After Invoke）
- Node 之间支持切换条件（Switch Condition）
- 条件可组合多个 Event，支持 `All`（全部满足）、`Any`（任一满足）、`Sort`（顺序执行）三种模式

**存档系统**
- 内置 `INovaSaveManager` 与 `INovaSave` 接口，可替换为自定义存档实现
- 示例：记录当前节点 GUID，加载时直接跳转至对应节点

**其他**
- `TransformChecker` 支持预览实体 / 背景在场景中的位置 / 大小 / 角度（Edit Transform）
- `NovaLine\Image\Transition Mask` 提供 3 种遮罩，可搭配 EntityAnim 制作场景过渡

## 项目结构

```
Script/
├── Anim/               # 动画组件（Entity 的淡入淡出、变换动画）
├── Data/               # 序列化数据层（节点、连线、Flowchart 数据）
├── Editor/             # Unity 编辑器扩展（节点图视图、Inspector、文件管理）
├── Element/            # 核心运行时元素
│   ├── Action/         # 内置 Action 实现
│   ├── Event/          # 内置 Event 实现
│   └── Switcher/       # 节点跳转逻辑
├── Registry/           # 全局注册表
├── Save/               # 存档接口与示例实现
├── UI/                 # UI 组件（对话框、按钮、存档菜单）
└── Utils/              # 工具类与扩展方法
```

## 如何扩展

> 注意：项目内置的事件仅为**示例**用途，功能并不完整，如有需要请自行扩展。

**自定义 Action**

继承 `NovaAction` 并重写 `OnInvoke()`：

```csharp
[Serializable]
public class MyCustomAction : NovaAction
{
    public string myParam;

    protected override IEnumerator OnInvoke()
    {
        // 自定义逻辑
        Debug.Log(myParam);
        yield return base.OnInvoke();
    }
}
```

**自定义 Event**

继承 `NovaEvent` 并重写 `OnEvent()`：

```csharp
[Serializable]
public class MyCustomEvent : NovaEvent
{
    public override IEnumerator OnEvent()
    {
        // 自定义逻辑
        yield return new WaitUntil(() => MyCondition());
        yield return base.OnEvent();
    }
}
```

**自定义存档**

实现 `INovaSaveManager` 接口：

```csharp
public class MySaveManager : INovaSaveManager
{
    // 实现 ImportSave、ExportSave、SaveInMenu、LoadInMenu、CreateSave......
}
```

在 `SaveManager.cs` 中替换默认管理器（`NovaLine\Script\Save\SaveManager.cs`）：

```csharp
public static class SaveManager
{
    public static INovaSaveManager Manager { get; private set; } = new MySaveManager();
}
```

## 使用教程

后续会更新简单教程，敬请期待 :)

## 环境要求

- Unity 2021.3 或更高版本（需支持 `UnityEditor.Experimental.GraphView` 与 `SerializeReference`）
- 程序集定义：`NovaLineEditor`（编辑器专用）

## 依赖

- TextMesh Pro

---

# English

## Introduction

1. NovaLine is a visual dialogue and story system for Unity. It uses a node-graph editor to orchestrate story flow, enabling branching dialogue, character animations, background transitions, and audio control — all without writing additional code. The system is extensible, supporting custom Actions, Events, and Conditions.
2. This project is a personal learning exercise by a Unity beginner.

## Features

**Node Graph Editor**
- Visual Flowchart / Node / Condition editor interface
- Independent Command system with full Undo / Redo support
- Custom asset file format: export / import of nested node graphs

**Action System**
- `DialogAction` — Show or hide a dialogue box, with avatar, character name, and typewriter speed
- `EntityAction` — Show or animate characters and objects in the scene
- `BackgroundAction` — Switch background images and their transform properties
- `AudioAction` — Play, stop, pause, or resume background music and sound effects

**Event System**
- `ButtonClickedEvent` — Spawn a choice button and wait for the player to click it
- `GetKeyEvent` — Wait for a specific key to be pressed
- `MouseClickEvent` — Wait for a mouse click
- `WaitingEvent` — Wait for a specified number of seconds

**Condition System**
- Every Node and Action supports a Before Invoke and After Invoke condition
- Transitions between Nodes support a Switch Condition
- Conditions can combine multiple Events using `All`, `Any`, or `Sort` modes

**Save System**
- Built-in `INovaSaveManager` and `INovaSave` interfaces, replaceable with any custom implementation
- Example: records the current Node GUID and restores it on load

**Miscellaneous**
- `TransformChecker` lets you preview entity / background position, scale, and rotation directly in the scene (Edit Transform)
- `NovaLine\Image\Transition Mask` provides 3 mask textures for use with EntityAnim to create scene transitions

## Project Structure

```
Script/
├── Anim/               # Animation components (fade, transform animations for entities)
├── Data/               # Serialized data layer (nodes, edges, flowchart data)
├── Editor/             # Unity editor extensions (graph view, inspector, file management)
├── Element/            # Core runtime elements
│   ├── Action/         # Built-in Action implementations
│   ├── Event/          # Built-in Event implementations
│   └── Switcher/       # Node transition logic
├── Registry/           # Global element registry
├── Save/               # Save interfaces and example implementation
├── UI/                 # UI components (dialogue box, buttons, save menu)
└── Utils/              # Utilities and extension methods
```

## How to Extend

> Note: The built-in events are **examples only** and are not feature-complete. Please extend them as needed.

**Custom Action**

Inherit from `NovaAction` and override `OnInvoke()`:

```csharp
[Serializable]
public class MyCustomAction : NovaAction
{
    public string myParam;

    protected override IEnumerator OnInvoke()
    {
        // Your custom logic
        Debug.Log(myParam);
        yield return base.OnInvoke();
    }
}
```

**Custom Event**

Inherit from `NovaEvent` and override `OnEvent()`:

```csharp
[Serializable]
public class MyCustomEvent : NovaEvent
{
    public override IEnumerator OnEvent()
    {
        // Your custom logic
        yield return new WaitUntil(() => MyCondition());
        yield return base.OnEvent();
    }
}
```

**Custom Save Manager**

Implement the `INovaSaveManager` interface:

```csharp
public class MySaveManager : INovaSaveManager
{
    // Implement ImportSave, ExportSave, SaveInMenu, LoadInMenu, CreateSave......
}
```

Then replace the default manager in `SaveManager.cs` (`NovaLine\Script\Save\SaveManager.cs`):

```csharp
public static class SaveManager
{
    public static INovaSaveManager Manager { get; private set; } = new MySaveManager();
}
```

## Tutorial

A beginner tutorial is coming soon — stay tuned :)

## Requirements

- Unity 2021.3 or later (requires `UnityEditor.Experimental.GraphView` and `SerializeReference` support)
- Assembly Definition: `NovaLineEditor` (editor-only)

## Dependencies

- TextMesh Pro
