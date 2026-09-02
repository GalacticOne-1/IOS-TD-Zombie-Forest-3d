using System.Collections.Generic;
using UnityEngine;
using Galactic1.Code.Gameplay.Construction.Repair;

namespace Galactic1.Code.UI.Construction.Repair
{
    /// <summary>
    /// Пул строк требований ремонта. Переиспользуйте существующий
    /// build-requirement widget вместо этого класса, если он у вас уже есть —
    /// логика идентична отображению build-cost.
    /// </summary>
    public class RepairRequirementListView : MonoBehaviour
    {
        [SerializeField] private RepairRequirementRowView rowPrefab;
        [SerializeField] private Transform rowsRoot;

        private readonly List<RepairRequirementRowView> _pool = new();

        public void Render(IReadOnlyList<RepairRequirementEntry> entries)
        {
            EnsurePoolSize(entries.Count);

            for (int i = 0; i < _pool.Count; i++)
            {
                bool active = i < entries.Count;
                _pool[i].gameObject.SetActive(active);

                if (active)
                    _pool[i].Bind(entries[i]);
            }
        }

        private void EnsurePoolSize(int count)
        {
            while (_pool.Count < count)
                _pool.Add(Instantiate(rowPrefab, rowsRoot));
        }
    }
}