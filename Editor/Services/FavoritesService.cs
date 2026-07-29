using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace UnityFavorite.Favorites
{
    public sealed class FavoritesService
    {
        static FavoritesService _instance;

        public static FavoritesService Instance =>
            _instance ??= new FavoritesService();

        public FavoritesData Data { get; private set; }

        public event Action Changed;

        FavoritesService()
        {
            Data = FavoritesStore.Load();
        }

        public void Reload()
        {
            Data = FavoritesStore.Load();
            RaiseChanged();
        }

        public void Persist()
        {
            FavoritesStore.Save(Data);
            RaiseChanged();
        }

        public Category GetCategory(string categoryId) =>
            Data.categories.FirstOrDefault(c => c.id == categoryId);

        public FavoriteItem GetItem(string itemId) =>
            Data.items.FirstOrDefault(i => i.id == itemId);

        public FavoriteItem FindByGuid(string assetGuid) =>
            Data.items.FirstOrDefault(i => i.assetGuid == assetGuid);

        public bool TryAddAsset(string assetPath, string categoryId = null, bool silentIfDuplicate = false)
        {
            if (!IsSupportedAssetPath(assetPath, out var guid))
                return false;

            if (FindByGuid(guid) != null)
            {
                if (!silentIfDuplicate)
                    Debug.Log($"[常用资源] 已在常用列表中: {assetPath}");
                return false;
            }

            var category = ResolveCategory(categoryId);
            var item = new FavoriteItem
            {
                id = Guid.NewGuid().ToString("N"),
                assetGuid = guid,
                categoryId = category.id
            };

            Data.items.Add(item);
            category.itemIds.Add(item.id);
            Data.lastCategoryId = category.id;
            Persist();
            return true;
        }

        public int TryAddAssets(IEnumerable<string> assetPaths, string categoryId = null)
        {
            var count = 0;
            var category = ResolveCategory(categoryId);
            var dirty = false;

            foreach (var path in assetPaths)
            {
                if (!IsSupportedAssetPath(path, out var guid))
                    continue;

                if (FindByGuid(guid) != null)
                    continue;

                var item = new FavoriteItem
                {
                    id = Guid.NewGuid().ToString("N"),
                    assetGuid = guid,
                    categoryId = category.id
                };

                Data.items.Add(item);
                category.itemIds.Add(item.id);
                count++;
                dirty = true;
            }

            if (dirty)
            {
                Data.lastCategoryId = category.id;
                Persist();
            }
            else if (count == 0)
            {
                Debug.Log("[常用资源] 没有可添加的资源（可能已存在或不在 Assets 下）");
            }

            return count;
        }

        public bool RemoveItem(string itemId)
        {
            var item = GetItem(itemId);
            if (item == null)
                return false;

            Data.items.Remove(item);
            foreach (var category in Data.categories)
                category.itemIds.Remove(itemId);

            Persist();
            return true;
        }

        public bool MoveItem(string itemId, string targetCategoryId, int insertIndex = -1)
        {
            var item = GetItem(itemId);
            var target = GetCategory(targetCategoryId);
            if (item == null || target == null)
                return false;

            foreach (var category in Data.categories)
                category.itemIds.Remove(itemId);

            item.categoryId = target.id;
            if (insertIndex < 0 || insertIndex > target.itemIds.Count)
                target.itemIds.Add(itemId);
            else
                target.itemIds.Insert(insertIndex, itemId);

            Data.lastCategoryId = target.id;
            Persist();
            return true;
        }

        public bool ReorderItem(string categoryId, string itemId, int newIndex)
        {
            var category = GetCategory(categoryId);
            if (category == null)
                return false;

            var oldIndex = category.itemIds.IndexOf(itemId);
            if (oldIndex < 0)
                return false;

            category.itemIds.RemoveAt(oldIndex);
            if (newIndex > oldIndex)
                newIndex--;
            newIndex = Mathf.Clamp(newIndex, 0, category.itemIds.Count);
            category.itemIds.Insert(newIndex, itemId);
            Persist();
            return true;
        }

        public Category CreateCategory(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                name = "新分类";

            var category = new Category
            {
                id = Guid.NewGuid().ToString("N"),
                name = name.Trim(),
                collapsed = false,
                itemIds = new List<string>()
            };

            Data.categories.Add(category);
            Data.lastCategoryId = category.id;
            Persist();
            return category;
        }

        public bool RenameCategory(string categoryId, string newName)
        {
            var category = GetCategory(categoryId);
            if (category == null || string.IsNullOrWhiteSpace(newName))
                return false;

            category.name = newName.Trim();
            Persist();
            return true;
        }

        public bool SetCategoryCollapsed(string categoryId, bool collapsed)
        {
            var category = GetCategory(categoryId);
            if (category == null)
                return false;

            if (category.collapsed == collapsed)
                return true;

            category.collapsed = collapsed;
            Persist();
            return true;
        }

        public bool DeleteCategory(string categoryId, bool deleteItems)
        {
            if (Data.categories.Count <= 1)
            {
                Debug.LogWarning("[常用资源] 至少保留一个分类");
                return false;
            }

            var category = GetCategory(categoryId);
            if (category == null)
                return false;

            var itemIds = category.itemIds.ToList();
            if (deleteItems)
            {
                foreach (var itemId in itemIds)
                {
                    var item = GetItem(itemId);
                    if (item != null)
                        Data.items.Remove(item);
                }
            }
            else
            {
                var fallback = Data.categories.First(c => c.id != categoryId);
                foreach (var itemId in itemIds)
                {
                    var item = GetItem(itemId);
                    if (item == null)
                        continue;
                    item.categoryId = fallback.id;
                    if (!fallback.itemIds.Contains(itemId))
                        fallback.itemIds.Add(itemId);
                }
            }

            Data.categories.Remove(category);
            if (Data.lastCategoryId == categoryId)
                Data.lastCategoryId = Data.categories[0].id;

            Persist();
            return true;
        }

        public int CleanupMissing()
        {
            var removed = 0;
            var missingIds = new List<string>();

            foreach (var item in Data.items)
            {
                var path = AssetDatabase.GUIDToAssetPath(item.assetGuid);
                if (string.IsNullOrEmpty(path) || !AssetExists(path))
                    missingIds.Add(item.id);
            }

            foreach (var id in missingIds)
            {
                if (RemoveItemWithoutPersist(id))
                    removed++;
            }

            if (removed > 0)
                Persist();

            return removed;
        }

        public static bool IsSupportedAssetPath(string assetPath, out string guid)
        {
            guid = null;
            if (string.IsNullOrEmpty(assetPath))
                return false;

            assetPath = assetPath.Replace('\\', '/');
            if (!assetPath.StartsWith("Assets/", StringComparison.Ordinal))
                return false;

            if (!AssetExists(assetPath))
                return false;

            guid = AssetDatabase.AssetPathToGUID(assetPath);
            return !string.IsNullOrEmpty(guid);
        }

        public static bool TryResolve(string assetGuid, out string path, out UnityEngine.Object asset)
        {
            path = AssetDatabase.GUIDToAssetPath(assetGuid);
            asset = null;

            if (string.IsNullOrEmpty(path) || !AssetExists(path))
                return false;

            asset = AssetDatabase.LoadMainAssetAtPath(path);
            return asset != null;
        }

        public static void PingAsset(string assetGuid)
        {
            if (!TryResolve(assetGuid, out _, out var asset))
            {
                Debug.LogWarning("[常用资源] 资源已丢失，请清理无效项");
                return;
            }

            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
        }

        public static void OpenAsset(string assetGuid)
        {
            if (!TryResolve(assetGuid, out var path, out var asset))
            {
                Debug.LogWarning("[常用资源] 资源已丢失，请清理无效项");
                return;
            }

            Selection.activeObject = asset;
            if (AssetDatabase.IsValidFolder(path))
            {
                EditorGUIUtility.PingObject(asset);
                return;
            }

            AssetDatabase.OpenAsset(asset);
        }

        static bool AssetExists(string assetPath)
        {
            if (AssetDatabase.IsValidFolder(assetPath))
                return true;

            return !string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(assetPath)) &&
                   AssetDatabase.LoadMainAssetAtPath(assetPath) != null;
        }

        Category ResolveCategory(string categoryId)
        {
            if (!string.IsNullOrEmpty(categoryId))
            {
                var found = GetCategory(categoryId);
                if (found != null)
                    return found;
            }

            var last = GetCategory(Data.lastCategoryId);
            return last ?? Data.categories[0];
        }

        bool RemoveItemWithoutPersist(string itemId)
        {
            var item = GetItem(itemId);
            if (item == null)
                return false;

            Data.items.Remove(item);
            foreach (var category in Data.categories)
                category.itemIds.Remove(itemId);
            return true;
        }

        void RaiseChanged() => Changed?.Invoke();
    }
}
