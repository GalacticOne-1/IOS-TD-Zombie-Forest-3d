using System.Collections.Generic;
using Galactic1.Code.Gameplay.Units;
using UnityEngine;

namespace Galactic1.Code.Systems.Squad
{
    /// <summary>
    /// Создаёт слоты и вычисляет LocalOffset для каждого.
    ///
    /// LocalOffset пересчитывается только при:
    ///   - создании отряда
    ///   - смене типа формации
    ///   - смене параметров формации
    ///   - изменении состава отряда
    ///
    /// Никогда не пересчитывается во время обычного движения.
    ///
    /// FormationFollower использует LocalOffset каждый тик,
    /// применяя вращение через Quaternion.LookRotation —
    /// без повторного вызова FormationSystem.GetOffset().
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

        /// <summary>
        /// Пересчитывает LocalOffset для всех слотов.
        /// Вызывается только при смене формации / параметров / состава.
        /// </summary>
        public void RebuildOffsets(
            FormationSystem.FormationType type,
            FormationSystem.GridParams gridParams)
        {
            int total = Slots.Length;
            for (int i = 0; i < total; i++)
            {
                // Vector3.forward как нейтральный базис.
                // FormationFollower применит реальный forward через Quaternion.LookRotation.
                Slots[i].LocalOffset = FormationSystem.GetOffset(
                    i, total, type, Vector3.forward, gridParams);
            }

            // Инициализируем FinalWorldPosition чтобы ComputeSpeed()
            // в первом тике не работал с Vector3.zero.
            foreach (var slot in Slots)
                slot.FinalWorldPosition = slot.LocalOffset;
        }
    }
}