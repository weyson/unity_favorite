# 常用資源（Unity Editor）

**Languages:** [简体中文](README.md) | [繁體中文](README.zh-TW.md) | [English](README.en.md) | [日本語](README.ja.md) | [한국어](README.ko.md) | [Русский](README.ru.md)

Editor-only 外掛：將 Project 中的資源／資料夾加入常用清單，支援分類、摺疊，點擊即可在 Project 中定位。

## 安裝

### 方式一：嵌入專案

將本倉庫複製到 Unity 專案的 `Packages/com.unityfavorite.favorites/` 目錄。

### 方式二：Git URL（Package Manager）

```
https://gitee.com/weyson_admin/unity_favorite.git
```

若套件在倉庫根目錄，直接新增上述 URL；也可使用本機路徑：`file:D:/workspace/unity_favorite`。

## 使用

1. 開啟視窗：`Window → 常用资源 → 常用资源`
2. **使用說明**：視窗工具列「?」，或 `Window → 常用资源 → 使用说明`（說明視窗可開啟對應語言的 README）
3. **新增**：從 Project 拖曳資源／資料夾到視窗；或在資源上右鍵 → `添加到常用` / `添加到常用（指定分类）`
4. **分類**：工具列「新建分类」；分類標題右鍵可重新命名、刪除
5. **摺疊**：點擊分類前的摺疊箭頭（狀態會保存）
6. **搜尋**：視窗搜尋欄依名稱或路徑過濾；搜尋時自動展開符合的分類
7. **定位／開啟**：單擊列表項在 Project 中 Ping；雙擊開啟資源（資料夾仍為定位）；列內 `↗` 用系統檔案總管開啟（檔案開啟所在目錄）
8. **語言**：工具列右側 Popup 可切換簡體中文／繁體中文／English／日本語／한국어／Русский（保存在 EditorPrefs）
9. **清理**：工具列「清理无效项」移除已刪除資源

## 資料

保存在專案根目錄 `UserSettings/FavoritesData.json`（依使用者、依專案，預設不進版本庫）。引用使用資產 GUID，移動或改名後仍可定位。

## 範圍

- 僅支援 `Assets/` 下的資源與資料夾
- 不含場景 Hierarchy 物件、元件、選單項、外部連結

## 文件

| 語言 | 檔案 |
|------|------|
| 简体中文 | [README.md](README.md) |
| 繁體中文 | [README.zh-TW.md](README.zh-TW.md) |
| English | [README.en.md](README.en.md) |
| 日本語 | [README.ja.md](README.ja.md) |
| 한국어 | [README.ko.md](README.ko.md) |
| Русский | [README.ru.md](README.ru.md) |
