
using UnityEngine;

namespace Galactic1.Code.Systems.Squad
{
    /// <summary>
    /// Создаёт слоты и вычисляет LocalOffset для каждого.
    ///
    /// Оффсеты центрируются: среднее по всем слотам = 0.
    /// Поэтому центроид отряда всегда совпадает с FormationCenter,
    /// а у одиночного юнита оффсет равен нулю (он идёт ровно по линии пути).
    /// </summary>
    public sealed class SquadFormationSlots
    {
        private SquadSceneRuntime _runtime;
        public SquadSlot[] Slots { get; private set; }

        public SquadFormationSlots(
            SquadSceneRuntime runtime,
            FormationSystem.FormationType type,
            FormationSystem.GridParams gridParams)
        {
            _runtime = runtime;

            var l = _runtime.Agents.Count;
            Slots = new SquadSlot[l];

            for (int i = 0; i < l; i++)
                Slots[i] = new SquadSlot { Index = i, Occupant = _runtime.Agents[i] };

            RebuildOffsets(type, gridParams);
        }

        public void RebuildOffsets(
            FormationSystem.FormationType type,
            FormationSystem.GridParams gridParams)
        {
            int total = Slots.Length;
            Vector3 mean = Vector3.zero;

            for (int i = 0; i < total; i++)
            {
                Slots[i].LocalOffset = FormationSystem.GetOffset(
                    i, total, type, Vector3.forward, gridParams);
                mean += Slots[i].LocalOffset;
            }

            if (total > 0)
                mean /= total;

            foreach (var slot in Slots)
            {
                slot.LocalOffset -= mean;

                // Реальная позиция юнита, а не LocalOffset: иначе ComputeSpeed()
                // в первом кадре видит огромную «ошибку» и тормозит центр.
                slot.FinalWorldPosition = slot.Occupant != null
                    ? slot.Occupant.transform.position
                    : slot.LocalOffset;
            }
        }
    }
}