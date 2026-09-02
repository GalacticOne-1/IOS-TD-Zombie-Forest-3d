
using System.Collections.Generic;
using System.Linq;
using Galactic1.Core.Enums;
using Galactic1.Game.Meta.Items;
using UnityEditor;
using UnityEngine;
using Galactic1.Items;

namespace Galactic1.Tools
{
    public class ItemActionsEditor
    {
        private ItemManagerWindow itemManager;
        private ItemActionDatabase actionDatabase;
        private ItemActionConfig selectedAction;
        private int? pendingDeleteIndex = null;

        
        private Vector2 actionScroll;
        private Vector2 consumableScroll;
        private bool showAction;


        public ItemActionsEditor(ItemManagerWindow itemManager)
        {
            this.itemManager = itemManager;

            LoadActionDatabase();
        }


        private void LoadActionDatabase()
        {
            string[] guids = AssetDatabase.FindAssets("t:ItemActionDatabase");

            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                actionDatabase = AssetDatabase.LoadAssetAtPath<ItemActionDatabase>(path);
            }
            else
            {
                Debug.LogWarning("ItemActionDatabase not found. Please create it via menu.");
            }
        }




        // ============================================================
        // GUI ENTRY
        // ============================================================
        public void DrawPanel()
        {
            if (actionDatabase == null)
            {
                EditorGUILayout.HelpBox("ItemActionDatabase not found!", MessageType.Error);
                return;
            }
            
            if (itemManager.SelectedItem == null)
            {
                EditorGUILayout.LabelField("Выберите предмет слева", EditorStyles.centeredGreyMiniLabel);
                return;
            }

            EditorGUILayout.BeginVertical("box", GUILayout.Width(400));

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("🧩 Actions", EditorStyles.boldLabel, GUILayout.Width(100));
            DrawAddSection();
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);
            DrawActionsList();

            EditorGUILayout.EndVertical();
            DrawConsumableData();
        }






        // ============================================================
        // ACTIONS LIST
        // ============================================================
        private void DrawActionsList()
        {
            actionScroll = EditorGUILayout.BeginScrollView(actionScroll, GUILayout.Height(150));

            if (itemManager.SelectedItem == null ||
                !itemManager.SelectedItem.HasModule<ActionModule>() ||
                itemManager.SelectedItem.Action.Actions == null
               )
            {
                EditorGUILayout.HelpBox("No actions found for selected ItemBase", MessageType.Info);
                EditorGUILayout.EndScrollView();
                return;
            }

            for (int i = 0; i < itemManager.SelectedItem.Action.Actions.Count; i++)
            {
                var actionObj = itemManager.SelectedItem.Action.Actions[i];
                if (actionObj == null) continue;

                if (selectedAction == actionObj)
                    GUI.backgroundColor = Color.cyan;

                EditorGUILayout.BeginHorizontal("box");
                GUI.backgroundColor = Color.white;

                if (GUILayout.Button(actionObj.name, GUILayout.Height(22)))
                {
                    showAction = !showAction;
                    selectedAction = actionObj;
                    Selection.activeObject = actionObj;
                    EditorGUIUtility.PingObject(actionObj);
                }

                GUI.backgroundColor = Color.red;
                if (GUILayout.Button("x", GUILayout.Width(20)))
                {
                    pendingDeleteIndex = i;
                }
                GUI.backgroundColor = Color.white;

                EditorGUILayout.EndHorizontal();

                // окно подтверждения
                if (pendingDeleteIndex == i)
                {
                    EditorGUILayout.BeginVertical("box");
                    EditorGUILayout.HelpBox($"Delete '{actionObj.name}' ?", MessageType.Warning);

                    EditorGUILayout.BeginHorizontal();

                    if (GUILayout.Button("Yes", GUILayout.Width(45)))
                    {
                        DeleteAction(i, actionObj);
                        pendingDeleteIndex = null;
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.EndVertical();
                        break;
                    }

                    if (GUILayout.Button("No", GUILayout.Width(45)))
                        pendingDeleteIndex = null;

                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                }

                // вложенный инспектор
                if (showAction && selectedAction == actionObj)
                {
                    EditorGUILayout.BeginVertical("box");
                    var edt = Editor.CreateEditor(actionObj);
                    edt.OnInspectorGUI();
                    EditorGUILayout.EndVertical();
                }
            }

            EditorGUILayout.EndScrollView();
        }
        
        
        

        private void DrawConsumableData()
        {
            EditorGUILayout.BeginVertical("box", GUILayout.Width(400));
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Consumable Data", EditorStyles.boldLabel, GUILayout.Width(120));

            var hasUse = itemManager.SelectedItem.HasModule<UseModule>();
            if (!hasUse)
            {
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.HelpBox("Not exist -> module.use", MessageType.Info);
                EditorGUILayout.EndVertical();
                return;
            }
            
            var useModule = itemManager.SelectedItem.Use;
            
            // Enabled
            bool newEnabled = EditorGUILayout.Toggle(useModule.ConsumeOnUse);
            if (newEnabled != useModule.ConsumeOnUse)
            {
                //itemManager.selectedItem.Config.ConsumableData.enabled = newEnabled;
                useModule.SetConsume = newEnabled;
                EditorUtility.SetDirty(itemManager.SelectedItem);
            }

            // Add effect
            if (hasUse && useModule.ConsumeOnUse &&
                GUILayout.Button("+", GUILayout.Width(20), GUILayout.Height(20)))
            {
                var ef = useModule.Effects.ToList();
                ef.Add(new());
                useModule.SetEffects(ef);
                EditorUtility.SetDirty(itemManager.SelectedItem);
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(4);

            consumableScroll = EditorGUILayout.BeginScrollView(consumableScroll, GUILayout.Height(200));

            if (hasUse && !useModule.ConsumeOnUse)
            {
                EditorGUILayout.EndScrollView();
                EditorGUILayout.EndVertical();
                return;
            }

            // UseType
            // var newUseType =
            //     (UseType)EditorGUILayout.EnumPopup("Use Type", itemManager.selectedItem.Config.ConsumableData.useType);
            // if (newUseType != itemManager.selectedItem.Config.ConsumableData.useType)
            // {
            //     itemManager.selectedItem.Config.ConsumableData.useType = newUseType;
            //     EditorUtility.SetDirty(itemManager.selectedItem.Config);
            // }

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Effects", EditorStyles.boldLabel);

            var effects = useModule.Effects.ToList() ?? new();
            for (int i = 0; i < effects.Count; i++)
            {
                var ef = effects[i];
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.BeginHorizontal();

                var newKey = (StatId)EditorGUILayout.EnumPopup(ef.StatId, GUILayout.Width(120));
                var newOperation = (ModifierOperation)EditorGUILayout.EnumPopup(ef.Operation, GUILayout.Width(120));
                float newVal = EditorGUILayout.FloatField(ef.Value, GUILayout.Width(60));

                if (newKey != ef.StatId ||
                    newOperation != ef.Operation ||
                    !Mathf.Approximately(newVal, ef.Value))
                {
                    ef.StatId = newKey;
                    ef.Operation = newOperation;
                    ef.Value = newVal;
                    effects[i] = ef;
                    EditorUtility.SetDirty(itemManager.SelectedItem);
                }

                GUI.backgroundColor = Color.red;
                if (GUILayout.Button("x", GUILayout.Width(20)))
                {
                    effects.RemoveAt(i);
                    useModule.SetEffects(effects);
                    
                    GUI.backgroundColor = Color.white;
                    EditorUtility.SetDirty(itemManager.SelectedItem);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    break;
                }
                GUI.backgroundColor = Color.white;

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }







        // ============================================================
        // DELETE ACTION
        // ============================================================
        private void DeleteAction(int index, ItemActionConfig asset)
        {
            // Удаляем из массива
            var list = itemManager.SelectedItem.Action.Actions.ToList();
            list.RemoveAt(index);
            itemManager.SelectedItem.Action.SetActions(list);

            EditorUtility.SetDirty(itemManager.SelectedItem);

            if (selectedAction == asset)
                selectedAction = null;
        }




        // ============================================================
        // ADD NEW ACTION
        // ============================================================
        private void DrawAddSection()
        {
            EditorGUILayout.Space(10);

            if (actionDatabase.actions.Count == 0)
            {
                EditorGUILayout.HelpBox("Database is empty", MessageType.Warning);
                return;
            }

            if (!itemManager.SelectedItem.HasModule<ActionModule>())
            {
                EditorGUILayout.HelpBox("Not exist -> module.action", MessageType.Info);
                return;
            }

            string[] names = actionDatabase.actions.Select(a => a.name).ToArray();
            int pick = EditorGUILayout.Popup(-1, names, GUILayout.Width(200));

            if (pick >= 0)
                CreateNewAction(pick);
        }


        private void CreateNewAction(int index)
        {
            if (itemManager.SelectedItem == null) return;

            var actionRef = actionDatabase.actions[index];
            if (actionRef == null) return;

            // Добавляем ссылку на конфиг из базы
            var list = itemManager.SelectedItem.Action.Actions?.ToList() ?? new List<ItemActionConfig>();
            list.Add(actionRef);
            itemManager.SelectedItem.Action.SetActions(list);

            EditorUtility.SetDirty(itemManager.SelectedItem);

            Debug.Log($"Linked action: {actionRef.name} to {itemManager.SelectedItem.name}");
        }




    }
}
