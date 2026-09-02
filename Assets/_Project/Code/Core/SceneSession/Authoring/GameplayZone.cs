using UnityEngine;

namespace Galactic1.Gameplay.Locations.Authoring
{
    public class GameplayZone : MonoBehaviour
    {
        [SerializeField] private Vector2 zoneCenter;
        [SerializeField] private Vector2 size;

        [SerializeField]
        private Color gizmoColor = new Color(1f, 0.61f, 0f, 0.68f);

        // бокс лежит на земле: реальная ширина/глубина зоны - X/Z, высота - фиксированная, только для видимости гизмо
        private const float BoxHeight = .5f;

        public Vector2 Size => size;
        public Vector2 ZoneCenter => zoneCenter;

        /// <summary>
        /// Мировой центр зоны: zoneCenter.x -> world X, zoneCenter.y -> world Z (глубина в плоскости земли),
        /// world Y берётся из фактической позиции transform'а зоны (а не хардкодом 0), чтобы якорь не улетал
        /// на уровень земли, если зона стоит на другой высоте.
        /// </summary>
        public Vector3 GetWorldCenter() => new Vector3(zoneCenter.x, transform.position.y, zoneCenter.y);

        /// <summary>
        /// Мировой размер бокса зоны: X/Z - реальные ширина/глубина, Y - фиксированная высота для видимости.
        /// </summary>
        public Vector3 GetWorldSize() => new Vector3(size.x, BoxHeight, size.y);

        /// <summary>
        /// Мировая точка нижней (дальней по Z) границы зоны - используется как якорь для столбца лейблов.
        /// </summary>
        public Vector3 GetBottomBorderWorldPosition()
        {
            return GetWorldCenter() - new Vector3(0f, 0f, size.y * 0.5f);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.matrix = Matrix4x4.identity;

            Gizmos.color = gizmoColor;
            Gizmos.DrawCube(GetWorldCenter(), GetWorldSize());

            // подпись имени зоны над боксом, видна при выделении зоны в Scene View
            // var labelPos = GetWorldCenter() + Vector3.up * (BoxHeight * 0.5f + 0.5f);
            // UnityEditor.Handles.Label(labelPos, gameObject.name);
        }

#endif
    }
}