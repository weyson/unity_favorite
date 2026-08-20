# Favorites (Unity Editor)

**Languages:** [简体中文](README.md) | [繁體中文](README.zh-TW.md) | [English](README.en.md) | [日本語](README.ja.md) | [한국어](README.ko.md) | [Русский](README.ru.md)

Editor-only package: bookmark Project assets and folders, organize them by category, fold groups, and jump to assets quickly.

## Installation

### Option 1: Embed in the project

Copy this repository into your Unity project at `Packages/com.unityfavorite.favorites/`.

### Option 2: Git URL (Package Manager)

```
https://github.com/weyson/unity_favorite.git
```

If the package root is the repository root, paste that URL in Package Manager. You can also use a local path such as `file:D:/workspace/unity_favorite`.

## Usage

1. Open the window: `Window → 常用资源 → 常用资源`
2. **Help**: click `?` on the toolbar, or `Window → 常用资源 → 使用说明` (the help window can open the README for the current language)
3. **Add**: drag assets/folders from Project into the window; or right-click → `添加到常用` / `添加到常用（指定分类）`
4. **Categories**: toolbar “New Category”; right-click a category header to rename or delete
5. **Fold**: use the foldout arrow (state is persisted)
6. **Search**: filter by name or path; matching categories expand while searching
7. **Locate / open**: click to ping in Project; double-click to open (folders still ping); row `↗` opens the system file manager (for files, opens the parent folder)
8. **Language**: toolbar popup — Simplified Chinese / Traditional Chinese / English / 日本語 / 한국어 / Русский (saved in EditorPrefs)
9. **Cleanup**: toolbar “Clean Missing” removes deleted assets from the list

## Data

Stored at `UserSettings/FavoritesData.json` in the project root (per-user, per-project; usually not versioned). References use asset GUIDs, so moves and renames keep working.

## Scope

- Only assets and folders under `Assets/`
- No Hierarchy objects, components, menu items, or external files/URLs

## Documentation

| Language | File |
|----------|------|
| 简体中文 | [README.md](README.md) |
| 繁體中文 | [README.zh-TW.md](README.zh-TW.md) |
| English | [README.en.md](README.en.md) |
| 日本語 | [README.ja.md](README.ja.md) |
| 한국어 | [README.ko.md](README.ko.md) |
| Русский | [README.ru.md](README.ru.md) |
