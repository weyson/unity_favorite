using System;
using UnityEditor;
using UnityEngine;

namespace UnityFavorite.Favorites
{
    public enum FavoriteLanguage
    {
        ZhCn = 0,
        ZhTw = 1,
        En = 2,
        Ja = 3,
        Ko = 4,
        Ru = 5
    }

    public static class Loc
    {
        const string PrefsKey = "UnityFavorite.Favorites.Language";

        static FavoriteLanguage _language;
        static bool _initialized;

        public static event Action LanguageChanged;

        public static FavoriteLanguage Language
        {
            get
            {
                EnsureInit();
                return _language;
            }
        }

        public static readonly string[] DisplayNames =
        {
            "简体中文",
            "繁體中文",
            "English",
            "日本語",
            "한국어",
            "Русский"
        };

        public static void EnsureInit()
        {
            if (_initialized)
                return;

            if (EditorPrefs.HasKey(PrefsKey))
                _language = (FavoriteLanguage)Mathf.Clamp(EditorPrefs.GetInt(PrefsKey), 0, DisplayNames.Length - 1);
            else
                _language = DetectSystemLanguage();

            _initialized = true;
        }

        public static void SetLanguage(FavoriteLanguage language)
        {
            EnsureInit();
            if (_language == language)
                return;

            _language = language;
            EditorPrefs.SetInt(PrefsKey, (int)language);
            LanguageChanged?.Invoke();
        }

        public static string T(string key)
        {
            EnsureInit();
            if (TryGet(key, _language, out var value))
                return value;
            if (TryGet(key, FavoriteLanguage.En, out value))
                return value;
            return key;
        }

        public static string Tf(string key, params object[] args)
        {
            try
            {
                return string.Format(T(key), args);
            }
            catch (FormatException)
            {
                return T(key);
            }
        }

        static FavoriteLanguage DetectSystemLanguage()
        {
            switch (Application.systemLanguage)
            {
                case SystemLanguage.Chinese:
                case SystemLanguage.ChineseSimplified:
                    return FavoriteLanguage.ZhCn;
                case SystemLanguage.ChineseTraditional:
                    return FavoriteLanguage.ZhTw;
                case SystemLanguage.Japanese:
                    return FavoriteLanguage.Ja;
                case SystemLanguage.Korean:
                    return FavoriteLanguage.Ko;
                case SystemLanguage.Russian:
                    return FavoriteLanguage.Ru;
                case SystemLanguage.English:
                    return FavoriteLanguage.En;
                default:
                    return FavoriteLanguage.ZhCn;
            }
        }

        static bool TryGet(string key, FavoriteLanguage language, out string value)
        {
            value = null;
            if (!Table.TryGetValue(key, out var row))
                return false;
            if ((int)language < 0 || (int)language >= row.Length)
                return false;
            value = row[(int)language];
            return !string.IsNullOrEmpty(value);
        }

        // Order: ZhCn, ZhTw, En, Ja, Ko, Ru
        static readonly System.Collections.Generic.Dictionary<string, string[]> Table =
            new System.Collections.Generic.Dictionary<string, string[]>
            {
                ["window_title"] = new[]
                {
                    "常用资源", "常用資源", "Favorites", "お気に入り", "즐겨찾기", "Избранное"
                },
                ["new_category"] = new[]
                {
                    "新建分类", "新建分類", "New Category", "新規グループ", "새 분류", "Новая категория"
                },
                ["cleanup"] = new[]
                {
                    "清理无效项", "清理無效項", "Clean Missing", "欠落を削除", "유효하지 않음 정리", "Очистить"
                },
                ["ok"] = new[]
                {
                    "确定", "確定", "OK", "OK", "확인", "ОК"
                },
                ["cancel"] = new[]
                {
                    "取消", "取消", "Cancel", "キャンセル", "취소", "Отмена"
                },
                ["delete"] = new[]
                {
                    "删除", "刪除", "Delete", "削除", "삭제", "Удалить"
                },
                ["search"] = new[]
                {
                    "搜索", "搜尋", "Search", "検索", "검색", "Поиск"
                },
                ["language"] = new[]
                {
                    "语言", "語言", "Language", "言語", "언어", "Язык"
                },
                ["default_category"] = new[]
                {
                    "未分类", "未分類", "Uncategorized", "未分類", "미분류", "Без категории"
                },
                ["new_category_name"] = new[]
                {
                    "新分类", "新分類", "New Category", "新しいグループ", "새 분류", "Новая категория"
                },
                ["unnamed_category"] = new[]
                {
                    "未命名分类", "未命名分類", "Untitled", "無題", "이름 없음", "Без названия"
                },
                ["cleaned_count"] = new[]
                {
                    "已清理 {0} 个无效项。", "已清理 {0} 個無效項。", "Removed {0} missing item(s).",
                    "{0} 件の欠落を削除しました。", "유효하지 않은 항목 {0}개를 정리했습니다.", "Удалено отсутствующих: {0}."
                },
                ["cleaned_none"] = new[]
                {
                    "没有无效项。", "沒有無效項。", "No missing items.", "欠落はありません。", "정리할 항목이 없습니다.", "Нет отсутствующих элементов."
                },
                ["no_search_results"] = new[]
                {
                    "未找到匹配「{0}」的常用项。", "未找到符合「{0}」的常用項。", "No favorites matching \"{0}\".",
                    "「{0}」に一致するお気に入りはありません。", "「{0}」와(과) 일치하는 즐겨찾기가 없습니다.", "Нет избранного по запросу «{0}»."
                },
                ["empty_hint"] = new[]
                {
                    "暂无常用项。\n可将 Project 中的资源拖到此处，或右键资源选择「添加到常用」。\n单击定位，双击打开。",
                    "暫無常用項。\n可將 Project 中的資源拖到此處，或右鍵資源選擇「新增到常用」。\n單擊定位，雙擊開啟。",
                    "No favorites yet.\nDrag assets from the Project window here, or use the Assets context menu.\nClick to ping, double-click to open.",
                    "お気に入りはまだありません。\nProject からここにドラッグするか、Assets のコンテキストメニューから追加できます。\nクリックでフォーカス、ダブルクリックで開く。",
                    "즐겨찾기가 없습니다.\nProject에서 여기로 드래그하거나 Assets 우클릭 메뉴로 추가하세요.\n클릭하여 찾기, 더블클릭하여 열기.",
                    "Пока нет избранного.\nПеретащите ресурсы из Project или добавьте через контекстное меню Assets.\nКлик — найти, двойной клик — открыть."
                },
                ["empty_hint_short"] = new[]
                {
                    "暂无常用项。\n可将 Project 中的资源拖到此处，\n或右键资源选择「添加到常用」。",
                    "暫無常用項。\n可將 Project 中的資源拖到此處，\n或右鍵資源選擇「新增到常用」。",
                    "No favorites yet.\nDrag assets here, or use the Assets context menu.",
                    "お気に入りはまだありません。\nここにドラッグするか、コンテキストメニューから追加。",
                    "즐겨찾기가 없습니다.\n여기로 드래그하거나 우클릭 메뉴로 추가하세요.",
                    "Пока нет избранного.\nПеретащите сюда или добавьте через меню Assets."
                },
                ["empty_category"] = new[]
                {
                    "（空）将资源拖到此处", "（空）將資源拖到此處", "(Empty) Drop assets here",
                    "（空）ここにドロップ", "(비어 있음) 여기에 드롭", "(Пусто) Перетащите сюда"
                },
                ["remove_tooltip"] = new[]
                {
                    "从常用中移除", "從常用中移除", "Remove from favorites", "お気に入りから削除", "즐겨찾기에서 제거", "Убрать из избранного"
                },
                ["ping"] = new[]
                {
                    "定位资源", "定位資源", "Ping Asset", "アセットをフォーカス", "에셋 찾기", "Показать ресурс"
                },
                ["open"] = new[]
                {
                    "打开资源", "開啟資源", "Open Asset", "アセットを開く", "에셋 열기", "Открыть ресурс"
                },
                ["reveal"] = new[]
                {
                    "在资源管理器中显示", "在檔案總管中顯示", "Show in Explorer", "エクスプローラーで表示", "탐색기에서 표시", "Показать в проводнике"
                },
                ["move_to_category"] = new[]
                {
                    "移到分类/{0}", "移到分類/{0}", "Move to/{0}", "グループへ移動/{0}", "분류로 이동/{0}", "Переместить/{0}"
                },
                ["remove"] = new[]
                {
                    "移除", "移除", "Remove", "削除", "제거", "Удалить"
                },
                ["rename"] = new[]
                {
                    "重命名", "重新命名", "Rename", "名前を変更", "이름 바꾸기", "Переименовать"
                },
                ["delete_category_move"] = new[]
                {
                    "删除分类（项移到其他分类）", "刪除分類（項目移到其他分類）", "Delete Category (move items)",
                    "グループを削除（項目を移動）", "분류 삭제(항목 이동)", "Удалить категорию (перенести)"
                },
                ["delete_category_items"] = new[]
                {
                    "删除分类及其中的项", "刪除分類及其中的項目", "Delete Category and Items",
                    "グループと項目を削除", "분류와 항목 삭제", "Удалить категорию и элементы"
                },
                ["delete_category_disabled"] = new[]
                {
                    "删除分类（至少保留一个）", "刪除分類（至少保留一個）", "Delete Category (keep at least one)",
                    "グループ削除（最低1つ必要）", "분류 삭제(최소 1개 유지)", "Удалить категорию (оставьте одну)"
                },
                ["confirm_delete_category_move"] = new[]
                {
                    "删除分类「{0}」？其中的常用项将移到其他分类。",
                    "刪除分類「{0}」？其中的常用項將移到其他分類。",
                    "Delete category \"{0}\"? Items will be moved to another category.",
                    "グループ「{0}」を削除しますか？項目は別のグループへ移動します。",
                    "분류 「{0}」을(를) 삭제할까요? 항목은 다른 분류로 이동합니다.",
                    "Удалить категорию «{0}»? Элементы будут перенесены."
                },
                ["confirm_delete_category_items"] = new[]
                {
                    "删除分类「{0}」及其全部常用项？",
                    "刪除分類「{0}」及其全部常用項？",
                    "Delete category \"{0}\" and all its favorites?",
                    "グループ「{0}」とその項目をすべて削除しますか？",
                    "분류 「{0}」과(와) 모든 즐겨찾기를 삭제할까요?",
                    "Удалить категорию «{0}» и все её элементы?"
                },
                ["log_prefix"] = new[]
                {
                    "常用资源", "常用資源", "Favorites", "お気に入り", "즐겨찾기", "Избранное"
                },
                ["log_already_added"] = new[]
                {
                    "[{0}] 已在常用列表中: {1}", "[{0}] 已在常用清單中: {1}", "[{0}] Already in favorites: {1}",
                    "[{0}] すでにお気に入りです: {1}", "[{0}] 이미 즐겨찾기에 있습니다: {1}", "[{0}] Уже в избранном: {1}"
                },
                ["log_nothing_to_add"] = new[]
                {
                    "[{0}] 没有可添加的资源（可能已存在或不在 Assets 下）",
                    "[{0}] 沒有可新增的資源（可能已存在或不在 Assets 下）",
                    "[{0}] Nothing to add (duplicates or not under Assets/)",
                    "[{0}] 追加できるアセットがありません（重複、または Assets 外）",
                    "[{0}] 추가할 에셋이 없습니다(중복이거나 Assets 밖)",
                    "[{0}] Нечего добавить (дубликаты или не в Assets/)"
                },
                ["log_keep_one_category"] = new[]
                {
                    "[{0}] 至少保留一个分类", "[{0}] 至少保留一個分類", "[{0}] Keep at least one category",
                    "[{0}] グループは最低1つ必要です", "[{0}] 분류를 최소 1개 유지하세요", "[{0}] Оставьте хотя бы одну категорию"
                },
                ["log_missing_asset"] = new[]
                {
                    "[{0}] 资源已丢失，请清理无效项", "[{0}] 資源已遺失，請清理無效項", "[{0}] Asset missing — clean invalid items",
                    "[{0}] アセットが見つかりません。欠落を削除してください", "[{0}] 에셋을 찾을 수 없습니다. 정리하세요", "[{0}] Ресурс отсутствует — очистите список"
                },
                ["log_corrupt"] = new[]
                {
                    "[{0}] 数据文件损坏，已重建。原因: {1}", "[{0}] 資料檔案損壞，已重建。原因: {1}",
                    "[{0}] Data file corrupt, rebuilt. Reason: {1}",
                    "[{0}] データが破損したため再作成しました。理由: {1}",
                    "[{0}] 데이터 파일이 손상되어 재생성했습니다. 원인: {1}",
                    "[{0}] Файл данных повреждён, пересоздан. Причина: {1}"
                },
                ["log_backup_failed"] = new[]
                {
                    "[{0}] 备份失败: {1}", "[{0}] 備份失敗: {1}", "[{0}] Backup failed: {1}",
                    "[{0}] バックアップ失敗: {1}", "[{0}] 백업 실패: {1}", "[{0}] Ошибка резервной копии: {1}"
                }
            };
    }
}
