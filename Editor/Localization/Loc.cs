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
                ["open_in_explorer"] = new[]
                {
                    "在资源管理器中打开", "在檔案總管中開啟", "Open in Explorer", "エクスプローラーで開く", "탐색기에서 열기", "Открыть в проводнике"
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
                },
                ["help_title"] = new[]
                {
                    "使用说明", "使用說明", "Help", "ヘルプ", "사용 설명", "Справка"
                },
                ["help_tooltip"] = new[]
                {
                    "打开使用说明", "開啟使用說明", "Open help", "ヘルプを開く", "사용 설명 열기", "Открыть справку"
                },
                ["help_intro"] = new[]
                {
                    "将常用的 Project 资源集中管理，支持分类、搜索与快速定位。以下为完整使用说明。",
                    "將常用的 Project 資源集中管理，支援分類、搜尋與快速定位。以下為完整使用說明。",
                    "Manage frequently used Project assets with categories, search, and quick navigation. Full guide below.",
                    "よく使う Project アセットをグループ化・検索・すばやく開けます。以下が使い方です。",
                    "자주 쓰는 Project 에셋을 분류·검색·빠르게 찾을 수 있습니다. 아래는 사용 설명입니다.",
                    "Управляйте часто используемыми ресурсами Project: категории, поиск и быстрый доступ. Инструкция ниже."
                },
                ["help_open_title"] = new[]
                {
                    "打开窗口", "開啟視窗", "Open Window", "ウィンドウを開く", "창 열기", "Открыть окно"
                },
                ["help_open_body"] = new[]
                {
                    "菜单：Window → 常用资源 → 常用资源。\n也可在常用资源窗口工具栏点击「?」打开本说明。",
                    "選單：Window → 常用資源 → 常用資源。\n也可在常用資源視窗工具列點「?」開啟本說明。",
                    "Menu: Window → Favorites → Favorites.\nOr click 「?」 on the Favorites toolbar to open this help.",
                    "メニュー：Window → 常用资源 → 常用资源。\nツールバーの「?」からもこのヘルプを開けます。",
                    "메뉴: Window → 常用资源 → 常用资源.\n즐겨찾기 툴바의 「?」로도 이 설명을 열 수 있습니다.",
                    "Меню: Window → 常用资源 → 常用资源.\nТакже можно нажать 「?」 на панели Избранного."
                },
                ["help_add_title"] = new[]
                {
                    "添加常用项", "新增常用項", "Add Favorites", "追加", "즐겨찾기 추가", "Добавление"
                },
                ["help_add_body"] = new[]
                {
                    "• 从 Project 将资源或文件夹拖到常用资源窗口。\n• 在资源上右键：Assets → 添加到常用 / 添加到常用（指定分类）。\n• 同一资源不会重复添加（按 GUID 去重）。\n• 仅支持 Assets/ 下的资源与文件夹。",
                    "• 從 Project 將資源或資料夾拖到常用資源視窗。\n• 在資源上右鍵：Assets → 新增到常用 / 新增到常用（指定分類）。\n• 同一資源不會重複新增（以 GUID 去重）。\n• 僅支援 Assets/ 下的資源與資料夾。",
                    "• Drag assets or folders from Project into the Favorites window.\n• Right-click assets: Assets → Add to Favorites / Add to Category.\n• Duplicates are ignored (GUID-based).\n• Only assets and folders under Assets/ are supported.",
                    "• Project からアセット／フォルダをウィンドウへドラッグ。\n• 右クリック：Assets → 添加到常用 / 添加到常用（指定分类）。\n• 同一 GUID は重複追加されません。\n• Assets/ 配下のみ対応。",
                    "• Project에서 에셋/폴더를 창으로 드래그.\n• 우클릭: Assets → 添加到常用 / 添加到常用（指定分类）.\n• 동일 GUID는 중복 추가되지 않습니다.\n• Assets/ 아래만 지원.",
                    "• Перетащите ресурсы или папки из Project в окно.\n• ПКМ: Assets → 添加到常用 / 添加到常用（指定分类）.\n• Дубликаты по GUID игнорируются.\n• Только объекты в Assets/."
                },
                ["help_category_title"] = new[]
                {
                    "分类管理", "分類管理", "Categories", "グループ", "분류", "Категории"
                },
                ["help_category_body"] = new[]
                {
                    "• 工具栏「新建分类」创建分类。\n• 分类标题右键：重命名、删除（可将项移到其他分类，或连同项一并删除）。\n• 项右键「移到分类」可更换所属分类。\n• 支持在同分类内拖拽排序，或拖到其他分类。",
                    "• 工具列「新建分類」建立分類。\n• 分類標題右鍵：重新命名、刪除（可將項目移到其他分類，或連同項目刪除）。\n• 項目右鍵「移到分類」可更換所屬分類。\n• 支援同分類內拖曳排序，或拖到其他分類。",
                    "• Toolbar 「New Category」 creates a group.\n• Right-click a category header: rename or delete (move items elsewhere, or delete with items).\n• Item context menu 「Move to」 changes category.\n• Drag to reorder within a category or move between categories.",
                    "• ツールバー「新規グループ」で作成。\n• 見出しを右クリック：名前変更／削除（移動または項目ごと削除）。\n• 項目の「グループへ移動」で変更。\n• ドラッグで並べ替え・別グループへ移動可能。",
                    "• 툴바 「새 분류」로 생성.\n• 분류 제목 우클릭: 이름 바꾸기/삭제(이동 또는 항목 포함 삭제).\n• 항목 「분류로 이동」으로 변경.\n• 드래그로 정렬 또는 다른 분류로 이동.",
                    "• Кнопка 「Новая категория」 создаёт группу.\n• ПКМ по заголовку: переименовать/удалить (перенести или удалить с элементами).\n• ПКМ по элементу → «Переместить».\n• Перетаскивание меняет порядок или категорию."
                },
                ["help_fold_title"] = new[]
                {
                    "折叠分类", "摺疊分類", "Fold Categories", "折りたたみ", "분류 접기", "Сворачивание"
                },
                ["help_fold_body"] = new[]
                {
                    "点击分类前的折叠箭头可展开/收起。折叠状态会保存，重启 Unity 后仍有效。搜索时会临时展开有匹配项的分类。",
                    "點擊分類前的摺疊箭頭可展開/收起。摺疊狀態會保存，重啟 Unity 後仍有效。搜尋時會暫時展開有符合項目的分類。",
                    "Use the foldout arrow to expand/collapse. Collapse state is saved across sessions. While searching, categories with matches are temporarily expanded.",
                    "折りたたみ矢印で開閉。状態は保存され再起動後も維持。検索中は一致するグループが一時的に展開されます。",
                    "접기 화살표로 펼치기/접기. 상태는 저장되며 재시작 후에도 유지됩니다. 검색 중에는 일치 항목이 있는 분류가 일시적으로 펼쳐집니다.",
                    "Стрелка сворачивает/разворачивает группу. Состояние сохраняется. При поиске категории с совпадениями временно раскрываются."
                },
                ["help_search_title"] = new[]
                {
                    "搜索", "搜尋", "Search", "検索", "검색", "Поиск"
                },
                ["help_search_body"] = new[]
                {
                    "在搜索栏输入关键字，按文件名或路径过滤（不区分大小写）。无匹配时显示提示。点「×」或在搜索框聚焦时按 Esc 可清空。",
                    "在搜尋欄輸入關鍵字，依檔名或路徑過濾（不分大小寫）。無符合時顯示提示。點「×」或在搜尋框聚焦時按 Esc 可清空。",
                    "Type in the search field to filter by file name or path (case-insensitive). Clear with 「×」 or Esc while the field is focused.",
                    "検索欄にキーワードを入力し、名前／パスで絞り込み（大文字小文字無視）。「×」またはフォーカス時の Esc でクリア。",
                    "검색창에 키워드를 입력해 이름/경로로 필터(대소문자 무시). 「×」 또는 포커스 중 Esc로 지웁니다.",
                    "Введите запрос для фильтра по имени или пути (без учёта регистра). Очистка: 「×」 или Esc при фокусе."
                },
                ["help_item_title"] = new[]
                {
                    "定位、打开与行内按钮", "定位、開啟與列內按鈕", "Locate, Open & Row Buttons", "フォーカス・開く・行ボタン", "찾기·열기·행 버튼", "Найти, открыть и кнопки"
                },
                ["help_item_body"] = new[]
                {
                    "• 单击：在 Project 中选中并 Ping。\n• 双击：打开资源（文件夹仍为定位）。\n• ↗：用系统资源管理器打开；文件打开其所在目录，文件夹打开自身。\n• ×：从常用列表移除。\n• 右键：定位、打开、资源管理器、移到分类、移除。\n• Delete / Backspace：删除当前选中项。",
                    "• 單擊：在 Project 中選取並 Ping。\n• 雙擊：開啟資源（資料夾仍為定位）。\n• ↗：用系統檔案總管開啟；檔案開啟其所在目錄，資料夾開啟自身。\n• ×：從常用清單移除。\n• 右鍵：定位、開啟、檔案總管、移到分類、移除。\n• Delete / Backspace：刪除目前選取項。",
                    "• Click: select and ping in Project.\n• Double-click: open asset (folders still ping).\n• ↗: open in system file manager; files open their parent folder, folders open themselves.\n• ×: remove from favorites.\n• Context menu: ping, open, explorer, move, remove.\n• Delete / Backspace: remove the selected item.",
                    "• クリック：Project で選択＆Ping。\n• ダブルクリック：開く（フォルダはフォーカス）。\n• ↗：エクスプローラーで開く。ファイルは親フォルダ、フォルダは自身。\n• ×：お気に入りから削除。\n• 右クリック：フォーカス／開く／Explorer／移動／削除。\n• Delete / Backspace：選択項目を削除。",
                    "• 클릭: Project에서 선택 및 Ping.\n• 더블클릭: 에셋 열기(폴더는 찾기).\n• ↗: 시스템 탐색기에서 열기. 파일은 상위 폴더, 폴더는 자신.\n• ×: 즐겨찾기에서 제거.\n• 우클릭: 찾기/열기/탐색기/이동/제거.\n• Delete / Backspace: 선택 항목 삭제.",
                    "• Клик: выделить и ping в Project.\n• Двойной клик: открыть (папки — только ping).\n• ↗: открыть в проводнике; файл — родительская папка, папка — сама.\n• ×: убрать из избранного.\n• ПКМ: найти, открыть, проводник, переместить, удалить.\n• Delete / Backspace: удалить выбранный элемент."
                },
                ["help_language_title"] = new[]
                {
                    "语言", "語言", "Language", "言語", "언어", "Язык"
                },
                ["help_language_body"] = new[]
                {
                    "工具栏右侧 Popup 可切换：简体中文、繁体中文、English、日本語、한국어、Русский。设置保存在 EditorPrefs，重启后仍然有效。说明窗口也会跟随当前语言。",
                    "工具列右側 Popup 可切換：簡體中文、繁體中文、English、日本語、한국어、Русский。設定保存在 EditorPrefs，重啟後仍然有效。說明視窗也會跟隨目前語言。",
                    "Use the language popup on the toolbar: Simplified Chinese, Traditional Chinese, English, Japanese, Korean, Russian. Saved in EditorPrefs. This help window follows the selected language.",
                    "ツールバーの言語 Popup：簡体字中国語／繁体字／English／日本語／한국어／Русский。EditorPrefs に保存。ヘルプも言語に追従します。",
                    "툴바 언어 Popup: 간체/번체/English/日本語/한국어/Русский. EditorPrefs에 저장되며, 이 설명 창도 언어를 따릅니다.",
                    "Popup языка на панели: упрощённый/традиционный китайский, English, 日本語, 한국어, Русский. Сохраняется в EditorPrefs. Справка следует выбранному языку."
                },
                ["help_cleanup_title"] = new[]
                {
                    "清理无效项", "清理無效項", "Clean Missing", "欠落の削除", "유효하지 않음 정리", "Очистка"
                },
                ["help_cleanup_body"] = new[]
                {
                    "资源被删除后，列表中会显示为 Missing。点击工具栏「清理无效项」可一次性移除这些条目。",
                    "資源被刪除後，清單中會顯示為 Missing。點擊工具列「清理無效項」可一次移除這些項目。",
                    "Deleted assets appear as Missing. Use toolbar 「Clean Missing」 to remove those entries in one step.",
                    "削除されたアセットは Missing と表示されます。ツールバー「欠落を削除」で一括除去できます。",
                    "삭제된 에셋은 Missing으로 표시됩니다. 툴바 「유효하지 않음 정리」로 한 번에 제거하세요.",
                    "Удалённые ресурсы отображаются как Missing. Кнопка 「Очистить」 удаляет такие записи сразу."
                },
                ["help_data_title"] = new[]
                {
                    "数据存储", "資料儲存", "Data Storage", "データの保存", "데이터 저장", "Хранение данных"
                },
                ["help_data_body"] = new[]
                {
                    "常用数据保存在项目根目录 UserSettings/FavoritesData.json（按用户、按项目，通常不进版本库）。引用使用资产 GUID，移动或改名后仍可定位。",
                    "常用資料保存在專案根目錄 UserSettings/FavoritesData.json（依使用者、依專案，通常不進版本庫）。引用使用資產 GUID，移動或改名後仍可定位。",
                    "Data is stored at UserSettings/FavoritesData.json in the project root (per-user, per-project; usually not versioned). References use asset GUIDs, so moves/renames keep working.",
                    "データはプロジェクト直下の UserSettings/FavoritesData.json（ユーザー／プロジェクト単位、通常は VCS 対象外）。GUID 参照のため移動・改名後も有効です。",
                    "데이터는 프로젝트 루트 UserSettings/FavoritesData.json에 저장됩니다(사용자·프로젝트별, 보통 VCS 제외). GUID 참조라 이동/이름 변경 후에도 유효합니다.",
                    "Данные: UserSettings/FavoritesData.json в корне проекта (на пользователя и проект, обычно не в VCS). Ссылки по GUID — переименование/перемещение сохраняются."
                },
                ["help_scope_title"] = new[]
                {
                    "功能范围", "功能範圍", "Scope", "対応範囲", "지원 범위", "Ограничения"
                },
                ["help_scope_body"] = new[]
                {
                    "本插件为 Editor-only，不影响运行时与构建。\n当前不支持：场景 Hierarchy 物体、组件、菜单项、外部文件/URL。",
                    "本外掛為 Editor-only，不影響執行時與建置。\n目前不支援：場景 Hierarchy 物件、元件、選單項、外部檔案/URL。",
                    "Editor-only: no runtime or build impact.\nNot supported: Hierarchy objects, components, menu items, external files/URLs.",
                    "Editor 専用でランタイム／ビルドに影響しません。\n非対応：Hierarchy オブジェクト、コンポーネント、メニュー項目、外部ファイル／URL。",
                    "Editor 전용이며 런타임/빌드에 영향 없습니다.\n미지원: Hierarchy 오브젝트, 컴포넌트, 메뉴 항목, 외부 파일/URL.",
                    "Только Editor: не влияет на runtime и сборку.\nНе поддерживается: объекты Hierarchy, компоненты, пункты меню, внешние файлы/URL."
                }
            };
    }
}
