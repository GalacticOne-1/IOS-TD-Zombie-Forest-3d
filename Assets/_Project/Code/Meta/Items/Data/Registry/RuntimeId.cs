using UnityEditor;
using UnityEngine;

namespace Galactic1.Code.GameDatabase.Registries
{
    public abstract class RuntimeId : ScriptableObject
    {
        [SerializeField, HideInInspector] private string guid;
        public string Guid => guid;

        [SerializeField] private string debugKey;
        public string DebugKey => debugKey;



        public override bool Equals(object other)
        {
            if (other is RuntimeId rid)
                return guid == rid.guid;
            return false;
        }

        public override int GetHashCode() => guid?.GetHashCode() ?? 0;


        public static bool operator ==(RuntimeId a, RuntimeId b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a is null || b is null) return false;
            return a.Equals(b);
        }

        public static bool operator !=(RuntimeId a, RuntimeId b) => !(a == b);

#if UNITY_EDITOR
        
        private void OnValidate()
        {
            debugKey = name;
        }
        /// <summary>
        /// Editor-only explicit initialization. Must be called once on creation.
        /// </summary>
        public void Editor_InitializeIfNeeded()
        {
            if (!string.IsNullOrEmpty(guid))
                return;

            Debug.LogError($"{name} got new RuntimeId");
            guid = System.Guid.NewGuid().ToString("N");
            debugKey = name;

            EditorUtility.SetDirty(this);
        }
#endif

    }
}