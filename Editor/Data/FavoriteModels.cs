using System;
using System.Collections.Generic;

namespace UnityFavorite.Favorites
{
    [Serializable]
    public class FavoritesData
    {
        public int version = 1;
        public string lastCategoryId;
        public List<Category> categories = new List<Category>();
        public List<FavoriteItem> items = new List<FavoriteItem>();

        public static FavoritesData CreateDefault()
        {
            var data = new FavoritesData();
            var uncategorized = new Category
            {
                id = Guid.NewGuid().ToString("N"),
                name = "未分类",
                collapsed = false,
                itemIds = new List<string>()
            };
            data.categories.Add(uncategorized);
            data.lastCategoryId = uncategorized.id;
            return data;
        }
    }

    [Serializable]
    public class Category
    {
        public string id;
        public string name;
        public bool collapsed;
        public List<string> itemIds = new List<string>();
    }

    [Serializable]
    public class FavoriteItem
    {
        public string id;
        public string assetGuid;
        public string categoryId;
    }
}
