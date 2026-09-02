using UnityEngine;

namespace Galactic1.Code.Gameplay.Units
{
    // ─────────────────────────────────────────────
    //  1. CoverPoint — размещается на сцене дизайнером
    //     Простой маркер. MarineReactiveAI ищет их
    //     через CoverFinder.
    // ─────────────────────────────────────────────

    public sealed class CoverPoint : MonoBehaviour
    {
        [Tooltip("Направление 'лицом к укрытию' — откуда стрелять при peek")] [SerializeField]
        private Transform peekDirection;

        public bool IsOccupied { get; private set; }
        public SurvivorInstance Occupant { get; private set; }

        public Vector3 Position => transform.position;

        public Vector3 PeekForward => peekDirection != null
            ? peekDirection.forward
            : transform.forward;

        public bool TryOccupy(SurvivorInstance unit)
        {
            if (IsOccupied && Occupant != unit) return false;
            IsOccupied = true;
            Occupant = unit;
            return true;
        }

        public void Release(SurvivorInstance unit)
        {
            if (Occupant != unit) return;
            IsOccupied = false;
            Occupant = null;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = IsOccupied ? Color.red : Color.green;
            Gizmos.DrawWireCube(transform.position, Vector3.one * 0.4f);
            Gizmos.DrawRay(transform.position, PeekForward * 0.6f);
        }
    }
}