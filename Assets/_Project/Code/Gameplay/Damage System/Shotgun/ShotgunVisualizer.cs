namespace GGalactic1.Code.Gameplay.Damage
{
    using UnityEngine;

    [ExecuteAlways] // чтобы рисовало даже в редакторе без Play
    public class ShotgunVisualizer : MonoBehaviour
    {
        [Header("Параметры конуса")] public float Range = 5f;
        [Range(1f, 180f)] public float Angle = 30f;

        [Header("Цвет Gizmos")] public Color ConeColor = new Color(1f, 0.5f, 0f, 0.25f);

        void OnDrawGizmos()
        {
            Gizmos.color = ConeColor;

            // Рисуем круг дальности
            Gizmos.DrawWireSphere(transform.position, Range);

            // Рисуем линии конуса
            Vector3 forward = transform.right; // в 2D обычно используем right
            Quaternion leftRot = Quaternion.AngleAxis(-Angle / 2f, Vector3.forward);
            Quaternion rightRot = Quaternion.AngleAxis(Angle / 2f, Vector3.forward);

            Vector3 leftDir = leftRot * forward * Range;
            Vector3 rightDir = rightRot * forward * Range;

            Gizmos.DrawLine(transform.position, transform.position + leftDir);
            Gizmos.DrawLine(transform.position, transform.position + rightDir);

            // Можно нарисовать сектор, чтобы нагляднее
#if UNITY_EDITOR
            UnityEditor.Handles.color = ConeColor;
            UnityEditor.Handles.DrawSolidArc(
                transform.position,
                Vector3.forward,
                leftDir.normalized,
                Angle,
                Range
            );
#endif
        }
    }

}