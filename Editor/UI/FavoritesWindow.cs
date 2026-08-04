using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace UnityFavorite.Favorites
{
    public sealed class FavoritesWindow : EditorWindow
    {
        const float IconSize = 18f;
        const float RowHeight = 22f;
        const float ActionButtonWidth = 20f;
        const float ActionButtonsTotalWidth = ActionButtonWidth * 2f + 4f;

        Vector2 _scroll;
        string _searchText = string.Empty;
        string _renameCategoryId;
        string _renameBuffer;
        string _dragItemId;
        string _selectedItemId;

        bool IsSearching => !string.IsNullOrWhiteSpace(_searchText);

        FavoritesService Service => FavoritesService.Instance;

        [MenuItem("Window/常用资源/常用资源", false, 0)]
        public static void Open()
        {
            var window = GetWindow<FavoritesWindow>();
            window.ApplyWindowTitle();
            window.minSize = new Vector2(240, 180);
            window.Show();
        }

        void OnEnable()
        {
            Loc.EnsureInit();
            Loc.LanguageChanged += OnLanguageChanged;
            Service.Changed += Repaint;
            wantsMouseMove = true;
            ApplyWindowTitle();
        }

        void OnDisable()
        {
            Loc.LanguageChanged -= OnLanguageChanged;
            Service.Changed -= Repaint;
        }

        void OnLanguageChanged()
        {
            ApplyWindowTitle();
            Repaint();
        }

        void ApplyWindowTitle()
        {
            titleContent = new GUIContent(Loc.T("window_title"));
        }

        void OnGUI()
        {
            DrawToolbar();
            DrawSearchBar();
            DrawBody();
            HandleKeyboard();
        }

        void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                if (GUILayout.Button(Loc.T("new_category"), EditorStyles.toolbarButton))
                {
                    var category = Service.CreateCategory(Loc.T("new_category_name"));
                    _renameCategoryId = category.id;
                    _renameBuffer = category.name;
                }

                if (GUILayout.Button(Loc.T("cleanup"), EditorStyles.toolbarButton))
                {
                    var removed = Service.CleanupMissing();
                    EditorUtility.DisplayDialog(
                        Loc.T("window_title"),
                        removed > 0 ? Loc.Tf("cleaned_count", removed) : Loc.T("cleaned_none"),
                        Loc.T("ok"));
                }

                GUILayout.FlexibleSpace();

                if (GUILayout.Button(new GUIContent("?", Loc.T("help_tooltip")), EditorStyles.toolbarButton, GUILayout.Width(22)))
                    FavoritesHelpWindow.Open();

                GUILayout.Label(Loc.T("language"), EditorStyles.toolbarButton);
                EditorGUI.BeginChangeCheck();
                var languageIndex = (int)Loc.Language;
                var newIndex = EditorGUILayout.Popup(
                    languageIndex,
                    Loc.DisplayNames,
                    EditorStyles.toolbarPopup,
                    GUILayout.MinWidth(88),
                    GUILayout.MaxWidth(110));
                if (EditorGUI.EndChangeCheck() && newIndex != languageIndex)
                    Loc.SetLanguage((FavoriteLanguage)newIndex);
            }
        }

        void DrawSearchBar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUILayout.Label(Loc.T("search"), GUILayout.Width(48));
                GUI.SetNextControlName("FavoritesSearch");
                _searchText = GUILayout.TextField(_searchText ?? string.Empty, EditorStyles.toolbarSearchField);

                if (!string.IsNullOrEmpty(_searchText) &&
                    GUILayout.Button("×", EditorStyles.toolbarButton, GUILayout.Width(20)))
                {
                    _searchText = string.Empty;
                    GUI.FocusControl(null);
                }
            }
        }

        void DrawBody()
        {
            var data = Service.Data;
            if (data.categories.Count == 0 && data.items.Count == 0)
            {
                DrawEmptyState();
                return;
            }

            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            var anyVisible = false;
            foreach (var category in data.categories.ToList())
            {
                if (DrawCategory(category))
                    anyVisible = true;
            }

            if (IsSearching && !anyVisible)
            {
                EditorGUILayout.Space(8);
                EditorGUILayout.HelpBox(Loc.Tf("no_search_results", _searchText.Trim()), MessageType.Info);
            }
            else if (!IsSearching && data.items.Count == 0)
            {
                EditorGUILayout.Space(8);
                var hintRect = GUILayoutUtility.GetRect(0, 72, GUILayout.ExpandWidth(true));
                EditorGUI.HelpBox(hintRect, Loc.T("empty_hint"), MessageType.Info);
                AcceptAssetDropOnRect(hintRect, data.lastCategoryId, -1);
            }

            if (!IsSearching)
            {
                var trailing = GUILayoutUtility.GetRect(0, 24, GUILayout.ExpandWidth(true));
                AcceptAssetDropOnRect(trailing, data.lastCategoryId, -1);
            }

            EditorGUILayout.EndScrollView();
        }

        void DrawEmptyState()
        {
            var rect = GUILayoutUtility.GetRect(
                GUIContent.none,
                GUIStyle.none,
                GUILayout.ExpandWidth(true),
                GUILayout.ExpandHeight(true));

            GUI.Label(
                rect,
                Loc.T("empty_hint_short"),
                new GUIStyle(EditorStyles.centeredGreyMiniLabel) { wordWrap = true, alignment = TextAnchor.MiddleCenter });

            AcceptAssetDropOnRect(rect, null, -1);
        }

        bool DrawCategory(Category category)
        {
            var visibleItems = category.itemIds
                .Select(Service.GetItem)
                .Where(item => item != null && MatchesSearch(item))
                .ToList();

            if (IsSearching && visibleItems.Count == 0)
                return false;

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                DrawCategoryHeader(category, visibleItems.Count);

                var collapsed = !IsSearching && category.collapsed;
                if (collapsed)
                    return true;

                if (visibleItems.Count == 0)
                {
                    var rect = GUILayoutUtility.GetRect(0, 28, GUILayout.ExpandWidth(true));
                    EditorGUI.LabelField(rect, Loc.T("empty_category"), EditorStyles.centeredGreyMiniLabel);
                    AcceptDropOnRect(rect, category.id, category.itemIds.Count);
                    return true;
                }

                for (var i = 0; i < visibleItems.Count; i++)
                {
                    var item = visibleItems[i];
                    var sourceIndex = category.itemIds.IndexOf(item.id);
                    DrawItemRow(category, item, sourceIndex >= 0 ? sourceIndex : i);
                }
            }

            return true;
        }

        void DrawCategoryHeader(Category category, int visibleCount)
        {
            var headerRect = EditorGUILayout.GetControlRect(false, 20);
            var foldRect = new Rect(headerRect.x, headerRect.y, 16, headerRect.height);
            var labelRect = new Rect(headerRect.x + 16, headerRect.y, headerRect.width - 16, headerRect.height);

            var expanded = IsSearching || !category.collapsed;
            EditorGUI.BeginDisabledGroup(IsSearching);
            var newExpanded = EditorGUI.Foldout(foldRect, expanded, GUIContent.none, true);
            EditorGUI.EndDisabledGroup();
            if (!IsSearching && newExpanded != expanded)
                Service.SetCategoryCollapsed(category.id, !newExpanded);

            if (_renameCategoryId == category.id)
            {
                GUI.SetNextControlName("RenameCategory");
                _renameBuffer = EditorGUI.TextField(labelRect, _renameBuffer);
                if (Event.current.type == EventType.Layout)
                    EditorGUI.FocusTextInControl("RenameCategory");

                if (Event.current.type == EventType.KeyDown)
                {
                    if (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter)
                    {
                        CommitRename(category.id);
                        Event.current.Use();
                    }
                    else if (Event.current.keyCode == KeyCode.Escape)
                    {
                        _renameCategoryId = null;
                        Event.current.Use();
                    }
                }

                if (Event.current.type == EventType.MouseDown && !labelRect.Contains(Event.current.mousePosition))
                    CommitRename(category.id);
            }
            else
            {
                var countLabel = IsSearching
                    ? $"{category.name} ({visibleCount}/{category.itemIds.Count})"
                    : $"{category.name} ({visibleCount})";
                EditorGUI.LabelField(labelRect, countLabel, EditorStyles.boldLabel);
            }

            if (Event.current.type == EventType.ContextClick && headerRect.Contains(Event.current.mousePosition))
            {
                ShowCategoryContextMenu(category);
                Event.current.Use();
            }

            if (!IsSearching)
                AcceptDropOnRect(headerRect, category.id, 0);
        }

        void DrawItemRow(Category category, FavoriteItem item, int index)
        {
            var rowRect = GUILayoutUtility.GetRect(0, RowHeight, GUILayout.ExpandWidth(true));
            var isMissing = !FavoritesService.TryResolve(item.assetGuid, out var path, out var asset);
            var name = isMissing
                ? "(Missing)"
                : Path.GetFileNameWithoutExtension(path);
            if (!isMissing && AssetDatabase.IsValidFolder(path))
                name = Path.GetFileName(path);

            if (_selectedItemId == item.id)
                EditorGUI.DrawRect(rowRect, new Color(0.24f, 0.48f, 0.90f, 0.28f));
            else if (rowRect.Contains(Event.current.mousePosition))
                EditorGUI.DrawRect(rowRect, new Color(0.24f, 0.48f, 0.90f, 0.18f));

            var deleteRect = new Rect(
                rowRect.xMax - ActionButtonWidth - 2f,
                rowRect.y + 1f,
                ActionButtonWidth,
                RowHeight - 2f);
            var revealRect = new Rect(
                deleteRect.x - ActionButtonWidth - 2f,
                rowRect.y + 1f,
                ActionButtonWidth,
                RowHeight - 2f);
            var contentRect = new Rect(
                rowRect.x,
                rowRect.y,
                rowRect.width - ActionButtonsTotalWidth - 4f,
                rowRect.height);

            var iconRect = new Rect(contentRect.x + 4, contentRect.y + (RowHeight - IconSize) * 0.5f, IconSize, IconSize);
            var labelRect = new Rect(iconRect.xMax + 6, contentRect.y, contentRect.xMax - iconRect.xMax - 8, RowHeight);

            var icon = isMissing
                ? EditorGUIUtility.IconContent("console.erroricon.sml").image
                : AssetDatabase.GetCachedIcon(path);
            if (icon != null)
                GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit);

            var style = isMissing ? EditorStyles.helpBox : EditorStyles.label;
            var prevColor = GUI.contentColor;
            if (isMissing)
                GUI.contentColor = new Color(1f, 0.55f, 0.55f);
            GUI.Label(labelRect, new GUIContent(name, isMissing ? item.assetGuid : path), style);
            GUI.contentColor = prevColor;

            EditorGUI.BeginDisabledGroup(isMissing);
            if (GUI.Button(revealRect, new GUIContent("↗", Loc.T("open_in_explorer")), EditorStyles.miniButton))
            {
                FavoritesService.RevealInExplorer(item.assetGuid);
                GUIUtility.ExitGUI();
            }
            EditorGUI.EndDisabledGroup();

            if (GUI.Button(deleteRect, new GUIContent("×", Loc.T("remove_tooltip")), EditorStyles.miniButton))
            {
                if (_selectedItemId == item.id)
                    _selectedItemId = null;
                Service.RemoveItem(item.id);
                GUIUtility.ExitGUI();
            }

            HandleItemEvents(contentRect, item, isMissing, asset);
            AcceptDropOnRect(contentRect, category.id, index);
        }

        void HandleItemEvents(
            Rect hitRect,
            FavoriteItem item,
            bool isMissing,
            Object asset)
        {
            var e = Event.current;

            if (e.type == EventType.MouseDown && e.button == 0 && hitRect.Contains(e.mousePosition))
            {
                _selectedItemId = item.id;
                if (!isMissing)
                {
                    if (e.clickCount >= 2)
                        FavoritesService.OpenAsset(item.assetGuid);
                    else
                        FavoritesService.PingAsset(item.assetGuid);
                }

                _dragItemId = item.id;
                DragAndDrop.PrepareStartDrag();
                DragAndDrop.SetGenericData("FavoritesItemId", item.id);
                if (asset != null)
                {
                    DragAndDrop.objectReferences = new[] { asset };
                    DragAndDrop.paths = new[] { AssetDatabase.GetAssetPath(asset) };
                }

                e.Use();
            }

            if (e.type == EventType.MouseDrag && _dragItemId == item.id)
            {
                DragAndDrop.StartDrag("FavoriteItem");
                e.Use();
            }

            if (e.type == EventType.MouseUp)
                _dragItemId = null;

            if (e.type == EventType.ContextClick && hitRect.Contains(e.mousePosition))
            {
                _selectedItemId = item.id;
                ShowItemContextMenu(item, isMissing);
                e.Use();
            }
        }

        void AcceptDropOnRect(Rect rect, string categoryId, int insertIndex)
        {
            AcceptAssetDropOnRect(rect, categoryId, insertIndex);
        }

        void AcceptAssetDropOnRect(Rect rect, string categoryId, int insertIndex)
        {
            var e = Event.current;
            if (!rect.Contains(e.mousePosition))
                return;

            if (e.type != EventType.DragUpdated && e.type != EventType.DragPerform)
                return;

            var favoriteItemId = DragAndDrop.GetGenericData("FavoritesItemId") as string;
            var hasAssets = DragAndDrop.paths != null &&
                            DragAndDrop.paths.Any(p => p != null && p.Replace('\\', '/').StartsWith("Assets/"));

            if (string.IsNullOrEmpty(favoriteItemId) && !hasAssets)
                return;

            DragAndDrop.visualMode = string.IsNullOrEmpty(favoriteItemId)
                ? DragAndDropVisualMode.Copy
                : DragAndDropVisualMode.Move;

            if (e.type == EventType.DragUpdated)
            {
                e.Use();
                return;
            }

            DragAndDrop.AcceptDrag();

            if (!string.IsNullOrEmpty(favoriteItemId))
            {
                if (!string.IsNullOrEmpty(categoryId))
                {
                    var item = Service.GetItem(favoriteItemId);
                    if (item != null)
                    {
                        if (item.categoryId == categoryId)
                        {
                            var targetIndex = insertIndex < 0
                                ? Service.GetCategory(categoryId)?.itemIds.Count ?? 0
                                : insertIndex;
                            Service.ReorderItem(categoryId, favoriteItemId, targetIndex);
                        }
                        else
                        {
                            Service.MoveItem(favoriteItemId, categoryId, insertIndex);
                        }
                    }
                }
            }
            else if (hasAssets)
            {
                var paths = DragAndDrop.paths
                    .Where(p => !string.IsNullOrEmpty(p))
                    .Select(p => p.Replace('\\', '/'))
                    .Where(p => p.StartsWith("Assets/"))
                    .Distinct()
                    .ToArray();

                Service.TryAddAssets(paths, categoryId);
            }

            _dragItemId = null;
            e.Use();
        }

        void HandleKeyboard()
        {
            var e = Event.current;
            if (e.type != EventType.KeyDown)
                return;

            if (e.keyCode == KeyCode.Escape && IsSearching &&
                string.IsNullOrEmpty(_renameCategoryId) &&
                GUI.GetNameOfFocusedControl() == "FavoritesSearch")
            {
                _searchText = string.Empty;
                GUI.FocusControl(null);
                e.Use();
                Repaint();
                return;
            }

            if ((e.keyCode == KeyCode.Delete || e.keyCode == KeyCode.Backspace) &&
                !string.IsNullOrEmpty(_selectedItemId) &&
                string.IsNullOrEmpty(_renameCategoryId) &&
                GUI.GetNameOfFocusedControl() != "FavoritesSearch")
            {
                Service.RemoveItem(_selectedItemId);
                _selectedItemId = null;
                e.Use();
            }
        }

        bool MatchesSearch(FavoriteItem item)
        {
            if (!IsSearching)
                return true;

            var keyword = _searchText.Trim();
            if (FavoritesService.TryResolve(item.assetGuid, out var path, out _))
            {
                var fileName = Path.GetFileNameWithoutExtension(path);
                if (AssetDatabase.IsValidFolder(path))
                    fileName = Path.GetFileName(path);

                return ContainsIgnoreCase(fileName, keyword) || ContainsIgnoreCase(path, keyword);
            }

            return ContainsIgnoreCase("(Missing)", keyword) ||
                   ContainsIgnoreCase(item.assetGuid, keyword);
        }

        static bool ContainsIgnoreCase(string source, string value) =>
            !string.IsNullOrEmpty(source) &&
            source.IndexOf(value, System.StringComparison.OrdinalIgnoreCase) >= 0;

        void ShowItemContextMenu(FavoriteItem item, bool isMissing)
        {
            var menu = new GenericMenu();

            if (!isMissing)
            {
                menu.AddItem(new GUIContent(Loc.T("ping")), false, () => FavoritesService.PingAsset(item.assetGuid));
                menu.AddItem(new GUIContent(Loc.T("open")), false, () => FavoritesService.OpenAsset(item.assetGuid));
                menu.AddItem(new GUIContent(Loc.T("open_in_explorer")), false,
                    () => FavoritesService.RevealInExplorer(item.assetGuid));
                menu.AddSeparator("");
            }

            foreach (var category in Service.Data.categories)
            {
                var cat = category;
                var checkedState = item.categoryId == cat.id;
                menu.AddItem(
                    new GUIContent(Loc.Tf("move_to_category", cat.name)),
                    checkedState,
                    () =>
                    {
                        if (!checkedState)
                            Service.MoveItem(item.id, cat.id);
                    });
            }

            menu.AddSeparator("");
            menu.AddItem(new GUIContent(Loc.T("remove")), false, () => Service.RemoveItem(item.id));
            menu.ShowAsContext();
        }

        void ShowCategoryContextMenu(Category category)
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent(Loc.T("rename")), false, () =>
            {
                _renameCategoryId = category.id;
                _renameBuffer = category.name;
            });

            if (Service.Data.categories.Count > 1)
            {
                menu.AddItem(new GUIContent(Loc.T("delete_category_move")), false, () =>
                {
                    if (EditorUtility.DisplayDialog(
                            Loc.T("window_title"),
                            Loc.Tf("confirm_delete_category_move", category.name),
                            Loc.T("delete"),
                            Loc.T("cancel")))
                    {
                        Service.DeleteCategory(category.id, deleteItems: false);
                    }
                });

                menu.AddItem(new GUIContent(Loc.T("delete_category_items")), false, () =>
                {
                    if (EditorUtility.DisplayDialog(
                            Loc.T("window_title"),
                            Loc.Tf("confirm_delete_category_items", category.name),
                            Loc.T("delete"),
                            Loc.T("cancel")))
                    {
                        Service.DeleteCategory(category.id, deleteItems: true);
                    }
                });
            }
            else
            {
                menu.AddDisabledItem(new GUIContent(Loc.T("delete_category_disabled")));
            }

            menu.ShowAsContext();
        }

        void CommitRename(string categoryId)
        {
            if (!string.IsNullOrEmpty(_renameBuffer))
                Service.RenameCategory(categoryId, _renameBuffer);
            _renameCategoryId = null;
            _renameBuffer = null;
            GUI.FocusControl(null);
        }
    }
}
