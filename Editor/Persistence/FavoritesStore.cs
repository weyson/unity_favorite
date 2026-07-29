using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace UnityFavorite.Favorites
{
    public static class FavoritesStore
    {
        const string FileName = "FavoritesData.json";
        const string BackupSuffix = ".bak";

        static string FilePath =>
            Path.Combine(GetUserSettingsDirectory(), FileName);

        static string BackupPath => FilePath + BackupSuffix;

        public static FavoritesData Load()
        {
            EnsureUserSettingsDirectory();

            if (!File.Exists(FilePath))
            {
                var created = FavoritesData.CreateDefault();
                Save(created);
                return created;
            }

            try
            {
                var json = File.ReadAllText(FilePath);
                if (string.IsNullOrWhiteSpace(json))
                    return RecoverCorrupt("empty file");

                var data = JsonUtility.FromJson<FavoritesData>(json);
                if (data == null || data.categories == null || data.items == null)
                    return RecoverCorrupt("invalid structure");

                if (data.categories.Count == 0)
                {
                    var fallback = FavoritesData.CreateDefault();
                    data.categories = fallback.categories;
                    data.lastCategoryId = fallback.lastCategoryId;
                }

                Sanitize(data);
                return data;
            }
            catch (Exception ex)
            {
                return RecoverCorrupt(ex.Message);
            }
        }

        public static void Save(FavoritesData data)
        {
            if (data == null)
                return;

            EnsureUserSettingsDirectory();
            Sanitize(data);

            var json = JsonUtility.ToJson(data, true);
            File.WriteAllText(FilePath, json);
        }

        static FavoritesData RecoverCorrupt(string reason)
        {
            Debug.LogWarning(Loc.Tf("log_corrupt", Loc.T("log_prefix"), reason));

            try
            {
                if (File.Exists(FilePath))
                    File.Copy(FilePath, BackupPath, true);
            }
            catch (Exception ex)
            {
                Debug.LogWarning(Loc.Tf("log_backup_failed", Loc.T("log_prefix"), ex.Message));
            }

            var data = FavoritesData.CreateDefault();
            Save(data);
            return data;
        }

        static void Sanitize(FavoritesData data)
        {
            data.version = Math.Max(1, data.version);
            data.categories ??= new List<Category>();
            data.items ??= new List<FavoriteItem>();

            foreach (var category in data.categories)
            {
                if (string.IsNullOrEmpty(category.id))
                    category.id = Guid.NewGuid().ToString("N");
                if (string.IsNullOrEmpty(category.name))
                    category.name = Loc.T("unnamed_category");
                category.itemIds ??= new List<string>();
            }

            foreach (var item in data.items)
            {
                if (string.IsNullOrEmpty(item.id))
                    item.id = Guid.NewGuid().ToString("N");
            }

            // Deduplicate items by id
            data.items = data.items
                .GroupBy(i => i.id)
                .Select(g => g.First())
                .ToList();

            var itemMap = data.items.ToDictionary(i => i.id, i => i);
            var knownIds = new HashSet<string>(itemMap.Keys);

            foreach (var category in data.categories)
            {
                category.itemIds = category.itemIds
                    .Where(id => knownIds.Contains(id))
                    .Distinct()
                    .ToList();

                foreach (var id in category.itemIds)
                    itemMap[id].categoryId = category.id;
            }

            var referenced = new HashSet<string>(data.categories.SelectMany(c => c.itemIds));
            foreach (var item in data.items)
            {
                if (referenced.Contains(item.id))
                    continue;

                var cat = data.categories.FirstOrDefault(c => c.id == item.categoryId) ?? data.categories[0];
                item.categoryId = cat.id;
                cat.itemIds.Add(item.id);
                referenced.Add(item.id);
            }

            if (string.IsNullOrEmpty(data.lastCategoryId) ||
                data.categories.Find(c => c.id == data.lastCategoryId) == null)
            {
                data.lastCategoryId = data.categories[0].id;
            }
        }

        static string GetUserSettingsDirectory()
        {
            // ProjectRoot/UserSettings — personal, not versioned with Assets
            var projectRoot = Path.GetDirectoryName(Application.dataPath);
            return Path.Combine(projectRoot ?? Application.dataPath, "UserSettings");
        }

        static void EnsureUserSettingsDirectory()
        {
            var dir = GetUserSettingsDirectory();
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
        }
    }
}
