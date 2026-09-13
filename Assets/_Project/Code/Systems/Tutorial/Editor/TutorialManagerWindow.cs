using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Galactic1.Code.Systems.Tutorial.Authoring;

namespace Galactic1.Tools
{
    public class TutorialManagerWindow : EditorWindow
    {
        private const int ObjectPickerControlId = 789456123;
        private static readonly Color ErrorColor = new(0.85f, 0.16f, 0.16f, 0.9f);
        private static readonly Color OkColor = new(0.2f, 0.75f, 0.3f, 0.85f);

        private delegate bool ValidateDelegate(out string error);

        private TutorialManager_MenuHelper MenuHelper;
        public TutorialCampaignRegistry registry { get; private set; }
        private readonly List<TutorialDefinition> campaigns = new();
        private readonly Dictionary<TutorialDefinition, List<string>> campaignErrorsCache = new();

        public TutorialDefinition selectedCampaign { get; private set; }
        public TutorialChapterDefinition selectedChapter;
        private TutorialStepDefinition selectedStep;
        private SerializedObject stepSO;
        public TutorialAssetCreationSettings assetCreationSettings { get; private set; }

        /// <summary>Установленный "Add Existing"-запросом коллбэк, вызывается когда
        /// ObjectPicker закрывается с выбранным объектом — единая точка для objectives
        /// и guidance conditions, чтобы не дублировать пикер-машинерию под каждый тип.</summary>
        private Action<UnityEngine.Object> pendingPickerCallback;

        // Сворачиваемость крупных секций — состояние окна, не персистится в ассет.
        private bool objectivesFoldout = true;
        private bool presentationFoldout = true;
        private bool guidanceFoldout = true;
        private bool rewardFoldout = true;
        private bool graphFoldout = true;
        private bool metaFoldout = true;

        private Vector2 leftScroll;
        private Vector2 centerScroll;
        private Color originalBg;

        [MenuItem("Tools/Tutorial/Tutorial Manager")]
        public static void ShowWindow() => GetWindow<TutorialManagerWindow>("Tutorial Manager");

        private void OnEnable()
        {
            MenuHelper = new TutorialManager_MenuHelper(this);
            originalBg = GUI.backgroundColor;
            LoadRegistry();
            LoadAssetCreationSettings();
            RefreshCampaignList();
        }

        private void LoadRegistry()
        {
            var guids = AssetDatabase.FindAssets("t:TutorialCampaignRegistry");
            registry = guids.Length > 0
                ? AssetDatabase.LoadAssetAtPath<TutorialCampaignRegistry>(AssetDatabase.GUIDToAssetPath(guids[0]))
                : null;
        }
        
        private void LoadAssetCreationSettings()
        {
            var guids = AssetDatabase.FindAssets("t:TutorialAssetCreationSettings");

            assetCreationSettings = guids.Length > 0
                ? AssetDatabase.LoadAssetAtPath<TutorialAssetCreationSettings>(AssetDatabase.GUIDToAssetPath(guids[0]))
                : null;
        }
        

        public void RefreshCampaignList()
        {
            campaigns.Clear();
            campaignErrorsCache.Clear();

            if (registry != null && registry.campaigns != null)
            {
                campaigns.AddRange(registry.campaigns.Where(c => c != null));
            }
            else
            {
                // Fallback: реестр не назначен — ищем все TutorialDefinition-ассеты проекта.
                foreach (var guid in AssetDatabase.FindAssets("t:TutorialDefinition"))
                {
                    var def = AssetDatabase.LoadAssetAtPath<TutorialDefinition>(AssetDatabase.GUIDToAssetPath(guid));
                    if (def != null) campaigns.Add(def);
                }
            }

            foreach (var c in campaigns)
                campaignErrorsCache[c] = ValidateCampaign(c);
        }

        private List<string> ValidateCampaign(TutorialDefinition def)
        {
            if (def == null) return new List<string> { "Campaign asset is null." };
            def.ValidateGraph(out var errors);
            return errors ?? new List<string>();
        }

        public void RefreshCampaignErrors(TutorialDefinition campaign)
        {
            if (campaign != null) campaignErrorsCache[campaign] = ValidateCampaign(campaign);
        }

        private bool HasErrors(TutorialDefinition def)
            => campaignErrorsCache.TryGetValue(def, out var errors) && errors.Count > 0;

        // ================================================================
        // OnGUI
        // ================================================================
        private void OnGUI()
        {
            HandleObjectPicker();

            EditorGUILayout.BeginHorizontal();
            DrawLeftColumn();
            GUILayout.Space(5);
            EditorGUILayout.BeginVertical();
            DrawCenterPanel();
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(8);
            DrawBottomNavBar();
            EditorGUILayout.Space(4);
        }

        // ================================================================
        // LEFT COLUMN — CAMPAIGNS
        // ================================================================
        private void DrawLeftColumn()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(250));

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("📋 Campaigns", EditorStyles.boldLabel);
            if (GUILayout.Button("+", GUILayout.Width(24))) MenuHelper.CreateNewCampaign();
            if (GUILayout.Button("Refresh", GUILayout.Width(70))) RefreshCampaignList();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginVertical("box");
            leftScroll = EditorGUILayout.BeginScrollView(leftScroll);

            foreach (var campaign in campaigns)
            {
                if (campaign == null) continue;
                DrawCampaignButton(campaign);
            }

            if (campaigns.Count == 0)
                EditorGUILayout.HelpBox(
                    "No TutorialDefinition assets found. Assign a TutorialCampaignRegistry or create a campaign.",
                    MessageType.Info);

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndVertical();
        }

        private void DrawCampaignButton(TutorialDefinition campaign)
        {
            bool isSelected = campaign == selectedCampaign;
            bool hasErrors = HasErrors(campaign);

            EditorGUILayout.BeginHorizontal();

            if (isSelected) GUI.backgroundColor = Color.cyan;
            string label = campaign.campaignId != null ? campaign.campaignId.DebugKey : campaign.name;
            if (GUILayout.Button(label, GUILayout.Width(200), GUILayout.Height(26)))
                SelectCampaign(campaign);
            GUI.backgroundColor = originalBg;

            var indicatorRect = GUILayoutUtility.GetRect(20, 20, GUILayout.Width(20), GUILayout.Height(20));
            EditorGUI.DrawRect(indicatorRect, hasErrors ? ErrorColor : OkColor);
            GUI.Label(indicatorRect, new GUIContent("",
                hasErrors ? string.Join("\n", campaignErrorsCache[campaign]) : "No problems found."));

            EditorGUILayout.EndHorizontal();

            if (hasErrors && isSelected)
            {
                var errors = campaignErrorsCache[campaign];
                string shown = string.Join("\n", errors.Take(5));
                if (errors.Count > 5) shown += $"\n… +{errors.Count - 5} more";
                EditorGUILayout.HelpBox(shown, MessageType.Error);
            }

            GUILayout.Space(2);
        }

        public void SelectCampaign(TutorialDefinition campaign)
        {
            selectedCampaign = campaign;
            selectedChapter = campaign.chapters != null && campaign.chapters.Count > 0 ? campaign.chapters[0] : null;
            SelectStep(selectedChapter != null && selectedChapter.steps.Count > 0 ? selectedChapter.steps[0] : null);
        }

        

        // ================================================================
        // BOTTOM NAV BAR — CHAPTER / STEP DROPDOWNS
        // ================================================================
        private void DrawBottomNavBar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            GUI.enabled = selectedCampaign != null;
            string chapterLabel = selectedChapter != null
                ? (selectedChapter.chapterId != null ? selectedChapter.chapterId.DebugKey : selectedChapter.name)
                : "— Select Chapter —";
            if (GUILayout.Button(chapterLabel, EditorStyles.toolbarDropDown, GUILayout.Width(220)))
                ShowChapterMenu(GUILayoutUtility.GetLastRect());

            GUI.enabled = selectedChapter != null;
            string stepLabel = selectedStep != null
                ? (selectedStep.stepId != null ? selectedStep.stepId.DebugKey : selectedStep.name)
                : "— Select Step —";
            if (GUILayout.Button(stepLabel, EditorStyles.toolbarDropDown, GUILayout.Width(220)))
                ShowStepMenu(GUILayoutUtility.GetLastRect());

            GUI.enabled = true;
            GUILayout.FlexibleSpace();

            if (selectedStep != null && GUILayout.Button("Ping Asset", EditorStyles.toolbarButton, GUILayout.Width(90)))
                EditorGUIUtility.PingObject(selectedStep);

            EditorGUILayout.EndHorizontal();
        }

        private void ShowChapterMenu(Rect activatorRect)
        {
            var menu = new GenericMenu();

            if (selectedCampaign?.chapters != null)
            {
                foreach (var chapter in selectedCampaign.chapters)
                {
                    if (chapter == null) continue;
                    string label = chapter.chapterId != null ? chapter.chapterId.DebugKey : chapter.name;
                    var captured = chapter;
                    menu.AddItem(new GUIContent(label), chapter == selectedChapter, () =>
                    {
                        selectedChapter = captured;
                        SelectStep(captured.steps != null && captured.steps.Count > 0 ? captured.steps[0] : null);
                    });
                }
            }

            if (menu.GetItemCount() == 0) menu.AddDisabledItem(new GUIContent("No chapters"));
            menu.AddSeparator("");
            menu.AddItem(new GUIContent("+ New Chapter"), false, MenuHelper.CreateNewChapter);

            menu.DropDown(activatorRect);
        }

        private void ShowStepMenu(Rect activatorRect)
        {
            var menu = new GenericMenu();

            if (selectedChapter?.steps != null)
            {
                foreach (var step in selectedChapter.steps)
                {
                    if (step == null) continue;
                    string label = step.stepId != null ? step.stepId.DebugKey : step.name;
                    if (!step.Validate(out _)) label = "⚠ " + label;
                    var captured = step;
                    menu.AddItem(new GUIContent(label), step == selectedStep, () => SelectStep(captured));
                }
            }

            if (menu.GetItemCount() == 0) menu.AddDisabledItem(new GUIContent("No steps"));
            menu.AddSeparator("");
            menu.AddItem(new GUIContent("+ New Step"), false, MenuHelper.CreateNewStep);

            menu.DropDown(activatorRect);
        }

        public void SelectStep(TutorialStepDefinition step)
        {
            selectedStep = step;
            stepSO = step != null ? new SerializedObject(step) : null;
        }

        

        

        

        // ================================================================
        // CENTER PANEL — STEP DETAILS
        // ================================================================
        private void DrawCenterPanel()
        {
            if (selectedStep == null)
            {
                EditorGUILayout.LabelField("Select a step to see details", EditorStyles.centeredGreyMiniLabel);
                return;
            }

            if (stepSO == null || stepSO.targetObject != selectedStep)
                stepSO = new SerializedObject(selectedStep);

            stepSO.Update();
            centerScroll = EditorGUILayout.BeginScrollView(centerScroll);

            DrawStepHeader();
            EditorGUILayout.Space(5);
            DrawStepValidation();
            EditorGUILayout.Space(5);
            DrawObjectivesBox();
            EditorGUILayout.Space(5);
            DrawPresentationBox();
            EditorGUILayout.Space(5);
            DrawGuidanceBox();
            EditorGUILayout.Space(5);
            DrawRewardBox();
            EditorGUILayout.Space(5);
            DrawGraphBox();
            EditorGUILayout.Space(5);
            DrawMetaBox();

            EditorGUILayout.EndScrollView();

            if (stepSO.ApplyModifiedProperties())
            {
                EditorUtility.SetDirty(selectedStep);
                RefreshCampaignErrors(selectedCampaign);
            }
        }

        private void DrawStepHeader()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.SelectableLabel(
                selectedStep.stepId != null ? selectedStep.stepId.DebugKey : "StepId: ----",
                EditorStyles.boldLabel, GUILayout.Height(18));
            if (GUILayout.Button("Ping", GUILayout.Width(50)))
                EditorGUIUtility.PingObject(selectedStep);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.PropertyField(stepSO.FindProperty("stepId"));
            EditorGUILayout.PropertyField(stepSO.FindProperty("chapterId"));
            EditorGUILayout.PropertyField(stepSO.FindProperty("analyticsStepIndex"));
            EditorGUILayout.EndVertical();
        }

        private void DrawStepValidation()
        {
            if (!selectedStep.Validate(out var error))
                EditorGUILayout.HelpBox(error, MessageType.Error);
        }

        // ---------------- Objectives ----------------
        private void DrawObjectivesBox()
        {
            objectivesFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(objectivesFoldout, "🎯 Objectives");
            if (objectivesFoldout)
            {
                EditorGUILayout.BeginVertical("box");

                var objectivesProp = stepSO.FindProperty("objectives");
                var modeProp = objectivesProp.FindPropertyRelative("mode");
                var listProp = objectivesProp.FindPropertyRelative("objectives");

                EditorGUILayout.PropertyField(modeProp, new GUIContent("Composition Mode"));
                EditorGUILayout.Space(4);

                if (listProp.arraySize == 0)
                    EditorGUILayout.HelpBox("Objective group is empty — step can never complete.", MessageType.Warning);

                for (int i = 0; i < listProp.arraySize; i++)
                    DrawObjectiveEntry(listProp, i);

                EditorGUILayout.Space(4);
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("+ Add Existing", GUILayout.Width(130)))
                    ShowAddExistingMenu<TutorialObjectiveDefinition>(picked => AddArrayElement(listProp, picked));
                if (GUILayout.Button("+ Create New", GUILayout.Width(130)))
                {
                    ShowCreateAssetMenu<TutorialObjectiveDefinition>(
                        instance => AddArrayElement(listProp, instance));
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawObjectiveEntry(SerializedProperty listProp, int index)
        {
            var elementProp = listProp.GetArrayElementAtIndex(index);
            var objective = elementProp.objectReferenceValue as TutorialObjectiveDefinition;

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();

            if (objective == null)
            {
                GUI.backgroundColor = ErrorColor;
                EditorGUILayout.LabelField("⚠ Empty objective slot", GUILayout.Width(300));
                GUI.backgroundColor = originalBg;
            }
            else
            {
                string title = objective.ObjectiveTypeId +
                    (string.IsNullOrEmpty(objective.debugDescription) ? "" : $" — {objective.debugDescription}");
                //elementProp.isExpanded = EditorGUILayout.Foldout(elementProp.isExpanded, title, true);
                EditorGUILayout.LabelField(title, EditorStyles.label);

                if (GUILayout.Button("Ping", GUILayout.Width(45)))
                    EditorGUIUtility.PingObject(objective);
            }

            GUI.backgroundColor = Color.red;
            bool removed = GUILayout.Button("x", GUILayout.Width(20));
            GUI.backgroundColor = originalBg;
            EditorGUILayout.EndHorizontal();

            if (removed)
            {
                listProp.DeleteArrayElementAtIndex(index);
                EditorGUILayout.EndVertical();
                return;
            }

            // if (objective != null && elementProp.isExpanded)
            // {
            //     EditorGUI.indentLevel++;
            //     DrawGenericInlineEditor(objective, objective.Validate);
            //     EditorGUI.indentLevel--;
            // }

            EditorGUILayout.EndVertical();
        }

        // ---------------- Presentation (legacy fallback) ----------------
        private void DrawPresentationBox()
        {
            presentationFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(presentationFoldout, "🎬 Presentation");
            if (presentationFoldout)
            {
                EditorGUILayout.BeginVertical("box");

                var p = stepSO.FindProperty("presentation");

                EditorGUILayout.LabelField("Instruction", EditorStyles.miniBoldLabel);
                EditorGUILayout.PropertyField(p.FindPropertyRelative("instructionTitleKey"), new GUIContent("Title Key"));
                EditorGUILayout.PropertyField(p.FindPropertyRelative("instructionDesKey"), new GUIContent("Description Key"));

                EditorGUILayout.Space(4);
                EditorGUILayout.LabelField("Highlight / Arrow (legacy fallback — used only when Guidance is empty)",
                    EditorStyles.miniBoldLabel);
                EditorGUILayout.PropertyField(p.FindPropertyRelative("highlightTargetId"), new GUIContent("Highlight Target"));
                EditorGUILayout.PropertyField(p.FindPropertyRelative("highlightItemId"), new GUIContent("Highlight Item (inventory slot)"));
                EditorGUILayout.PropertyField(p.FindPropertyRelative("highlightInboxItemId"), new GUIContent("Highlight Item (inbox slot)"));
                EditorGUILayout.PropertyField(p.FindPropertyRelative("arrowTargetId"), new GUIContent("Arrow Target"));

                EditorGUILayout.Space(4);
                EditorGUILayout.LabelField("Dialogue", EditorStyles.miniBoldLabel);
                EditorGUILayout.HelpBox("Не обрабатывается — диалоговая система в проекте не найдена. Поле декларативно.",
                    MessageType.None);
                EditorGUILayout.PropertyField(p.FindPropertyRelative("dialogueId"), new GUIContent("Dialogue Id"));

                EditorGUILayout.Space(4);
                EditorGUILayout.LabelField("Camera", EditorStyles.miniBoldLabel);
                EditorGUILayout.PropertyField(p.FindPropertyRelative("cameraFocusTargetId"), new GUIContent("Camera Focus Target"));

                EditorGUILayout.Space(4);
                EditorGUILayout.LabelField("Input", EditorStyles.miniBoldLabel);
                EditorGUILayout.PropertyField(p.FindPropertyRelative("inputPolicy"), new GUIContent("Input Policy"));

                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        // ---------------- Guidance ----------------
        private void DrawGuidanceBox()
        {
            guidanceFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(
                guidanceFoldout, "🧭 Guidance (dynamic highlight, first satisfied wins)");
            if (guidanceFoldout)
            {
                EditorGUILayout.BeginVertical("box");

                var guidanceProp = stepSO.FindProperty("guidance");

                if (guidanceProp.arraySize == 0)
                    EditorGUILayout.HelpBox(
                        "Empty — falls back to presentation.highlight/arrow/camera directly (legacy behaviour).",
                        MessageType.Info);

                for (int i = 0; i < guidanceProp.arraySize; i++)
                    DrawGuidanceEntry(guidanceProp, i);

                if (GUILayout.Button("+ Add Guidance Entry", GUILayout.Width(180)))
                    guidanceProp.InsertArrayElementAtIndex(guidanceProp.arraySize);

                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawGuidanceEntry(SerializedProperty guidanceProp, int index)
        {
            var entryProp = guidanceProp.GetArrayElementAtIndex(index);
            var conditionProp = entryProp.FindPropertyRelative("condition");
            var presentationProp = entryProp.FindPropertyRelative("presentation");
            var condition = conditionProp.objectReferenceValue as TutorialGuidanceConditionDefinition;

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"#{index}", GUILayout.Width(25));
            EditorGUILayout.LabelField(
                condition != null ? condition.ConditionTypeId : "Unconditional (always true)",
                EditorStyles.miniBoldLabel);

            GUI.backgroundColor = Color.red;
            bool removed = GUILayout.Button("x", GUILayout.Width(20));
            GUI.backgroundColor = originalBg;
            EditorGUILayout.EndHorizontal();

            if (removed)
            {
                guidanceProp.DeleteArrayElementAtIndex(index);
                EditorGUILayout.EndVertical();
                return;
            }

            EditorGUILayout.Space(2);
            EditorGUILayout.LabelField("Condition", EditorStyles.miniBoldLabel);
            EditorGUILayout.PropertyField(conditionProp, new GUIContent("Condition (null = always true)"));

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+ Add Existing", GUILayout.Width(120)))
                ShowAddExistingMenu<TutorialGuidanceConditionDefinition>(picked =>
                {
                    conditionProp.serializedObject.Update();
                    conditionProp.objectReferenceValue = picked;
                    conditionProp.serializedObject.ApplyModifiedProperties();
                    RefreshCampaignErrors(selectedCampaign);
                });
            if (GUILayout.Button("+ Create New", GUILayout.Width(120)))
            {
                ShowCreateAssetMenu<TutorialGuidanceConditionDefinition>(
                    instance =>
                    {
                        conditionProp.serializedObject.Update();
                        conditionProp.objectReferenceValue = instance;
                        conditionProp.serializedObject.ApplyModifiedProperties();

                        RefreshCampaignErrors(selectedCampaign);
                    });
            }
            EditorGUILayout.EndHorizontal();

            // if (condition != null)
            // {
            //     EditorGUI.indentLevel++;
            //     DrawGenericInlineEditor(condition, condition.Validate);
            //     EditorGUI.indentLevel--;
            // }

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Target (highlight / arrow / camera)", EditorStyles.miniBoldLabel);
            EditorGUILayout.PropertyField(presentationProp.FindPropertyRelative("highlightTargetId"), new GUIContent("Highlight Target"));
            EditorGUILayout.PropertyField(presentationProp.FindPropertyRelative("highlightItemId"), new GUIContent("Highlight Item (inventory slot)"));
            EditorGUILayout.PropertyField(presentationProp.FindPropertyRelative("highlightInboxItemId"), new GUIContent("Highlight Item (inbox slot)"));
            EditorGUILayout.PropertyField(presentationProp.FindPropertyRelative("arrowTargetId"), new GUIContent("Arrow Target"));
            EditorGUILayout.PropertyField(presentationProp.FindPropertyRelative("cameraFocusTargetId"), new GUIContent("Camera Focus Target"));

            EditorGUILayout.EndVertical();
        }

        // ---------------- Reward ----------------
        // NOTE: каждое поле рисуется БЕЗ встроенного лейбла (GUIContent.none) и с отдельным
        // компактным GUILayout.Label фиксированной ширины перед ним. PropertyField(prop, label,
        // GUILayout.Width(w)) резервирует w ПОД ВЕСЬ контрол (лейбл+поле вместе) — при таком
        // подходе числовые поля с непустым label получали настолько узкую область ввода, что
        // визуально наезжали на соседний контрол и не реагировали на клик (сообщённый баг:
        // "работает только itemId, durability/ammo/remove не работают"). Раздельные Label+
        // PropertyField(..., GUIContent.none, width) каждый получает свою явно заданную ширину
        // без внутреннего резерва под лейбл.
        private void DrawRewardBox()
        {
            rewardFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(rewardFoldout, "🎁 Reward");
            if (rewardFoldout)
            {
                EditorGUILayout.BeginVertical("box");

                var itemsProp = stepSO.FindProperty("reward").FindPropertyRelative("items");

                for (int i = 0; i < itemsProp.arraySize; i++)
                {
                    var itemProp = itemsProp.GetArrayElementAtIndex(i);
                    EditorGUILayout.BeginHorizontal("box");

                    EditorGUILayout.PropertyField(
                        itemProp.FindPropertyRelative("itemId"), GUIContent.none, GUILayout.Width(160));

                    GUILayout.Label("x", GUILayout.Width(12));
                    EditorGUILayout.PropertyField(
                        itemProp.FindPropertyRelative("amount"), GUIContent.none, GUILayout.Width(40));

                    GUILayout.Label("Dur", GUILayout.Width(26));
                    EditorGUILayout.PropertyField(
                        itemProp.FindPropertyRelative("durability"), GUIContent.none, GUILayout.Width(45));

                    GUILayout.Label("Ammo", GUILayout.Width(34));
                    EditorGUILayout.PropertyField(
                        itemProp.FindPropertyRelative("ammoInMagazine"), GUIContent.none, GUILayout.Width(45));

                    GUILayout.FlexibleSpace();

                    GUI.backgroundColor = Color.red;
                    bool removed = GUILayout.Button("x", GUILayout.Width(20));
                    GUI.backgroundColor = originalBg;

                    EditorGUILayout.EndHorizontal();

                    if (removed)
                    {
                        itemsProp.DeleteArrayElementAtIndex(i);
                        break;
                    }
                }

                if (GUILayout.Button("+ Add Reward Item", GUILayout.Width(150)))
                    itemsProp.InsertArrayElementAtIndex(itemsProp.arraySize);

                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        // ---------------- Graph / Transitions ----------------
        private void DrawGraphBox()
        {
            graphFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(graphFoldout, "🔀 Graph / Transitions");
            if (graphFoldout)
            {
                EditorGUILayout.BeginVertical("box");

                var transitionsProp = stepSO.FindProperty("transitions");

                if (transitionsProp.arraySize == 0)
                    EditorGUILayout.HelpBox("No transitions — this step is terminal.", MessageType.Info);

                for (int i = 0; i < transitionsProp.arraySize; i++)
                    if (DrawTransitionEntry(transitionsProp, i))
                        break;

                if (GUILayout.Button("+ Add Transition", GUILayout.Width(150)))
                    transitionsProp.InsertArrayElementAtIndex(transitionsProp.arraySize);

                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        /// <summary>Returns true if the entry was removed this frame (caller must stop
        /// iterating the now-shifted array).</summary>
        private bool DrawTransitionEntry(SerializedProperty transitionsProp, int index)
        {
            var t = transitionsProp.GetArrayElementAtIndex(index);
            var nextStepIdProp = t.FindPropertyRelative("nextStepId");

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"#{index}", GUILayout.Width(25));
            EditorGUILayout.PropertyField(nextStepIdProp, new GUIContent("Next Step (null = terminal)"));

            GUI.backgroundColor = Color.red;
            bool removed = GUILayout.Button("x", GUILayout.Width(20));
            GUI.backgroundColor = originalBg;
            EditorGUILayout.EndHorizontal();

            // Запрещаем self-loop: шаг не может переходить сам в себя. Проверяем сразу после
            // PropertyField — если только что назначили stepId текущего шага, откатываем.
            if (nextStepIdProp.objectReferenceValue != null && selectedStep.stepId != null &&
                nextStepIdProp.objectReferenceValue == selectedStep.stepId)
            {
                nextStepIdProp.objectReferenceValue = null;
                EditorGUILayout.HelpBox(
                    "A step cannot transition to itself — selection reverted to empty (terminal).",
                    MessageType.Warning);
            }

            if (!removed)
                EditorGUILayout.PropertyField(t.FindPropertyRelative("condition"),
                    new GUIContent("Condition (null = unconditional)"), true);

            EditorGUILayout.EndVertical();

            if (removed)
                transitionsProp.DeleteArrayElementAtIndex(index);

            return removed;
        }

        // ---------------- Persistence / Resume ----------------
        private void DrawMetaBox()
        {
            metaFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(metaFoldout, "⚙️ Persistence / Resume");
            if (metaFoldout)
            {
                EditorGUILayout.BeginVertical("box");

                EditorGUILayout.PropertyField(stepSO.FindProperty("isCheckpoint"), new GUIContent("Is Checkpoint"));
                EditorGUILayout.PropertyField(stepSO.FindProperty("requiredDomain"), new GUIContent("Required Domain"));

                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        // ================================================================
        // GENERIC ASSET PICKING / CREATION (shared by Objectives + Guidance)
        // ================================================================

        /// <summary>Generic-инлайн редактор: итерирует сериализуемые поля ассета —
        /// избегаем написания отдельного custom-редактора под каждый из десятков
        /// Objective/GuidanceCondition типов.</summary>
        private void DrawGenericInlineEditor(ScriptableObject asset, ValidateDelegate validate)
        {
            var so = new SerializedObject(asset);
            so.Update();

            var prop = so.GetIterator();
            bool enterChildren = true;
            EditorGUI.BeginChangeCheck();

            while (prop.NextVisible(enterChildren))
            {
                enterChildren = false;
                if (prop.name == "m_Script") continue;
                EditorGUILayout.PropertyField(prop, true);
            }

            if (EditorGUI.EndChangeCheck())
            {
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(asset);
                RefreshCampaignErrors(selectedCampaign);
            }

            if (validate != null && !validate(out var error))
                EditorGUILayout.HelpBox(error, MessageType.Error);
        }

        private void AddArrayElement(SerializedProperty listProp, UnityEngine.Object value)
        {
            listProp.serializedObject.Update();
            int index = listProp.arraySize;
            listProp.InsertArrayElementAtIndex(index);
            listProp.GetArrayElementAtIndex(index).objectReferenceValue = value;
            listProp.serializedObject.ApplyModifiedProperties();
            RefreshCampaignErrors(selectedCampaign);
        }

        private void ShowAddExistingMenu<T>(Action<T> onPicked) where T : UnityEngine.Object
        {
            pendingPickerCallback = obj => onPicked(obj as T);
            EditorGUIUtility.ShowObjectPicker<T>(null, false, "", ObjectPickerControlId);
        }

        private void HandleObjectPicker()
        {
            if (EditorGUIUtility.GetObjectPickerControlID() != ObjectPickerControlId) return;

            var evt = Event.current;

            if (evt.type != EventType.ExecuteCommand) return;

            // "ObjectSelectorUpdated" сознательно не обрабатываем: он прилетает на каждый клик
            // по ассету внутри открытого пикера, ещё до подтверждения выбора. Если добавлять
            // элемент на каждое такое событие — при щёлканье по нескольким ассетам добавятся
            // все они. Нужен только финальный выбор, поэтому ждём "ObjectSelectorClosed" и берём
            // итоговый объект оттуда — на этот момент EditorGUIUtility.GetObjectPickerObject()
            // уже возвращает то, что реально было выбрано последним (или null, если отменили).
            if (evt.commandName == "ObjectSelectorClosed")
            {
                var picked = EditorGUIUtility.GetObjectPickerObject();
                if (picked != null) pendingPickerCallback?.Invoke(picked);
                pendingPickerCallback = null;
                evt.Use();
            }
        }

        /// <summary>Меню "Create New <T>" — все конкретные наследники T через TypeCache,
        /// новый ассет создаётся рядом со step-ассетом и передаётся в onCreated.</summary>
        private void ShowCreateAssetMenu<T>(
            Action<T> onCreated) where T : ScriptableObject
        {
            var menu = new GenericMenu();

            var types = TypeCache.GetTypesDerivedFrom<T>()
                .Where(t => !t.IsAbstract)
                .OrderBy(t => t.Name);

            foreach (var type in types)
            {
                var captured = type;

                menu.AddItem(
                    new GUIContent(NiceTypeName(captured)),
                    false,
                    () =>
                    {
                        var instance = CreateAssetWithSavePanel(captured);

                        if (instance != null)
                            onCreated((T)instance);
                    });
            }

            if (menu.GetItemCount() == 0)
            {
                menu.AddDisabledItem(
                    new GUIContent($"No {typeof(T).Name} types found"));
            }

            menu.ShowAsContext();
        }

        /// <summary>Strips the common Definition-class suffix for a readable menu label —
        /// "EnemyKilledObjectiveDefinition" → "EnemyKilled", "AllOfGuidanceConditionDefinition" → "AllOf".</summary>
        private static string NiceTypeName(Type concrete)
        {
            string name = concrete.Name;
            foreach (var suffix in new[] { "ObjectiveDefinition", "GuidanceConditionDefinition", "Definition" })
            {
                if (name.EndsWith(suffix, StringComparison.Ordinal))
                    return name.Substring(0, name.Length - suffix.Length);
            }
            return name;
        }

        private ScriptableObject CreateAssetWithSavePanel(Type type)
        {
            if (assetCreationSettings == null)
            {
                EditorUtility.DisplayDialog(
                    "Missing Asset Creation Settings",
                    "Create and assign TutorialAssetCreationSettings.asset.",
                    "OK");

                return null;
            }

            bool isGuidance = typeof(TutorialGuidanceConditionDefinition)
                .IsAssignableFrom(type);

            bool isObjective = typeof(TutorialObjectiveDefinition)
                .IsAssignableFrom(type);

            string folder;
            string prefix = "";

            if (isGuidance)
            {
                folder = assetCreationSettings.GuidanceFolder;
                //prefix = assetCreationSettings.GuidanceNamePrefix;
            }
            else if (isObjective)
            {
                folder = assetCreationSettings.ObjectiveFolder;
                //prefix = assetCreationSettings.ObjectiveNamePrefix;
            }
            else
            {
                Debug.LogError(
                    $"Unsupported tutorial asset type: {type.Name}");

                return null;
            }

            if (!AssetDatabase.IsValidFolder(folder))
            {
                EditorUtility.DisplayDialog(
                    "Invalid Asset Folder",
                    $"Folder does not exist:\n{folder}",
                    "OK");

                return null;
            }

            string defaultName = $"{prefix}{NiceTypeName(type)}_";

            string path = EditorUtility.SaveFilePanelInProject(
                $"Create {NiceTypeName(type)}",
                defaultName,
                "asset",
                $"Choose a location for the new {NiceTypeName(type)} asset.",
                folder);

            if (string.IsNullOrEmpty(path))
                return null;

            var instance = (ScriptableObject)
                ScriptableObject.CreateInstance(type);

            AssetDatabase.CreateAsset(instance, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorGUIUtility.PingObject(instance);
            Selection.activeObject = instance;

            return instance;
        }

        private string GetStepAssetFolder()
        {
            string stepPath = AssetDatabase.GetAssetPath(selectedStep);
            string folder = string.IsNullOrEmpty(stepPath) ? "Assets" : Path.GetDirectoryName(stepPath);
            return string.IsNullOrEmpty(folder) ? "Assets" : folder;
        }
        
        
        
        
    }
}
