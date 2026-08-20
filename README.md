# 常用资源（Unity Editor）

**Languages:** [简体中文](README.md) | [繁體中文](README.zh-TW.md) | [English](README.en.md) | [日本語](README.ja.md) | [한국어](README.ko.md) | [Русский](README.ru.md)

Editor-only 插件：将 Project 中的资源/文件夹加入常用列表，支持分类、折叠，点击即可在 Project 中定位。

## 安装

### 方式一：嵌入项目

将本仓库复制到 Unity 项目的 `Packages/com.unityfavorite.favorites/` 目录。

### 方式二：Git URL（Package Manager）

```
https://github.com/weyson/unity_favorite.git
```

若包在仓库根目录，直接添加上述 URL；也可使用本地路径：`file:D:/workspace/unity_favorite`。

## 使用

1. 打开窗口：`Window → 常用资源 → 常用资源`
2. **使用说明**：窗口工具栏「?」，或 `Window → 常用资源 → 使用说明`（说明窗口可打开对应语言的 README）
3. **添加**：从 Project 拖拽资源/文件夹到窗口；或在资源上右键 → `添加到常用` / `添加到常用（指定分类）`
4. **分类**：工具栏「新建分类」；分类标题右键可重命名、删除
5. **折叠**：点击分类前的折叠箭头（状态会保存）
6. **搜索**：窗口搜索栏按名称或路径过滤；搜索时自动展开匹配分类
7. **定位 / 打开**：单击列表项在 Project 中 Ping；双击打开资源（文件夹仍为定位）；行内 `↗` 用系统资源管理器打开（文件打开所在目录）
8. **语言**：工具栏右侧 Popup 可切换简体中文 / 繁体中文 / English / 日本語 / 한국어 / Русский（保存在 EditorPrefs）
9. **清理**：工具栏「清理无效项」移除已删除资源

## 数据

保存在项目根目录 `UserSettings/FavoritesData.json`（按用户、按项目，默认不进版本库）。引用使用资产 GUID，移动或改名后仍可定位。

## 范围

- 仅支持 `Assets/` 下的资源与文件夹
- 不含场景 Hierarchy 物体、菜单项、外部链接

## 文档

| 语言 | 文件 |
|------|------|
| 简体中文 | [README.md](README.md) |
| 繁體中文 | [README.zh-TW.md](README.zh-TW.md) |
| English | [README.en.md](README.en.md) |
| 日本語 | [README.ja.md](README.ja.md) |
| 한국어 | [README.ko.md](README.ko.md) |
| Русский | [README.ru.md](README.ru.md) |
