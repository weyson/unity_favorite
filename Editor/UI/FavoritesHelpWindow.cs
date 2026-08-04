using UnityEditor;
using UnityEngine;

namespace UnityFavorite.Favorites
{
    public sealed class FavoritesHelpWindow : EditorWindow
    {
        static readonly string[] SectionKeys =
        {
            "help_open",
            "help_add",
            "help_category",
            "help_fold",
            "help_search",
            "help_item",
            "help_language",
            "help_cleanup",
            "help_data",
            "help_scope"
        };

        Vector2 _scroll;
        GUIStyle _bodyStyle;

        [MenuItem("Window/常用资源/使用说明", false, 1)]
        public static void Open()
        {
            var window = GetWindow<FavoritesHelpWindow>(true);
            window.minSize = new Vector2(420, 360);
            window.ApplyTitle();
            window.Show();
        }

        void OnEnable()
        {
            Loc.EnsureInit();
            Loc.LanguageChanged += OnLanguageChanged;
            ApplyTitle();
        }

        void OnDisable()
        {
            Loc.LanguageChanged -= OnLanguageChanged;
        }

        void OnLanguageChanged()
        {
            ApplyTitle();
            Repaint();
        }

        void ApplyTitle()
        {
            titleContent = new GUIContent(Loc.T("help_title"));
        }

        void OnGUI()
        {
            EnsureStyles();

            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUILayout.Label(Loc.T("help_title"), EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();

                if (GUILayout.Button(Loc.T("open_readme"), EditorStyles.toolbarButton))
                    ReadmeLinks.TryOpen(Loc.Language);

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

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField(Loc.T("help_intro"), _bodyStyle);
            EditorGUILayout.Space(6);
            EditorGUILayout.HelpBox(Loc.T("help_readme_hint"), MessageType.Info);
            EditorGUILayout.Space(10);

            foreach (var key in SectionKeys)
            {
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.LabelField(Loc.T(key + "_title"), EditorStyles.boldLabel);
                    EditorGUILayout.Space(2);
                    EditorGUILayout.LabelField(Loc.T(key + "_body"), _bodyStyle);
                }

                EditorGUILayout.Space(4);
            }

            EditorGUILayout.EndScrollView();
        }

        void EnsureStyles()
        {
            if (_bodyStyle != null)
                return;

            _bodyStyle = new GUIStyle(EditorStyles.wordWrappedLabel)
            {
                richText = false,
                padding = new RectOffset(2, 2, 2, 2)
            };
        }
    }
}
