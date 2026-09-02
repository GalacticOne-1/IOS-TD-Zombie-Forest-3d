using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using Galactic1.Core.Enums;
using Galactic1.Game.Meta.Items;

namespace Galactic1.Tools
{

    public class IngredientPickerWindow : EditorWindow
    {
        private static ItemManagerWindow manager;
        private IReadOnlyList<ItemConfig> items;
        private Action<ItemConfig> onItemSelected;
        
        private Vector2 scroll;

        public static void ShowWindow(
            ItemCategoryEditor category,
            IReadOnlyList<ItemConfig> items,
            Action<ItemConfig> callback)
        {
            var window = CreateInstance<IngredientPickerWindow>();
            window.items = items;
            window.onItemSelected = callback;
            window.titleContent = new GUIContent("Select Ingredient");
            window.minSize = new Vector2(300, 400);
            window.ShowUtility();
            manager = ItemManagerWindow.Manager;
            manager.RecipesEditor._selectedCategoryEditor = category;
        }

        private void OnGUI()
        {
            // Выбор категории
            manager.RecipesEditor._selectedCategoryEditor = 
                (ItemCategoryEditor)EditorGUILayout.EnumPopup("Category", manager.RecipesEditor._selectedCategoryEditor);

            EditorGUILayout.Space(5);
            scroll = EditorGUILayout.BeginScrollView(scroll);

            foreach (var item in items)
            {
                if (item == null) continue;
                if (manager.RecipesEditor._selectedCategoryEditor != ItemCategoryEditor.All &&
                    !manager.ItemMatchesCategory(item, manager.RecipesEditor._selectedCategoryEditor))
                    continue;

                EditorGUILayout.BeginHorizontal("box");

                // Иконка
                if (item.Header.icon != null)
                    EditorUtils.DrawSprite(item.Header.icon, 30f);
                else
                {
                    var rect = GUILayoutUtility.GetRect(30, 30, GUILayout.ExpandWidth(false));
                    EditorGUI.DrawRect(rect, Color.black);
                }

                // Кнопка выбора предмета
                if (GUILayout.Button(item.Header.titleLid, GUILayout.Height(30)))
                {
                    onItemSelected?.Invoke(item);
                    Close();
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
        }

        

    }

}