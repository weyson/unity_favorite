using System.Linq;
using UnityEditor;
using UnityEngine;

namespace UnityFavorite.Favorites
{
    public static class FavoritesMenus
    {
        const string AddMenuPath = "Assets/添加到常用";
        const string AddToCategoryMenuPath = "Assets/添加到常用（指定分类）";

        [MenuItem(AddMenuPath, false, 2000)]
        static void AddSelectedToFavorites()
        {
            var paths = GetSelectedAssetPaths();
            if (paths.Length == 0)
                return;

            var count = FavoritesService.Instance.TryAddAssets(paths);
            if (count > 0)
                FavoritesWindow.Open();
        }

        [MenuItem(AddMenuPath, true)]
        static bool ValidateAddSelectedToFavorites() => GetSelectedAssetPaths().Length > 0;

        [MenuItem(AddToCategoryMenuPath, false, 2001)]
        static void AddSelectedToFavoritesWithCategory()
        {
            var paths = GetSelectedAssetPaths();
            if (paths.Length == 0)
                return;

            var categories = FavoritesService.Instance.Data.categories;
            if (categories.Count == 0)
            {
                FavoritesService.Instance.TryAddAssets(paths);
                FavoritesWindow.Open();
                return;
            }

            var menu = new GenericMenu();
            foreach (var category in categories)
            {
                var cat = category;
                menu.AddItem(new GUIContent(cat.name), false, () =>
                {
                    FavoritesService.Instance.TryAddAssets(paths, cat.id);
                    FavoritesWindow.Open();
                });
            }

            menu.ShowAsContext();
        }

        [MenuItem(AddToCategoryMenuPath, true)]
        static bool ValidateAddSelectedToFavoritesWithCategory() => GetSelectedAssetPaths().Length > 0;

        static string[] GetSelectedAssetPaths()
        {
            return Selection.assetGUIDs
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(p => !string.IsNullOrEmpty(p))
                .Select(p => p.Replace('\\', '/'))
                .Where(p => p.StartsWith("Assets/"))
                .Distinct()
                .ToArray();
        }
    }
}
