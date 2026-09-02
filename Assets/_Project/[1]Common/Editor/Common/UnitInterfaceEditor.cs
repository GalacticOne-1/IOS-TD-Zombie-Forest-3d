
using Galactic1.AbstractFactory;
using Galactic1.Tools;
using UnityEditor;
using UnityEngine;

namespace Galactic1
{
    [CustomEditor(typeof(_UnitInterface))]
    public class UnitInterfaceEditor : EditorABS
    {
        private _UnitInterface script;
        
        private int tab;
        private string[] tTab = new[]
        {
            "General", "Need State", "Dev"
        };
        
        
        private void OnEnable()
        {
            script = (_UnitInterface) target;

            tab = 0;
        }
        
        
        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();

            
            EditorGUILayout.Space(15);
            
            
            
            // top menu
            EditorGUILayout.Space(10);
            var t  = GUILayout.Toolbar(tab, tTab, GUILayout.MaxWidth(230), GUILayout.MaxHeight(30));
            if (tab != t)
            {
                GUI.FocusControl(null);
            }
            tab = t;
            EditorGUILayout.Space(20);
            //


            
            switch (tab)
            {
                case 0:
                    General();
                    break;
                
                case 1:
                    State();
                    break;
                
                case 2:
                    Dev();
                    break;
            }
            
            
            
            
            EditorGUILayout.Space(15);
            
            EditorEndCheck();
        }

        private void Dev()
        {
            script._ShowLogs(EditorGUILayout.Toggle("Show logs", script.ShowLogs));
            script._OnlyThisLogs(EditorGUILayout.Toggle("Only this logs", script.OnlyThisLogs));
        }


        void General()
        {
            // #1 принадлежность юнита
            EditorGUILayout.BeginVertical(new GUIStyle() { fixedWidth = 150 });
            EditorGUILayout.HelpBox("Сторона", MessageType.Info);
            script._Team((_UnitInterface.EGameSide)EditorGUILayout.EnumPopup(script.Team));
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space(15);
            
            // #2
            Button(new CButtonData()
            {
                name = "Need Logic",
                func =  () => script._Logic(!script.RequestLogic),
                enabled = script.RequestLogic,
            });
            
            // #3 полная атака
            Button(new CButtonData()
            {
                name = "Full Attack",
                func =  () => script._FullAttack(!script.FullAttack),
                enabled = script.FullAttack,
            });
            
            // #3 визуал не менят направление
            Button(new CButtonData()
            {
                name = "Freeze Visual",
                func =  () => script._FreezeVisual(!script.FreezeVisual),
                enabled = script.FreezeVisual,
            });
        }

        void State()
        {
            EditorGUILayout.HelpBox("Какие состояния должны быть у юнита", MessageType.Info);
            
            // #1
            var st = script.RequestState;
            Button(new CButtonData()
            {
                name = "Idle",
                func = () =>
                {
                    st.idle = !st.idle;
                    script._State(st);
                },
                enabled = st.idle,
            });
            
            
            // #2
            st = script.RequestState;
            Button(new CButtonData()
            {
                name = "Movement",
                func = () =>
                {
                    st.movement = !st.movement;
                    script._State(st);
                },
                enabled = st.movement,
            });
            
            
            // #3
            st = script.RequestState;
            Button(new CButtonData()
            {
                name = "Chase",
                func = () =>
                {
                    st.chase = !st.chase;
                    script._State(st);
                },
                enabled = st.chase,
            });
            
            
            // #4
            st = script.RequestState;
            Button(new CButtonData()
            {
                name = "Attack",
                func = () =>
                {
                    st.attack = !st.attack;
                    script._State(st);
                },
                enabled = st.attack,
            });
            
            
            // #5
            st = script.RequestState;
            Button(new CButtonData()
            {
                name = "Die",
                func = () =>
                {
                    st.die = !st.die;
                    script._State(st);
                },
                enabled = st.die,
            });
        }
    }
}