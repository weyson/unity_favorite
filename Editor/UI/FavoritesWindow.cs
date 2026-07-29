using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace UnityFavorite.Favorites
{
    public sealed class FavoritesWindow : EditorWindow
    {
        const string WindowTitle = "常用资源";
        const float IconSize = 18f;
        const float RowHeight = 22f;

        Vector2 _scroll;
        string _renameCategoryId;
        string _renameBuffer;
        string _dragItemId;
        string _selectedItemId;

        FavoritesService Service => FavoritesService.Instance;

        [MenuItem("Window/常用资源")]
        public static void Open()
        {
            var window = GetWindow<FavoritesWindow>();
            window.titleContent = new GUIContent(WindowTitle);
            window.minSize = new Vector2(220, 160);
            window.Show();
        }

        void OnEnable()
        {
            Service.Changed += Repaint;
            wantsMouseMove = true;
        }

        void OnDisable()
        {
            Service.Changed -= Repaint;
        }

        void OnGUI()
        {
            DrawToolbar();
            DrawBody();
            HandleKeyboard();
        }

        void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                if (GUILayout.Button("新建分类", EditorStyles.toolbarButton, GUILayout.Width(72)))
                {
                    var category = Service.CreateCategory("新分类");
                    _renameCategoryId = category.id;
                    _renameBuffer = category.name;
                }

                if (GUILayout.Button("清理无效项", EditorStyles.toolbarButton, GUILayout.Width(84)))
                {
                    var removed = Service.CleanupMissing();
                    EditorUtility.DisplayDialog(
                        WindowTitle,
                        removed > 0 ? $"已清理 {removed} 个无效项。" : "没有无效项。",
                        "确定");
                }

                GUILayout.FlexibleSpace();
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

            foreach (var category in data.categories.ToList())
                DrawCategory(category);

            var totalItems = data.items.Count;
            if (totalItems == 0)
            {
                EditorGUILayout.Space(8);
                var hintRect = GUILayoutUtility.GetRect(0, 64, GUILayout.ExpandWidth(true));
                EditorGUI.HelpBox(
                    hintRect,
                    "暂无常用项。\n可将 Project 中的资源拖到此处，或右键资源选择「添加到常用」。",
                    MessageType.Info);
                AcceptAssetDropOnRect(hintRect, data.lastCategoryId, -1);
            }

            // Trailing drop zone for adding into last-used category
            var trailing = GUILayoutUtility.GetRect(0, 24, GUILayout.ExpandWidth(true));
            AcceptAssetDropOnRect(trailing, data.lastCategoryId, -1);

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
                "暂无常用项。\n可将 Project 中的资源拖到此处，\n或右键资源选择「添加到常用」。",
                new GUIStyle(EditorStyles.centeredGreyMiniLabel) { wordWrap = true, alignment = TextAnchor.MiddleCenter });

            AcceptAssetDropOnRect(rect, null, -1);
        }

        void DrawCategory(Category category)
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                DrawCategoryHeader(category);

                if (category.collapsed)
                    return;

                if (category.itemIds.Count == 0)
                {
                    var rect = GUILayoutUtility.GetRect(0, 28, GUILayout.ExpandWidth(true));
                    EditorGUI.LabelField(rect, "（空）将资源拖到此处", EditorStyles.centeredGreyMiniLabel);
                    AcceptDropOnRect(rect, category.id, category.itemIds.Count);
                    return;
                }

                for (var i = 0; i < category.itemIds.Count; i++)
                {
                    var item = Service.GetItem(category.itemIds[i]);
                    if (item == null)
                        continue;
                    DrawItemRow(category, item, i);
                }
            }
        }

        void DrawCategoryHeader(Category category)
        {
            var headerRect = EditorGUILayout.GetControlRect(false, 20);
            var foldRect = new Rect(headerRect.x, headerRect.y, 16, headerRect.height);
            var labelRect = new Rect(headerRect.x + 16, headerRect.y, headerRect.width - 16, headerRect.height);

            var expanded = !category.collapsed;
            var newExpanded = EditorGUI.Foldout(foldRect, expanded, GUIContent.none, true);
            if (newExpanded != expanded)
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
                var count = category.itemIds.Count;
                EditorGUI.LabelField(labelRect, $"{category.name} ({count})", EditorStyles.boldLabel);
            }

            if (Event.current.type == EventType.ContextClick && headerRect.Contains(Event.current.mousePosition))
            {
                ShowCategoryContextMenu(category);
                Event.current.Use();
            }

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

            var iconRect = new Rect(rowRect.x + 4, rowRect.y + (RowHeight - IconSize) * 0.5f, IconSize, IconSize);
            var labelRect = new Rect(iconRect.xMax + 6, rowRect.y, rowRect.width - IconSize - 14, RowHeight);

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

            HandleItemEvents(rowRect, item, isMissing, asset);
            AcceptDropOnRect(rowRect, category.id, index);
        }

        void HandleItemEvents(
            Rect rowRect,
            FavoriteItem item,
            bool isMissing,
            Object asset)
        {
            var e = Event.current;

            if (e.type == EventType.MouseDown && e.button == 0 && rowRect.Contains(e.mousePosition))
            {
                _selectedItemId = item.id;
                if (!isMissing)
                    FavoritesService.PingAsset(item.assetGuid);

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

            if (e.type == EventType.ContextClick && rowRect.Contains(e.mousePosition))
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

            if ((e.keyCode == KeyCode.Delete || e.keyCode == KeyCode.Backspace) &&
                !string.IsNullOrEmpty(_selectedItemId) &&
                string.IsNullOrEmpty(_renameCategoryId))
            {
                Service.RemoveItem(_selectedItemId);
                _selectedItemId = null;
                e.Use();
            }
        }

        void ShowItemContextMenu(FavoriteItem item, bool isMissing)
        {
            var menu = new GenericMenu();

            if (!isMissing)
            {
                menu.AddItem(new GUIContent("定位资源"), false, () => FavoritesService.PingAsset(item.assetGuid));
                menu.AddItem(new GUIContent("在资源管理器中显示"), false, () =>
                {
                    if (FavoritesService.TryResolve(item.assetGuid, out var path, out _))
                        EditorUtility.RevealInFinder(path);
                });
                menu.AddSeparator("");
            }

            foreach (var category in Service.Data.categories)
            {
                var cat = category;
                var checkedState = item.categoryId == cat.id;
                menu.AddItem(
                    new GUIContent($"移到分类/{cat.name}"),
                    checkedState,
                    () =>
                    {
                        if (!checkedState)
                            Service.MoveItem(item.id, cat.id);
                    });
            }

            menu.AddSeparator("");
            menu.AddItem(new GUIContent("移除"), false, () => Service.RemoveItem(item.id));
            menu.ShowAsContext();
        }

        void ShowCategoryContextMenu(Category category)
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("重命名"), false, () =>
            {
                _renameCategoryId = category.id;
                _renameBuffer = category.name;
            });

            if (Service.Data.categories.Count > 1)
            {
                menu.AddItem(new GUIContent("删除分类（项移到其他分类）"), false, () =>
                {
                    if (EditorUtility.DisplayDialog(
                            WindowTitle,
                            $"删除分类「{category.name}」？其中的常用项将移到其他分类。",
                            "删除",
                            "取消"))
                    {
                        Service.DeleteCategory(category.id, deleteItems: false);
                    }
                });

                menu.AddItem(new GUIContent("删除分类及其中的项"), false, () =>
                {
                    if (EditorUtility.DisplayDialog(
                            WindowTitle,
                            $"删除分类「{category.name}」及其全部常用项？",
                            "删除",
                            "取消"))
                    {
                        Service.DeleteCategory(category.id, deleteItems: true);
                    }
                });
            }
            else
            {
                menu.AddDisabledItem(new GUIContent("删除分类（至少保留一个）"));
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
