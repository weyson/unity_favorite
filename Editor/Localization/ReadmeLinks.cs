using System.IO;
using UnityEditor;
using UnityEngine;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace UnityFavorite.Favorites
{
    public static class ReadmeLinks
    {
        public static string GetFileName(FavoriteLanguage language)
        {
            switch (language)
            {
                case FavoriteLanguage.ZhTw:
                    return "README.zh-TW.md";
                case FavoriteLanguage.En:
                    return "README.en.md";
                case FavoriteLanguage.Ja:
                    return "README.ja.md";
                case FavoriteLanguage.Ko:
                    return "README.ko.md";
                case FavoriteLanguage.Ru:
                    return "README.ru.md";
                case FavoriteLanguage.ZhCn:
                default:
                    return "README.md";
            }
        }

        public static string GetAbsolutePath(FavoriteLanguage language)
        {
            var fileName = GetFileName(language);
            var packageInfo = PackageInfo.FindForAssembly(typeof(ReadmeLinks).Assembly);
            if (packageInfo != null && !string.IsNullOrEmpty(packageInfo.resolvedPath))
            {
                var packagePath = Path.Combine(packageInfo.resolvedPath, fileName);
                if (File.Exists(packagePath))
                    return packagePath;

                // Fall back to default README in the same package
                var defaultPath = Path.Combine(packageInfo.resolvedPath, "README.md");
                if (File.Exists(defaultPath))
                    return defaultPath;
            }

            // Embedded under Assets/ fallback
            var dataPathParent = Path.GetDirectoryName(Application.dataPath);
            var candidates = new[]
            {
                Path.Combine(dataPathParent ?? string.Empty, "Packages", "com.unityfavorite.favorites", fileName),
                Path.Combine(Application.dataPath, "unity_favorite", fileName)
            };

            foreach (var candidate in candidates)
            {
                if (File.Exists(candidate))
                    return candidate;
            }

            return null;
        }

        public static bool TryOpen(FavoriteLanguage language)
        {
            var path = GetAbsolutePath(language);
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                Debug.LogWarning(Loc.Tf("log_readme_missing", Loc.T("log_prefix"), GetFileName(language)));
                return false;
            }

            EditorUtility.OpenWithDefaultApp(path);
            return true;
        }
    }
}
