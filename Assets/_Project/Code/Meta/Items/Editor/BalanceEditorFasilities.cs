
using Galactic1.Game.Meta.Items;
using UnityEditor;
using UnityEngine;

namespace Galactic1.Tools
{
    public class BalanceEditorFasilities
    {
        private ItemManagerWindow itemManager;

        public BalanceEditorFasilities(ItemManagerWindow itemManager)
        {
            this.itemManager = itemManager;
        }




        void PropertyField(SerializedProperty property, string label)
        {
            EditorGUILayout.PropertyField(property, new GUIContent(label));
        }

        void PropertyField(SerializedProperty property, string target, string label)
        {
            EditorGUILayout.PropertyField(property.FindPropertyRelative(target), new GUIContent(label));
        }



        void Main(FacilityModule facility)
        {
            if (facility != null)
            {
                SerializedObject so = new SerializedObject(facility.Item);
                SerializedProperty structureData = so.FindProperty("structureData");

                // PropertyField(structureData,"type", "Facility Type");
                // PropertyField(structureData,"buildTime", "Build Time");
                // PropertyField(structureData,"canBeRotated", "Can Be Rotated");
                // PropertyField(structureData,"maxHealth", "Max Health");
                // PropertyField(structureData,"damageResistance", "Damage Resistance");


            }
            else
            {
                EditorGUILayout.Space(10);
                EditorGUILayout.HelpBox("Нет ассета FacilityConfig.", MessageType.Info);
            }
        }





        public void StationMain(CraftingStationModule station)
        {
            // Левая колонка — CONFIG
            if (station != null)
            {
                Main(station);


                SerializedObject so = new SerializedObject(station.Item);


                // todo
                // ...

            }
            else
            {
                EditorGUILayout.Space(10);
                EditorGUILayout.HelpBox("Нет ассета ProductionStationConfig.", MessageType.Info);
            }

        }


        public void StorageMain(StorageModule storage)
        {
            if (storage != null)
            {
                SerializedProperty modules = itemManager.CurrentSO.FindProperty("modules");

                for (int i = 0; i < modules.arraySize; i++)
                {
                    var element = modules.GetArrayElementAtIndex(i);

                    if (ReferenceEquals(element.managedReferenceValue, storage))
                    {
                        EditorGUILayout.Space(10);
                        PropertyField(element.FindPropertyRelative("capacity"), "Capacity");
                        PropertyField(element.FindPropertyRelative("storageType"), "Storage Type");
                        PropertyField(element.FindPropertyRelative("autoCollectProduction"), "Auto Collect Production");

                        EditorGUILayout.Space(10);
                        EditorGUILayout.HelpBox("Какие типы предметов принимает склад", MessageType.Info);
                        PropertyField(element.FindPropertyRelative("allowedTags"), "Allowed Tags");

                        EditorGUILayout.Space(10);
                        EditorGUILayout.HelpBox("Описание для каких станций автосбор", MessageType.Info);
                        PropertyField(element.FindPropertyRelative("specialDescription"), "Special Description");

                        break;
                    }
                }

            }
            else
            {
                EditorGUILayout.Space(10);
                EditorGUILayout.HelpBox("Нет ассета WeaponConfig.", MessageType.Info);
            }
        }





        public void LivingModuleMain(LivingModule living)
        {
            // Левая колонка — CONFIG
            if (living != null)
            {
                Main(living);


                SerializedObject so = new SerializedObject(living.Item);

                // todo
                // ...

            }
            else
            {
                EditorGUILayout.Space(10);
                EditorGUILayout.HelpBox("Нет ассета LivingModuleConfig.", MessageType.Info);
            }
        }

        public void TavernMain(TavernModule tavern)
        {
            // Левая колонка — CONFIG
            if (tavern != null)
            {
                Main(tavern);


                SerializedObject so = new SerializedObject(tavern.Item);


                // todo
                // ...


            }
            else
            {
                EditorGUILayout.Space(10);
                EditorGUILayout.HelpBox("Нет ассета RecruitmentTavernConfig.", MessageType.Info);
            }
        }



        public void BuildingHealthMain(BuildingHealthModule buildingHealth)
        {
            if (buildingHealth != null)
            {
                SerializedProperty modules = itemManager.CurrentSO.FindProperty("modules");

                for (int i = 0; i < modules.arraySize; i++)
                {
                    var element = modules.GetArrayElementAtIndex(i);

                    if (ReferenceEquals(element.managedReferenceValue, buildingHealth))
                    {
                        EditorGUILayout.Space(10);
                        PropertyField(element.FindPropertyRelative("settings"), "Settings");
                        break;
                    }
                }
            }
            else
            {
                EditorGUILayout.Space(10);
                EditorGUILayout.HelpBox("Нет модуля BuildingHealthModule.", MessageType.Info);
            }
        }

        public void BuildingAttackMain(BuildingAttackModule buildingAttack)
        {
            if (buildingAttack != null)
            {
                SerializedProperty modules = itemManager.CurrentSO.FindProperty("modules");

                for (int i = 0; i < modules.arraySize; i++)
                {
                    var element = modules.GetArrayElementAtIndex(i);

                    if (ReferenceEquals(element.managedReferenceValue, buildingAttack))
                    {
                        EditorGUILayout.Space(10);
                        PropertyField(element.FindPropertyRelative("weaponDefinition"), "Weapon Definition");
                        PropertyField(element.FindPropertyRelative("settings"), "Settings");
                        break;
                    }
                }
            }
            else
            {
                EditorGUILayout.Space(10);
                EditorGUILayout.HelpBox("Нет модуля BuildingAttackModule.", MessageType.Info);
            }
        }

        public void BuildingPassiveDamageMain(BuildingPassiveDamageModule passiveDamage)
        {
            if (passiveDamage != null)
            {
                SerializedProperty modules = itemManager.CurrentSO.FindProperty("modules");

                for (int i = 0; i < modules.arraySize; i++)
                {
                    var element = modules.GetArrayElementAtIndex(i);

                    if (ReferenceEquals(element.managedReferenceValue, passiveDamage))
                    {
                        EditorGUILayout.Space(10);
                        PropertyField(element.FindPropertyRelative("effectConfig"), "Effect Config");
                        break;
                    }
                }
            }
            else
            {
                EditorGUILayout.Space(10);
                EditorGUILayout.HelpBox("Нет модуля BuildingPassiveDamageModule.", MessageType.Info);
            }
        }
    }
}