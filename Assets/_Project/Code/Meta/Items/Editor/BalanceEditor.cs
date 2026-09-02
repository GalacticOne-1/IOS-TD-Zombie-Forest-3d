using System.Collections.Generic;
using Galactic1.Core.Enums;
using Galactic1.Game.Meta.Items;
using UnityEditor;
using UnityEngine;

namespace Galactic1.Tools
{
    public class BalanceEditor
    {
        private ItemManagerWindow itemManager;
        private BalanceEditorFasilities _balanceFasilities;

        private Vector2 centerScroll;
        private Vector2 leftListScroll;
        private string searchQuery = "";

        private List<ScriptableObject> allBalances = new();
        private List<ItemConfig> allItems = new();

        private bool pendingScroll;
        private float targetScrollY;

        private bool editMode;

        private bool foldFire = true;
        private bool foldSpread;
        private bool foldHeat;
        private bool foldSuppression;
        private bool foldTracers;
        
        
        
        
        public BalanceEditor(ItemManagerWindow itemManager)
        {
            this.itemManager = itemManager;
            _balanceFasilities = new(itemManager);
        }
        
        
        void PropertyField(SerializedProperty property, string label)
        {
            EditorGUILayout.PropertyField(property, new GUIContent(label));
        }
        void PropertyField(SerializedProperty property, string target, string label)
        {
            EditorGUILayout.PropertyField(property.FindPropertyRelative(target), new GUIContent(label));
        }
        
        
        
        
        

        public void DrawPanel()
        {
            //RefreshList();
            
            EditorGUILayout.BeginHorizontal();


            // 🔹 Центральная панель — информация и данные
            EditorGUILayout.BeginVertical(GUILayout.Width(600));
            DrawCenterBox(); // 🔹 NEW
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
            GUILayout.Space(20);
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginVertical("box", GUILayout.Width(200));
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(70)))
                RefreshList();

            GUILayout.Space(10);
            var newCategory = (ItemCategoryEditor)EditorGUILayout.EnumPopup(itemManager.selectedCategoryEditor);
            // itemManager.SelectCategory(newCategory, () =>
            // {
            //     itemManager.selectedCategory = newCategory;
            //     itemManager.selectedItem = null;
            // });

            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);
            searchQuery = GUILayout.TextField(searchQuery, EditorStyles.toolbarSearchField, GUILayout.Width(200));
            EditorGUILayout.EndVertical();
        }


        
        

        // 🔹 NEW: Центральный бокс
        private void DrawCenterBox()
        {
            if (itemManager.SelectedItem == null)
            {
                EditorGUILayout.LabelField("Выберите предмет слева", EditorStyles.centeredGreyMiniLabel);
                return;
            }

            centerScroll = EditorGUILayout.BeginScrollView(centerScroll);
            EditorGUILayout.BeginVertical();

            // 🔹 1. Верхний блок — иконка + имя
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();

            if (itemManager.SelectedItem.Header.icon != null)
                EditorUtils.DrawSprite(itemManager.SelectedItem.Header.icon, 30f);
            else
            {
                var rect = GUILayoutUtility.GetRect(30, 30,
                    GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));
                EditorGUI.DrawRect(rect, Color.black);
            }

            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField(itemManager.SelectedItem.Header.titleLid, EditorStyles.boldLabel);
            EditorGUILayout.SelectableLabel(itemManager.SelectedItem.name, 
                EditorStyles.miniLabel, GUILayout.Height(18));
            EditorGUILayout.EndVertical();
            
            // 🔹 включить/отключить редактирование
            editMode = EditorGUILayout.ToggleLeft("✏️ Edit Mode", editMode, GUILayout.Width(120));

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            
            
            
            EditorGUILayout.BeginHorizontal();
            // 🔹 2. Нижний блок — поля WeaponBaseStats (если это WeaponConfig)
            EditorGUI.BeginDisabledGroup(!editMode); // 🔹 Если editMode выключен — поля неактивны

            Basic(itemManager.SelectedItem, out bool configExist);
            
            EditorGUI.EndDisabledGroup();   // 🔹 Если editMode выключен — поля неактивны

            if (!configExist)
            {
                EditorGUILayout.Space(10);
                EditorGUILayout.HelpBox("Этот предмет не имеет реализации.", MessageType.Info);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
        }


        #region Balance Result


        void Basic(ItemConfig item, out bool configExist)
        {
            configExist = true;

            if (itemManager.CurrentSO == null) return;
            
            // Левая колонка — CONFIG
            EditorGUILayout.BeginVertical("box"); // GUILayout.Width(280)
            if (item)
            {
                itemManager.CurrentSO.Update();
                
                EditorGUILayout.LabelField("CONFIG", EditorLabelStyles.Yellow);
                EditorGUILayout.Space(5);
                
                SerializedProperty headerData = itemManager.CurrentSO.FindProperty("header");
                SerializedProperty prefabName = itemManager.CurrentSO.FindProperty("prefabName");
                SerializedProperty prefabPath = itemManager.CurrentSO.FindProperty("prefabPath");
                //SerializedProperty ghostPrefabPath = itemManager.CurrentSO.FindProperty("ghostPrefabPath");
                
                //SerializedObject soConfig = new SerializedObject(configBase);
                //SerializedProperty recruitAccessData = soConfig.FindProperty("recruitAccess");
                //SerializedProperty equipSettingsData = soConfig.FindProperty("equipSettings");
                //SerializedProperty basicSettingsData = soConfig.FindProperty("basicSettings");
                
                EditorGUI.BeginChangeCheck();
                
                PropertyField(itemManager.CurrentSO.FindProperty("isEnabled"), "Is Enabled");
                
                EditorGUILayout.Space(10);
                PropertyField(headerData, "titleLid", "Title");
                PropertyField(headerData, "descriptionLid", "Description");
                PropertyField(headerData, "order", "Order");
                PropertyField(headerData, "icon", "Icon");
                PropertyField(headerData, "sizeUI", "Size UI");
                PropertyField(headerData, "iconOffset", "Icon Offset");
                
                
                EditorGUILayout.Space(10);
                PropertyField(prefabName, "Prefab Name");
                PropertyField(prefabPath, "Prefab Path");
                //PropertyField(ghostPrefabPath, "Ghost Prefab Path");
                
                // EditorGUILayout.Space(10);
                // EditorGUILayout.LabelField("Recruit Access", EditorStyles.boldLabel);
                // PropertyField(recruitAccessData, "tier", "Tier");
                // PropertyField(recruitAccessData, "weight", "Weight");
                // PropertyField(recruitAccessData, "allowedCategories", "Allowed Categories");
                
                Color oldColor = GUI.backgroundColor;
                GUI.backgroundColor = new Color(1f, 0.8f, 0f, 0.6f);
                
                EditorGUILayout.BeginVertical("box");
            
                GUI.backgroundColor = oldColor; // вернуть, чтобы поля не стали синими
                
                
                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField("Classification", EditorLabelStyles.Yellow);

                var classificationProp= itemManager.CurrentSO.FindProperty("classification");
                PropertyField(classificationProp, "category",  "Category");
                PropertyField(classificationProp, "economyCategory",  "Economy Category");
                PropertyField(classificationProp, "tier", "Tier");
                PropertyField(classificationProp, "itemLabel",  "Item Label");
                PropertyField(classificationProp, "rarity",  "Rarity");
                PropertyField(classificationProp, "maxStack",  "Max Stack");
                PropertyField(classificationProp, "sortCategory",  "Sort Category");
                PropertyField(classificationProp, "sortPriority",  "Sort Priority");
                
                EditorGUILayout.Space(10);
                var s = "Отмечать флаги";
                s += "\nНапример Что бы конфиг не отображался в виджете: ItemFlags.HideInConstruct";
                EditorGUILayout.HelpBox(s, MessageType.Info);
                PropertyField(classificationProp, "flag",  "Item Flags");
                
                
                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField("Physical", EditorLabelStyles.Yellow);
                var physicalProp= itemManager.CurrentSO.FindProperty("physical");
                PropertyField(physicalProp, "weight",  "Weight");
                PropertyField(physicalProp, "volume",  "Volume");
                PropertyField(physicalProp, "usesDurability",  "Uses Durability");
                PropertyField(physicalProp, "maxDurability",  "Max Durability");
                PropertyField(physicalProp, "durabilityLossType",  "Durability Loss Type");
                PropertyField(physicalProp, "canBeRepaired",  "Can Be Repaired");


                EditorGUILayout.Space(10);
                s = "Дополнительные характеристики, их может быть несколько";
                s += "\nОтвечает на вопрос какие свойства?";
                s += "\nИспользуется для совместимости обвесов, фильтрации хранилищ, условий рецептов.";
                s += "\nAssaultRifle, Auto  \u2190 несколько тегов";
                EditorGUILayout.HelpBox(s, MessageType.Info);
                PropertyField(itemManager.CurrentSO.FindProperty("tags"),  "Tags");

                
                
                // basicSettings
                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField("Basic Settings", EditorLabelStyles.Yellow);
                PropertyField(itemManager.CurrentSO.FindProperty("isCraftable"),  "Is Craftable");
                
                EditorGUILayout.Space(10);
                EditorGUILayout.EndVertical();
                
                
                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField("=== MODULE ===", EditorLabelStyles.Yellow);
                
                // === каждый тип рисует конкретно свои поля 
                
                if(item.HasModule<LootModule>()) Loot(item.LootModule);
                if(item.HasModule<ResourceModule>()) Resource(item.Resource);
                if(item.HasModule<UseModule>()) Consumable(item.Use);
                if(item.HasModule<WeaponModule>()) Weapon(item.Weapon);
                if(item.HasModule<EquipmentModule>()) Armor(item.Equipment);
                if(item.HasModule<AmmoModule>()) Ammo(item.Ammo);
                if(item.HasModule<UpgradeModule>()) Upgrade(item.Upgrade);
                
                if(item.HasModule<BlueprintModule>()) Blueprint(item.Blueprint);
                
                if(item.HasModule<CraftingStationModule>()) _balanceFasilities.StationMain(item.CraftStation);
                else if(item.HasModule<StorageModule>()) _balanceFasilities.StorageMain(item.Storage);
                else if(item.HasModule<LivingModule>()) _balanceFasilities.LivingModuleMain(item.Living);
                else if(item.HasModule<TavernModule>()) _balanceFasilities.TavernMain(item.Tavern);
                
                // === defense facilities
                if (item.HasModule<BuildingHealthModule>())
                    _balanceFasilities.BuildingHealthMain(item.BuildingHealth);
                if (item.HasModule<BuildingAttackModule>())
                    _balanceFasilities.BuildingAttackMain(item.BuildingAttack);
                if (item.HasModule<BuildingPassiveDamageModule>())
                    _balanceFasilities.BuildingPassiveDamageMain(item.BuildingPassiveDamage);
                
                

                if (EditorGUI.EndChangeCheck())
                {
                    itemManager.CurrentSO.ApplyModifiedProperties();
                    EditorUtility.SetDirty(item);
                }
            }

            EditorGUILayout.EndVertical();
        }
        


        void Weapon(WeaponModule module)
        {
            if (module != null)
            {
                SerializedProperty modules = itemManager.CurrentSO.FindProperty("modules");

                for (int i = 0; i < modules.arraySize; i++)
                {
                    var element = modules.GetArrayElementAtIndex(i);

                    if (ReferenceEquals(element.managedReferenceValue, module))
                    {
                        EditorGUILayout.Space(10);
                        
                        var s = "Статы влияющие на сам юнит";
                        s += "\nНапример: скорость движения или броня и тд. У оружия есть Definition !!!";
                        EditorGUILayout.HelpBox(s, MessageType.Info);
                        var baseStats = element.FindPropertyRelative("baseStats");
                        DrawBaseStatsList(baseStats);
                        
                        EditorGUILayout.Space(10);
                        EditorGUILayout.HelpBox("Единственное место для настройки оружия", MessageType.Info);
                        EditorGUILayout.LabelField("Definition", EditorLabelStyles.Yellow);
                        PropertyField(element.FindPropertyRelative("definition"), "Definition");
                        
                        
                        // Definition inline editor
                        var def = module.Definition;
                        if (def != null)
                        {
                            var defSO = new SerializedObject(def);
                            defSO.Update();

                            EditorGUI.BeginChangeCheck();

                            foldFire = EditorGUILayout.Foldout(foldFire, "Fire & Damage", true);
                            if (foldFire)
                            {
                                EditorGUILayout.BeginVertical("box");
                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.LabelField($"DPS: {module.DPS:0.0}", 
                                    EditorStyles.miniLabel,
                                    GUILayout.Width(70));
                                var spread_text = $"{module.Definition.baseSpreadDeg:0.0}";
                                spread_text += $" - {module.Definition.movingSpreadMul:0.0}";
                                spread_text += $" - {module.Definition.stressSpreadMul:0.0}";
                                EditorGUILayout.LabelField($"Spread: {spread_text}",
                                    EditorStyles.miniLabel,
                                    GUILayout.Width(150));
                                EditorGUILayout.EndHorizontal();
                                
                                EditorGUILayout.PropertyField(defSO.FindProperty("fireMode"));
                                EditorGUILayout.PropertyField(defSO.FindProperty("fireType"));
                                EditorGUILayout.PropertyField(defSO.FindProperty("ammoType"));
                                EditorGUILayout.PropertyField(defSO.FindProperty("supportedAmmo"));
                                EditorGUILayout.PropertyField(defSO.FindProperty("damage"));
                                EditorGUILayout.PropertyField(defSO.FindProperty("damageVariance"));
                                EditorGUILayout.PropertyField(defSO.FindProperty("armorPiercing"));
                                EditorGUILayout.PropertyField(defSO.FindProperty("range"));
                                EditorGUILayout.PropertyField(defSO.FindProperty("roundsPerMinute"));
                                EditorGUILayout.PropertyField(defSO.FindProperty("burstCount"));
                                EditorGUILayout.PropertyField(defSO.FindProperty("burstPauseSec"));
                                EditorGUILayout.PropertyField(defSO.FindProperty("projectilesPerShot"));
                                EditorGUILayout.PropertyField(defSO.FindProperty("magazineSize"));
                                EditorGUILayout.PropertyField(defSO.FindProperty("reloadTimeSec"));
                                EditorGUILayout.EndVertical();
                            }

                            foldSpread = EditorGUILayout.Foldout(foldSpread, "Spread & Range", true);
                            if (foldSpread)
                            {
                                EditorGUILayout.BeginVertical("box");
                                //EditorGUILayout.PropertyField(defSO.FindProperty("effectiveRange"));
                                //EditorGUILayout.PropertyField(defSO.FindProperty("maxRange"));
                                //EditorGUILayout.PropertyField(defSO.FindProperty("maxRangeSpreadPenalty"));
                                EditorGUILayout.PropertyField(defSO.FindProperty("baseSpreadDeg"));
                                EditorGUILayout.PropertyField(defSO.FindProperty("movingSpreadMul"));
                                EditorGUILayout.PropertyField(defSO.FindProperty("stressSpreadMul"));
                                EditorGUILayout.PropertyField(defSO.FindProperty("minDamageMultiplierAtMaxRange"));
                                EditorGUILayout.EndVertical();
                            }

                            foldHeat = EditorGUILayout.Foldout(foldHeat, "Heat", true);
                            if (foldHeat)
                            {
                                EditorGUILayout.BeginVertical("box");
                                EditorGUILayout.PropertyField(defSO.FindProperty("hasHeat"));
                                if (def.hasHeat)
                                {
                                    EditorGUILayout.PropertyField(defSO.FindProperty("heatPerShot"));
                                    EditorGUILayout.PropertyField(defSO.FindProperty("heatCoolRate"));
                                    EditorGUILayout.PropertyField(defSO.FindProperty("overheatThreshold"));
                                    EditorGUILayout.PropertyField(defSO.FindProperty("cooldownSec"));
                                }

                                EditorGUILayout.EndVertical();
                            }

                            foldSuppression = EditorGUILayout.Foldout(foldSuppression, "Suppression", true);
                            if (foldSuppression)
                            {
                                EditorGUILayout.BeginVertical("box");
                                EditorGUILayout.PropertyField(defSO.FindProperty("hasSuppression"));
                                if (def.hasSuppression)
                                {
                                    EditorGUILayout.PropertyField(defSO.FindProperty("suppressionAngle"));
                                    EditorGUILayout.PropertyField(defSO.FindProperty("suppressionRange"));
                                }

                                EditorGUILayout.EndVertical();
                            }

                            foldTracers = EditorGUILayout.Foldout(foldTracers, "Tracers", true);
                            if (foldTracers)
                            {
                                EditorGUILayout.BeginVertical("box");
                                EditorGUILayout.PropertyField(defSO.FindProperty("tracerEveryNthShot"));
                                EditorGUILayout.PropertyField(defSO.FindProperty("tracerPelletFraction"));
                                EditorGUILayout.EndVertical();
                            }

                            if (EditorGUI.EndChangeCheck())
                            {
                                defSO.ApplyModifiedProperties();
                                EditorUtility.SetDirty(def);
                            }
                        }

                        EditorGUILayout.Space(10);
                        
                        var equipRestrictions = element.FindPropertyRelative("equipRestrictions");
                        DrawEquipRestrictions(equipRestrictions);
                        
                        
                        EditorGUILayout.Space(10);
                        var infoProp = element.FindPropertyRelative("info");
                        EditorGUILayout.LabelField("Weapon Info", EditorLabelStyles.Yellow);
                        PropertyField(infoProp, "weaponType", "Weapon Type");
                        PropertyField(infoProp, "weaponSystem", "Weapon System");
                        PropertyField(infoProp, "ammoType", "Ammo Type");
                        PropertyField(infoProp, "isTwoHanded", "Is Two Handed");
                        PropertyField(infoProp, "canBeModified", "Can Be Modified");
                        
                        break;
                    }
                }

                

                RecruitAccessBox(itemManager.CurrentSO);
                
            }
            else
            {
                EditorGUILayout.Space(10);
                EditorGUILayout.HelpBox("Нет модуля WeaponModule.", MessageType.Info);
            }
        }

        

        void Armor(EquipmentModule module)
        {
            // Левая колонка — CONFIG
            if (module != null)
            {
                SerializedProperty modules = itemManager.CurrentSO.FindProperty("modules");

                for (int i = 0; i < modules.arraySize; i++)
                {
                    var element = modules.GetArrayElementAtIndex(i);

                    if (ReferenceEquals(element.managedReferenceValue, module))
                    {
                        EditorGUILayout.Space(10);
                        
                        var baseStats = element.FindPropertyRelative("baseStats");
                        DrawBaseStatsList(baseStats);
                        
                        EditorGUILayout.Space(10);
                        EditorGUILayout.LabelField("Equipment Module", EditorLabelStyles.Yellow);
                        
                        var equipRestrictions = element.FindPropertyRelative("equipRestrictions");
                        DrawEquipRestrictions(equipRestrictions);
                        break;
                    }
                }
                
                RecruitAccessBox(itemManager.CurrentSO);

            }
            else
            {
                EditorGUILayout.Space(10);
                EditorGUILayout.HelpBox("Нет модуля EquipmentModule.", MessageType.Info);
            }

        }
        
        
        void Ammo(AmmoModule module)
        {
            // Левая колонка — CONFIG
            if (module != null)
            {
                SerializedProperty modules = itemManager.CurrentSO.FindProperty("modules");

                for (int i = 0; i < modules.arraySize; i++)
                {
                    var element = modules.GetArrayElementAtIndex(i);

                    if (ReferenceEquals(element.managedReferenceValue, module))
                    {
                        EditorGUILayout.Space(10);
                        EditorGUILayout.LabelField("Ammo Module", EditorLabelStyles.Yellow);
                        
                        var s = "Разные типы патронов одного калибра связываются с оружием через Ammo Type";
                        s += "\nAmmo Type - тег(калибр) для связи с оружием";
                        s += "\nAmount - кол-во выстрелов";
                        EditorGUILayout.HelpBox(s, MessageType.Info);
                        PropertyField(element.FindPropertyRelative("ammoType"), "Ammo Type");
                        //PropertyField(element.FindPropertyRelative("amount"), "Amount");
                        
                        s = "Обязательно установть приоритет расхода этих патронов";
                        s += "\nОружие по время перезарядки ищет патроны начиная с 0 приоритета";
                        s += "\n(0 = обычные, 1 = бронебойные, и т.д.)";
                        EditorGUILayout.HelpBox(s, MessageType.Info);
                        PropertyField(element.FindPropertyRelative("priority"), "Priority");
                        break;
                    }
                }

            }
            else
            {
                EditorGUILayout.Space(10);
                EditorGUILayout.HelpBox("Нет модуля AmmoModule.", MessageType.Info);
            }

        }
        
        void Consumable(UseModule module)
        {
            // Левая колонка — CONFIG
            if (module != null)
            {
                SerializedProperty modules = itemManager.CurrentSO.FindProperty("modules");

                for (int i = 0; i < modules.arraySize; i++)
                {
                    var element = modules.GetArrayElementAtIndex(i);

                    if (ReferenceEquals(element.managedReferenceValue, module))
                    {
                        EditorGUILayout.Space(10);
                        EditorGUILayout.LabelField("Consumable Module", EditorLabelStyles.Yellow);
                        
                        PropertyField(element.FindPropertyRelative("consumeOnUse"), "Consume On Use");
                        //PropertyField(element.FindPropertyRelative("cooldown"), "Cooldown");
                        //PropertyField(element.FindPropertyRelative("effects"), "Effects");
                        PropertyField(element.FindPropertyRelative("slotType"), "Slot Type");
                        
                        EditorGUILayout.HelpBox("В первую очередь радиус для броска гранат", MessageType.Info);
                        PropertyField(element.FindPropertyRelative("range"), "Use Range");
                        
                        EditorGUILayout.Space(10);
                        
                        var behaviourProp = element.FindPropertyRelative("behaviour");
                        ConsumableBehaviourDrawer.Draw(behaviourProp);
                        
                        break;
                    }
                }

            }
            else
            {
                EditorGUILayout.Space(10);
                EditorGUILayout.HelpBox("Нет модуля UseModule.", MessageType.Info);
            }

        }
        
        void Upgrade(UpgradeModule module)
        {
            // Левая колонка — CONFIG
            if (module != null)
            {
                SerializedProperty modules = itemManager.CurrentSO.FindProperty("modules");

                for (int i = 0; i < modules.arraySize; i++)
                {
                    var element = modules.GetArrayElementAtIndex(i);

                    if (ReferenceEquals(element.managedReferenceValue, module))
                    {
                        EditorGUILayout.Space(10);
                        
                        EditorGUILayout.HelpBox("Куда создается предмет", MessageType.Info);
                        PropertyField(element.FindPropertyRelative("slotType"), "Slot Type");

                        EditorGUILayout.Space(5);
                        var s = "Добавить теги с какими предметами взаимодействовать";
                        s += "\nПример: 1.weapon 2.rifle 3.energy";
                        EditorGUILayout.HelpBox(s, MessageType.Info);
                        PropertyField(element.FindPropertyRelative("compatibleTags"), "Compatible Tags");
                        
                        EditorGUILayout.Space(5);
                        PropertyField(element.FindPropertyRelative("modifiers"), "Modifiers");
                        
                        break;
                    }
                }

            }
            else
            {
                EditorGUILayout.Space(10);
                EditorGUILayout.HelpBox("Нет модуля UpgradeModule.", MessageType.Info);
            }

        }
        
        void Blueprint(BlueprintModule module)
        {
            // Левая колонка — CONFIG
            if (module != null)
            {
                SerializedProperty modules = itemManager.CurrentSO.FindProperty("modules");

                for (int i = 0; i < modules.arraySize; i++)
                {
                    var element = modules.GetArrayElementAtIndex(i);

                    if (ReferenceEquals(element.managedReferenceValue, module))
                    {
                        
                        break;
                    }
                }

            }
            else
            {
                EditorGUILayout.Space(10);
                EditorGUILayout.HelpBox("Нет модуля BlueprintModule.", MessageType.Info);
            }

        }
        
        void Loot(LootModule module)
        {
            // Левая колонка — CONFIG
            if (module != null)
            {
                SerializedProperty modules = itemManager.CurrentSO.FindProperty("modules");

                for (int i = 0; i < modules.arraySize; i++)
                {
                    var element = modules.GetArrayElementAtIndex(i);

                    if (ReferenceEquals(element.managedReferenceValue, module))
                    {
                        EditorGUILayout.Space(10);
                        EditorGUILayout.LabelField("Loot Module", EditorLabelStyles.Yellow);
                        
                        PropertyField(element.FindPropertyRelative("lootCost"), "Loot Cost");
                        PropertyField(element.FindPropertyRelative("dropTag"), "Drop Tag");
                        
                        break;
                    }
                }

            }
            else
            {
                EditorGUILayout.Space(10);
                EditorGUILayout.HelpBox("Нет модуля LootModule.", MessageType.Info);
            }
            
        }
        
        
        
        void Resource(ResourceModule module)
        {
            // Левая колонка — CONFIG
            if (module != null)
            {   

               // SerializedProperty recruitAccessData = so.FindProperty("recruitAccess");
                
                // todo ...

            }
            else
            {
                EditorGUILayout.Space(10);
                EditorGUILayout.HelpBox("Нет модуля ResourceModule.", MessageType.Info);
            }

        }
        
        
        

        

        #endregion
        
        
        
        


        void RecruitAccessBox(SerializedObject so)
        {
            SerializedProperty recruitAccessData = so.FindProperty("recruitAccess");
            
            EditorGUILayout.Space(10);
            
            Color oldColor = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0f, 0.33f, 1f, 1f);
            
            EditorGUILayout.BeginVertical("box");
            
            GUI.backgroundColor = oldColor; // вернуть, чтобы поля не стали синими
            
            EditorGUILayout.LabelField("Recruit Access (Tavern)", EditorLabelStyles.Yellow);
            
            EditorGUILayout.Space(10);
            var s = "Доступ предмета для выпадения в таверне с новым выжившим";
            s += "\nTier - (Tier 1 самый слабый) (Tier 2 средний) (Tier 3 сильный) (Tier 4 редкий)";
            s += "\nWeight - вероятность выпадения предмета внутри одного тира (10 - 100)";
            s += "\nCategories - какие выжившие могут получить предмет";
            EditorGUILayout.HelpBox(s, MessageType.Info);
            
            PropertyField(recruitAccessData, "tier", "Tier");
            PropertyField(recruitAccessData, "weight", "Weight [0-100]");
            PropertyField(recruitAccessData, "allowedCategories", "Allowed Categories");
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawBaseStatsList(SerializedProperty listProp)
        {
            EditorGUILayout.BeginHorizontal("box");
            EditorGUILayout.LabelField("Base Stats", EditorLabelStyles.Yellow);
            
            if (GUILayout.Button("+", GUILayout.Width(20), GUILayout.Height(20)))
            {
                int index = listProp.arraySize;
                listProp.InsertArrayElementAtIndex(index);

                var element = listProp.GetArrayElementAtIndex(index);

                var statId = element.FindPropertyRelative("StatId");
                var value = element.FindPropertyRelative("Value");

                if (statId != null) statId.enumValueIndex = 0;
                if (value != null) value.floatValue = 0f;
            }
            
            EditorGUILayout.EndHorizontal();
            EditorGUI.indentLevel++;
            
            if (listProp == null)
            {
                EditorGUILayout.HelpBox("baseStats property not found", MessageType.Error);
                return;
            }

            for (int i = 0; i < listProp.arraySize; i++)
            {
                SerializedProperty statProp = listProp.GetArrayElementAtIndex(i);
                
                var statIdProp = statProp.FindPropertyRelative("StatId");
                var valueProp = statProp.FindPropertyRelative("Value");

                string statName = statIdProp != null
                    ? $"{(StatId)statIdProp.intValue} > {valueProp.floatValue}"
                    : $"Stat {i+1}";

                statProp.isExpanded = EditorGUILayout.Foldout(statProp.isExpanded, statName, true);

                if (statProp.isExpanded)
                {
                    EditorGUI.indentLevel++;
                    
                    

                    if (statIdProp == null || valueProp == null)
                    {
                        EditorGUILayout.HelpBox("Invalid ItemStatEntry layout", MessageType.Error);
                        continue;
                    }

                    EditorGUILayout.BeginHorizontal("box");
                    
                    EditorGUILayout.PropertyField(statIdProp, new GUIContent(), GUILayout.Width(150));
                    EditorGUILayout.PropertyField(valueProp, new GUIContent(), GUILayout.Width(60));
                    
                    GUILayout.FlexibleSpace();   // 👈 толкает кнопку вправо
                    GUI.backgroundColor = Color.red;
                    if (GUILayout.Button("x", GUILayout.Width(20)))
                    {
                        listProp.DeleteArrayElementAtIndex(i);
                        break;
                    }
                    GUI.backgroundColor = Color.white;
                    
                    EditorGUILayout.EndHorizontal();

                    EditorGUI.indentLevel--;
                }
            }

            

            EditorGUI.indentLevel--;
        }

        
        // только для снаряжения (требования к юниту)
        void DrawEquipRestrictions(SerializedProperty prop)
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Equip Restrictions", EditorLabelStyles.Yellow);

            EditorGUILayout.PropertyField(prop.FindPropertyRelative("requiredLevel"), new GUIContent("Required Level"));
            EditorGUILayout.PropertyField(prop.FindPropertyRelative("allowedUnits"), new GUIContent("Allowed Units"));

            EditorGUILayout.EndVertical();
        }
        
        

        

        public void RefreshList()
        {
            allBalances.Clear();
            allItems.Clear();

            string[] guids = AssetDatabase.FindAssets("t:ScriptableObject");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var obj = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);

                //if (obj is WeaponBalance || obj is ArmorBalance || obj is ConsumableBalance)
                    //allBalances.Add(obj);

                if (obj is ItemConfig item)
                    allItems.Add(item);
            }

            itemManager.Repaint();
        }

    }
}