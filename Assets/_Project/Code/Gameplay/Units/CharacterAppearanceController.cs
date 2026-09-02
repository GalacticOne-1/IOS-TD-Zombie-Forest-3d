using System;
using System.Collections.Generic;
using Galactic1.Code.GameDatabase.Registries;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Units
{
    [DisallowMultipleComponent]
    public sealed class CharacterAppearanceController : MonoBehaviour
    {
        [SerializeField]
        private List<AppearanceSet> _appearanceSets = new();

        [SerializeField]
        private AppearanceId _defaultAppearance;

        /// <summary>
        /// Применить внешний вид по индексу.
        /// </summary>
        public void Apply(AppearanceId id)
        {
            DisableAll();

            foreach (var set in _appearanceSets)
            {
                if (set.Id == id)
                {
                    foreach (var obj in set.Objects)
                    {
                        if (obj != null)
                            obj.SetActive(true);
                    }
                    return;
                }
            }

            Debug.LogError($"Appearance '{id?.DebugKey}' not found.", this);
        }

        /// <summary>
        /// Применить внешний вид по умолчанию.
        /// </summary>
        public void ApplyDefault()
        {
            Apply(_defaultAppearance);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!Application.isPlaying)
                ApplyDefault();
        }
#endif

        private void Awake()
        {
            ApplyDefault();
        }

        private void DisableAll()
        {
            foreach (var set in _appearanceSets)
            {
                foreach (var obj in set.Objects)
                {
                    if (obj != null)
                        obj.SetActive(false);
                }
            }
        }

        [Serializable]
        public sealed class AppearanceSet
        {
            [SerializeField]
            private AppearanceId _id;

            [SerializeField]
            private List<GameObject> _objects = new();

            public AppearanceId Id => _id;
            public IReadOnlyList<GameObject> Objects => _objects;
        }
    }
}