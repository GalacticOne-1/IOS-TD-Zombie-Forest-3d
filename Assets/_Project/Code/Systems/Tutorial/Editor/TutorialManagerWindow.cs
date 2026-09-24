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

            // Разворачиваем список глав прямо под кнопкой выбранной кампании —
            // раньше это был отдельный dropdown в нижнем тулбаре, теперь это часть
            // левой колонки, чтобы кампания/глава/шаг выбирались в одном месте.
            if (isSelected)
                DrawChapterList(campaign);

            GUILayout.Space(2);
        }

        public void SelectCampaign(TutorialDefinition campaign)
        {
            selectedCampaign = campaign;
            selectedChapter = campaign.chapters != null && campaign.chapters.Count > 0 ? campaign.chapters[0] : null;
            SelectStep(selectedChapter != null && selectedChapter.steps.Count > 0 ? selectedChapter.steps[0] : null);
        }

        // ---------------- Chapters (inline, under selected campaign) ----------------

        /// <summary>Вертикальный список глав выбранной кампании, кнопки шириной ~100px
        /// с лейблом "CH_01"/"CH_02"/... по порядковому номеру главы в списке.</summary>
        private void DrawChapterList(TutorialDefinition campaign)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.BeginVertical();

            if (campaign.chapters != null)
            {
                for (int i = 0; i < campaign.chapters.Count; i++)
                {
                    var chapter = campaign.chapters[i];
                    if (chapter == null) continue;
                    DrawChapterRow(chapter, i);
                }
            }

            if (GUILayout.Button("+ New Chapter", GUILayout.Width(100)))
                MenuHelper.CreateNewChapter();

            EditorGUILayout.EndVertical();
            EditorGUI.indentLevel--;
        }

        /// <summary>Одна строка главы: кнопка главы, и если глава выбрана — сразу справа
        /// от неё (горизонтально) разворачивается список шагов этой главы.</summary>
        private void DrawChapterRow(TutorialChapterDefinition chapter, int chapterIndex)
        {
            bool isChapterSelected = chapter == selectedChapter;

            EditorGUILayout.BeginHorizontal();

            if (isChapterSelected) GUI.backgroundColor = Color.cyan;
            string chapterLabel = $"CH_{(chapterIndex + 1):00}";
            if (GUILayout.Button(chapterLabel, GUILayout.Width(60), GUILayout.Height(24)))
            {
                selectedChapter = chapter;
                SelectStep(chapter.steps != null && chapter.steps.Count > 0 ? chapter.steps[0] : null);
            }
            GUI.backgroundColor = originalBg;

            if (isChapterSelected)
                DrawStepButtons(chapter);

            EditorGUILayout.EndHorizontal();
        }

        /// <summary>Квадратные 30x30 кнопки шагов главы, лейбл — просто порядковый индекс
        /// шага (1, 2, 3, ...). Невалидный шаг подсвечивается красным фоном.</summary>
        private void DrawStepButtons(TutorialChapterDefinition chapter)
        {
            if (chapter.steps == null) return;

            var size = 24;

            for (int i = 0; i < chapter.steps.Count; i++)
            {
                var step = chapter.steps[i];
                if (step == null) continue;

                bool isStepSelected = step == selectedStep;
                bool stepValid = step.Validate(out _);

                if (isStepSelected) GUI.backgroundColor = Color.cyan;
                else if (!stepValid) GUI.backgroundColor = ErrorColor;

                if (GUILayout.Button((i + 1).ToString(), GUILayout.Width(size), GUILayout.Height(size)))
                    SelectStep(step);

                GUI.backgroundColor = originalBg;
            }

            if (GUILayout.Button("+", GUILayout.Width(size), GUILayout.Height(size)))
                MenuHelper.CreateNewStep();
        }

        // ================================================================
        // BOTTOM NAV BAR — BREADCRUMB
        // ================================================================
        private void DrawBottomNavBar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            string campaignLabel = selectedCampaign != null
                ? (selectedCampaign.campaignId != null ? selectedCampaign.campaignId.DebugKey : selectedCampaign.name)
                : "— No Campaign —";
            string chapterLabel = selectedChapter != null
                ? (selectedChapter.chapterId != null ? selectedChapter.chapterId.DebugKey : selectedChapter.name)
                : "— No Chapter —";
            string stepLabel = selectedStep != null
                ? (selectedStep.stepId != null ? selectedStep.stepId.DebugKey : selectedStep.name)
                : "— No Step —";

            EditorGUILayout.LabelField($"{campaignLabel}  /  {chapterLabel}  /  {stepLabel}", EditorStyles.miniLabel);

            GUILayout.FlexibleSpace();

            if (selectedStep != null && GUILayout.Button("Ping Asset", EditorStyles.toolbarButton, GUILayout.Width(90)))
                EditorGUIUtility.PingObject(selectedStep);

            EditorGUILayout.EndHorizontal();
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

        // ---------------- Presentation ----------------
        private void DrawPresentationBox()
        {
            presentationFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(presentationFoldout, "🎬 Presentation");
            if (presentationFoldout)
            {
                EditorGUILayout.BeginVertical("box");

                var p = stepSO.FindProperty("presentation");

                // ---- Instruction ----
                EditorGUILayout.LabelField("Instruction", EditorStyles.miniBoldLabel);
                EditorGUILayout.PropertyField(p.FindPropertyRelative("instructionTitleKey"),
                    new GUIContent("Title Key"));
                EditorGUILayout.PropertyField(p.FindPropertyRelative("instructionDesKey"),
                    new GUIContent("Description Key"));

                // ---- Dialogue ----
                EditorGUILayout.Space(4);
                EditorGUILayout.LabelField("Dialogue", EditorStyles.miniBoldLabel);
                EditorGUILayout.HelpBox(
                    "Не обрабатывается — диалоговая система в проекте не найдена. Поле декларативно.",
                    MessageType.None);
                EditorGUILayout.PropertyField(p.FindPropertyRelative("dialogueId"),
                    new GUIContent("Dialogue Id"));

                // ---- Input ----
                EditorGUILayout.Space(4);
                EditorGUILayout.LabelField("Input", EditorStyles.miniBoldLabel);
                EditorGUILayout.PropertyField(p.FindPropertyRelative("inputPolicy"),
                    new GUIContent("Input Policy"));

                // ---- Capabilities ----
                EditorGUILayout.Space(4);
                EditorGUILayout.LabelField("Capabilities", EditorStyles.miniBoldLabel);
                EditorGUILayout.PropertyField(p.FindPropertyRelative("canMove"),
                    new GUIContent("Can Move"));
                EditorGUILayout.PropertyField(p.FindPropertyRelative("canControlCamera"),
                    new GUIContent("Can Control Camera"));
                EditorGUILayout.PropertyField(p.FindPropertyRelative("canInteract"),
                    new GUIContent("Can Interact"));
                EditorGUILayout.PropertyField(p.FindPropertyRelative("canUseAbilities"),
                    new GUIContent("Can Use Abilities"));

                // ---- Camera ----
                EditorGUILayout.Space(4);
                EditorGUILayout.LabelField("Camera", EditorStyles.miniBoldLabel);
                EditorGUILayout.PropertyField(p.FindPropertyRelative("cameraConstraint"),
                    new GUIContent("Camera Constraint"), true);

                EditorGUILayout.HelpBox(
                    "Highlight / Arrow / Camera focus больше не задаются здесь — только через Guidance ниже. " +
                    "Camera Constraint (область камеры) — статичен на уровне шага и не зависит от Guidance.",
                    MessageType.Info);

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
                {
                    EditorGUILayout.HelpBox(
                        "Empty — step has NO highlight/arrow/camera at all. " +
                        "Presentation no longer carries targeting data; this is the only place it lives now.",
                        MessageType.Info);
                }

                for (int i = 0; i < guidanceProp.arraySize; i++)
                {
                    if (DrawGuidanceEntry(guidanceProp, i))
                        break;
                }

                if (GUILayout.Button("+ Add Guidance Entry", GUILayout.Width(180)))
                {
                    guidanceProp.InsertArrayElementAtIndex(guidanceProp.arraySize);

                    // Явно очищаем condition у нового entry.
                    // presentations при этом создаётся сериализованным default-значением.
                    var newEntry = guidanceProp.GetArrayElementAtIndex(guidanceProp.arraySize - 1);
                    var conditionProp = newEntry.FindPropertyRelative("condition");

                    if (conditionProp != null)
                        conditionProp.objectReferenceValue = null;
                }

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        /// <summary>
        /// Returns true if the entry was removed this frame.
        /// </summary>
        private bool DrawGuidanceEntry(SerializedProperty guidanceProp, int index)
        {
            var entryProp = guidanceProp.GetArrayElementAtIndex(index);

            var conditionProp = entryProp.FindPropertyRelative("condition");
            var presentationsProp = entryProp.FindPropertyRelative("presentations");

            var condition =
                conditionProp != null
                    ? conditionProp.objectReferenceValue as TutorialGuidanceConditionDefinition
                    : null;

            EditorGUILayout.Space(10);

            // Получаем rect всего guidance entry.
            Rect entryRect = EditorGUILayout.BeginVertical("box");

            // Жёлтая рамка.
            const float borderWidth = 2f;
            Color borderColor = new Color(1f, 0.75f, 0.1f, 1f);

            EditorGUI.DrawRect(
                new Rect(
                    entryRect.x,
                    entryRect.y,
                    entryRect.width,
                    borderWidth),
                borderColor);

            EditorGUI.DrawRect(
                new Rect(
                    entryRect.x,
                    entryRect.yMax - borderWidth,
                    entryRect.width,
                    borderWidth),
                borderColor);

            EditorGUI.DrawRect(
                new Rect(
                    entryRect.x,
                    entryRect.y,
                    borderWidth,
                    entryRect.height),
                borderColor);

            EditorGUI.DrawRect(
                new Rect(
                    entryRect.xMax - borderWidth,
                    entryRect.y,
                    borderWidth,
                    entryRect.height),
                borderColor);

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField(
                $"Guidance #{index}",
                EditorStyles.boldLabel);

            GUILayout.FlexibleSpace();

            EditorGUILayout.LabelField(
                condition != null
                    ? condition.ConditionTypeId
                    : "Unconditional (always true)",
                EditorStyles.miniBoldLabel,
                GUILayout.Width(180));

            GUI.backgroundColor = Color.red;
            bool removed = GUILayout.Button("x", GUILayout.Width(20));
            GUI.backgroundColor = originalBg;

            EditorGUILayout.EndHorizontal();

            if (removed)
            {
                guidanceProp.DeleteArrayElementAtIndex(index);
                EditorGUILayout.EndVertical();
                return true;
            }

            // ------------------------------------------------------------
            // Condition
            // ------------------------------------------------------------

            EditorGUILayout.Space(2);
            EditorGUILayout.LabelField("Condition", EditorStyles.miniBoldLabel);

            EditorGUILayout.PropertyField(
                conditionProp,
                new GUIContent("Condition (null = always true)"));

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("+ Add Existing", GUILayout.Width(120)))
            {
                ShowAddExistingMenu<TutorialGuidanceConditionDefinition>(picked =>
                {
                    conditionProp.serializedObject.Update();
                    conditionProp.objectReferenceValue = picked;
                    conditionProp.serializedObject.ApplyModifiedProperties();

                    RefreshCampaignErrors(selectedCampaign);
                });
            }

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

            // ------------------------------------------------------------
            // Presentations
            // ------------------------------------------------------------

            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField(
                "Presentations (all shown simultaneously)",
                EditorStyles.miniBoldLabel);

            if (presentationsProp == null)
            {
                EditorGUILayout.HelpBox(
                    "Serialized field 'presentations' was not found.",
                    MessageType.Error);

                DrawGuidanceDescriptionPanel(entryProp);

                EditorGUILayout.EndVertical();
                return false;
            }

            if (presentationsProp.arraySize == 0)
            {
                EditorGUILayout.HelpBox(
                    "No presentation targets. This guidance entry is invalid.",
                    MessageType.Warning);
            }

            for (int i = 0; i < presentationsProp.arraySize; i++)
            {
                if (DrawGuidancePresentationEntry(presentationsProp, i))
                    break;
            }

            if (GUILayout.Button("+ Add Presentation", GUILayout.Width(150)))
            {
                int newIndex = presentationsProp.arraySize;

                presentationsProp.InsertArrayElementAtIndex(newIndex);

                var newPresentation =
                    presentationsProp.GetArrayElementAtIndex(newIndex);

                // ВАЖНО:
                // InsertArrayElementAtIndex копирует предыдущий element.
                // Поэтому очищаем все значения нового TutorialGuidanceTargetDefinition.
                var highlightTargetProp =
                    newPresentation.FindPropertyRelative("highlightTarget");

                var arrowTargetIdProp =
                    newPresentation.FindPropertyRelative("arrowTargetId");

                var cameraFocusTargetIdProp =
                    newPresentation.FindPropertyRelative("cameraFocusTargetId");

                if (highlightTargetProp != null)
                    highlightTargetProp.managedReferenceValue = null;

                if (arrowTargetIdProp != null)
                    arrowTargetIdProp.objectReferenceValue = null;

                if (cameraFocusTargetIdProp != null)
                    cameraFocusTargetIdProp.objectReferenceValue = null;

                presentationsProp.serializedObject.ApplyModifiedProperties();
            }

            // ------------------------------------------------------------
            // Description Panel
            // ------------------------------------------------------------

            DrawGuidanceDescriptionPanel(entryProp);

            EditorGUILayout.EndVertical();

            return false;
        }

        /// <summary>
        /// Draws one TutorialGuidanceTargetDefinition.
        /// Returns true when the element was removed this frame.
        /// </summary>
        private bool DrawGuidancePresentationEntry(
            SerializedProperty presentationsProp,
            int index)
        {
            var presentationProp =
                presentationsProp.GetArrayElementAtIndex(index);

            var highlightTargetProp =
                presentationProp.FindPropertyRelative("highlightTarget");

            var arrowTargetIdProp =
                presentationProp.FindPropertyRelative("arrowTargetId");

            var cameraFocusTargetIdProp =
                presentationProp.FindPropertyRelative("cameraFocusTargetId");

            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField(
                $"Target #{index}",
                EditorStyles.miniBoldLabel);

            GUILayout.FlexibleSpace();

            GUI.backgroundColor = Color.red;
            bool removed = GUILayout.Button("x", GUILayout.Width(20));
            GUI.backgroundColor = originalBg;

            EditorGUILayout.EndHorizontal();

            if (removed)
            {
                presentationsProp.DeleteArrayElementAtIndex(index);
                EditorGUILayout.EndVertical();
                return true;
            }

            // ------------------------------------------------------------
            // Highlight
            // ------------------------------------------------------------

            EditorGUILayout.PropertyField(
                highlightTargetProp,
                new GUIContent("Highlight Target"));

            // ------------------------------------------------------------
            // Arrow
            // ------------------------------------------------------------

            EditorGUILayout.PropertyField(
                arrowTargetIdProp,
                new GUIContent("Arrow Target"));

            // ------------------------------------------------------------
            // Camera
            // ------------------------------------------------------------

            EditorGUILayout.PropertyField(
                cameraFocusTargetIdProp,
                new GUIContent("Camera Focus Target"));

            EditorGUILayout.EndVertical();

            return false;
        }

        /// <summary>Overlay-панель с текстом для этого guidance-entry — параллельный
        /// highlight/arrow/camera канал, использует тот же condition (см.
        /// TutorialGuidancePanelDefinition докстринг). enabled=false по умолчанию —
        /// текстовые поля скрыты, пока панель явно не включена, чтобы не загромождать
        /// UI для большинства entries, которые её не используют.</summary>
        private void DrawGuidanceDescriptionPanel(SerializedProperty entryProp)
        {
            var panelProp = entryProp.FindPropertyRelative("descriptionPanel");
            if (panelProp == null) return;

            var enabledProp = panelProp.FindPropertyRelative("enabled");

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Description Panel (overlay text, same condition as above)",
                EditorStyles.miniBoldLabel);
            EditorGUILayout.PropertyField(enabledProp, new GUIContent("Enabled"));

            if (enabledProp.boolValue)
            {
                EditorGUILayout.PropertyField(panelProp.FindPropertyRelative("text"), new GUIContent("Text"));
                EditorGUILayout.PropertyField(panelProp.FindPropertyRelative("oneShot"), new GUIContent("One Shot"));
            }
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