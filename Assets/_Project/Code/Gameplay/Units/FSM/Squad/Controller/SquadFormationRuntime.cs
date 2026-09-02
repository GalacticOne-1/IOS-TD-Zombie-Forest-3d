using UnityEngine;

namespace Galactic1.Code.Systems.Squad
{
    /// <summary>
    /// Авторитетный источник позиции и ориентации формации.
    ///
    /// Forward          — MovementHeading: направление движения центра.
    ///                    Обновляется сразу при смене сегмента пути.
    ///                    Владелец: FormationCenterDriver.
    ///
    /// FormationHeading — направление ориентации формации.
    ///                    Плавно догоняет Forward со скоростью TurnSpeed.
    ///                    Используется только FormationFollower.
    ///                    Владелец: FormationCenterDriver.
    ///
    /// Правило: только FormationCenterDriver пишет оба направления.
    /// </summary>
    public sealed class SquadFormationRuntime
    {
        /// <summary>
        /// Навигационный центр. Движется по пути. Владелец: FormationCenterDriver.
        /// Не использовать напрямую для построения формации.
        /// </summary>
        public Vector3 NavigationCenter;

        /// <summary>
        /// Центр построения формации. Плавно догоняет NavigationCenter.
        /// Владелец: FormationCenterSmoother.
        /// Именно этот центр использует FormationFollower.
        /// </summary>
        public Vector3 FormationCenter;
        public Vector3 VisualCenter;

        /// <summary>MovementHeading — мгновенное направление движения.</summary>
        public Vector3 Forward;

        /// <summary>
        /// FormationHeading — плавное направление ориентации строя.
        /// Инициализируется вместе с Forward при bootstrap.
        /// </summary>
        public Vector3 FormationHeading;

        public bool IsInitialized;

        // Обратная совместимость: Center теперь алиас NavigationCenter.
        // FormationCenterDriver пишет сюда через свойство ниже.
        public Vector3 Center
        {
            get => NavigationCenter;
            set => NavigationCenter = value;
        }
    }
}