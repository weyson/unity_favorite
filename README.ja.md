# お気に入り（Unity Editor）

**Languages:** [简体中文](README.md) | [繁體中文](README.zh-TW.md) | [English](README.en.md) | [日本語](README.ja.md) | [한국어](README.ko.md) | [Русский](README.ru.md)

Editor 専用パッケージ：Project のアセット／フォルダをお気に入り登録し、グループ化・折りたたみ・すばやいフォーカスに対応します。

## インストール

### 方法 1：プロジェクトに埋め込む

本リポジトリを Unity プロジェクトの `Packages/com.unityfavorite.favorites/` に配置します。

### 方法 2：Git URL（Package Manager）

```
https://gitee.com/weyson_admin/unity_favorite.git
```

パッケージがリポジトリ直下にある場合は上記 URL を追加します。ローカルパス `file:D:/workspace/unity_favorite` も利用できます。

## 使い方

1. ウィンドウを開く：`Window → 常用资源 → 常用资源`
2. **ヘルプ**：ツールバーの「?」、または `Window → 常用资源 → 使用说明`（ヘルプから現在言語の README を開けます）
3. **追加**：Project からドラッグ、または右クリック → `添加到常用` / `添加到常用（指定分类）`
4. **グループ**：ツールバー「新規グループ」；見出し右クリックで名前変更／削除
5. **折りたたみ**：矢印で開閉（状態は保存されます）
6. **検索**：名前／パスで絞り込み；検索中は一致グループが展開されます
7. **フォーカス／開く**：クリックで Ping；ダブルクリックで開く（フォルダは Ping）；行の `↗` でエクスプローラーを開く（ファイルは親フォルダ）
8. **言語**：ツールバーの Popup（簡体字／繁体字／English／日本語／한국어／Русский、EditorPrefs に保存）
9. **クリーンアップ**：「欠落を削除」で Missing を一括除去

## データ

プロジェクト直下の `UserSettings/FavoritesData.json`（ユーザー／プロジェクト単位、通常は VCS 対象外）。GUID 参照のため移動・改名後も有効です。

## 対応範囲

- `Assets/` 配下のアセットとフォルダのみ
- Hierarchy オブジェクト、コンポーネント、メニュー項目、外部ファイル／URL は非対応

## ドキュメント

| 言語 | ファイル |
|------|----------|
| 简体中文 | [README.md](README.md) |
| 繁體中文 | [README.zh-TW.md](README.zh-TW.md) |
| English | [README.en.md](README.en.md) |
| 日本語 | [README.ja.md](README.ja.md) |
| 한국어 | [README.ko.md](README.ko.md) |
| Русский | [README.ru.md](README.ru.md) |
