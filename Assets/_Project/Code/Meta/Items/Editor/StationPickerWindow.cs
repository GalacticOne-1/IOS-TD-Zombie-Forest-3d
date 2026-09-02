using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using Galactic1.Configs;
using Galactic1.Game.Meta.Items;

namespace Galactic1.Tools
{

    public class StationPickerWindow : EditorWindow
    {
        private IReadOnlyCollection<CraftingStationModule> stations;
        private Action<CraftingStationModule> onStationSelected;
        private Vector2 scroll;

        public static void ShowWindow(
            IReadOnlyCollection<CraftingStationModule> stationList,
            Action<CraftingStationModule> callback)
        {
            var window = CreateInstance<StationPickerWindow>();
            window.stations = stationList;
            window.onStationSelected = callback;
            window.titleContent = new GUIContent("Select Station");
            window.minSize = new Vector2(300, 400);
            window.ShowUtility();
        }

        private void OnGUI()
        {
            if (stations == null || stations.Count == 0)
            {
                EditorGUILayout.HelpBox("No stations available!", MessageType.Warning);
                return;
            }

            scroll = EditorGUILayout.BeginScrollView(scroll);

            foreach (var station in stations)
            {
                if (station == null) continue;

                if (station.Item == null)
                {
                    Debug.LogError($"[{station}] not have ItemBase");
                    continue;
                }

                EditorGUILayout.BeginHorizontal("box");

                // Иконка станции
                if (station.Item.Header.icon != null)
                    EditorUtils.DrawSprite(station.Item.Header.icon, 30f);
                else
                {
                    var rect = GUILayoutUtility.GetRect(30, 30, GUILayout.ExpandWidth(false));
                    EditorGUI.DrawRect(rect, Color.black);
                }

                // Название станции
                if (GUILayout.Button(station.Item.Header.titleLid, GUILayout.Height(30)))
                {
                    onStationSelected?.Invoke(station);
                    Close();
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
        }
    }
}