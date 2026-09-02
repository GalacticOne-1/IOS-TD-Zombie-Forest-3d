
using UnityEngine;
using Galactic1.AbstractFactory;
using UnityEditor;


namespace Galactic1.Code.Gameplay.Damage
{
    public static class ShotgunService
    {
        /// <summary>
        /// Наносит урон всем целям в конусе перед оружием.
        /// </summary>
        /// <param name="origin">Точка выстрела</param>
        /// <param name="direction">Направление выстрела </param>
        /// <param name="angle">Угол конуса в градусах (например 30)</param>
        /// <param name="range">Дальность выстрела</param>
        /// <param name="damage">Базовый урон за дробинку</param>
        /// <param name="pellets">Количество дробинок (урон делится или суммируется)</param>
        /// <param name="targetLayer">Слой целей</param>
        /// <param name="attacker">Кто стреляет (для DamageEvent)</param>
        public static void FireShotgun(
            Vector3 origin,
            Vector3 direction,
            float angle,
            float range,
            float damage,
            int pellets,
            LayerMask targetLayer,
            _Entity attacker)
        {
            /*
             *      direction должен раситываться из hp.collider.transform.position
             *      т.к в Collider2D[] hits лежат именно коллайдеры, а у него как правило другая позиция
             */
            
            // Находим все цели в радиусе
            Collider2D[] hits = Physics2D.OverlapCircleAll(origin, range, targetLayer);
            float angleThreshold = Mathf.Cos(angle * 0.5f * Mathf.Deg2Rad);
            direction.Normalize();

            foreach (var hit in hits)
            {
                Vector3 toTarget = (hit.transform.position - origin).normalized;
                float dot = Vector3.Dot(direction, toTarget);
                
                if (dot >= angleThreshold)
                {
                    int pelletsHit = Random.Range(Mathf.CeilToInt(pellets * 0.5f), pellets + 1);
                    float totalDamage = damage * pelletsHit;

                    // var dmg = new DamageEvent
                    // {
                    //     Attacker = attacker,
                    //     Target = hit.GetComponent<IHealthComponentCollider>().GetControlller(),
                    //     Amount = totalDamage,
                    //     Type = DamageType.Shotgun,
                    //     HitPoint = hit.transform.position
                    // };
                    //
                    // ServiceLocator.Current.Get<DamageSystem>().ApplyDamage(dmg);
                }
            }
        }


#if UNITY_EDITOR
        /// <summary>
        /// Рисует конус выстрела и подсвечивает цели для турели без поворота.
        /// </summary>
        public static void DrawDebugConeForTurret(
            Vector3 origin,
            Transform target,
            float angle,
            float range,
            Color coneColor,
            Color lineColor,
            LayerMask targetLayer)
        {
            // --- направление на цель
            Vector3 direction = Vector3.right;
            if (target != null)
            {
                direction = target.position - origin;
                direction.z = 0;
                direction.Normalize();
            }

            float angleThreshold = Mathf.Cos(angle * 0.5f * Mathf.Deg2Rad);

            // --- рисуем сектор конуса
            Quaternion leftRot = Quaternion.AngleAxis(-angle / 2f, Vector3.forward);
            Quaternion rightRot = Quaternion.AngleAxis(angle / 2f, Vector3.forward);

            Vector3 leftDir = leftRot * direction;
            Vector3 rightDir = rightRot * direction;

            Handles.color = coneColor;
            Handles.DrawSolidArc(origin, Vector3.forward, leftDir, angle, range);

            // линии и окружность
            Gizmos.color = lineColor;
            Gizmos.DrawWireSphere(origin, range);
            Gizmos.DrawLine(origin, origin + leftDir * range);
            Gizmos.DrawLine(origin, origin + rightDir * range);

            // --- подсветка целей
            Collider2D[] hits = Physics2D.OverlapCircleAll(origin, range, targetLayer);
            foreach (var hit in hits)
            {
                Vector3 toTarget = (hit.transform.position - origin);
                toTarget.z = 0;
                toTarget.Normalize();

                float dot = Vector3.Dot(direction, toTarget);

                // цвет: зелёный — попадёт, красный — не попадёт
                Gizmos.color = (dot >= angleThreshold) ? Color.green : Color.red;
                Gizmos.DrawWireSphere(hit.transform.position, 0.1f);

#if UNITY_EDITOR
                Handles.Label(hit.transform.position + Vector3.up * 0.15f,
                    $"{hit.name}\nDot={dot:F2}");
#endif
            }

            // --- вывод angleThreshold
            Handles.Label(origin + Vector3.up * 0.5f,
                $"angleThreshold = {angleThreshold:F2}");
        }
#endif
    }
}