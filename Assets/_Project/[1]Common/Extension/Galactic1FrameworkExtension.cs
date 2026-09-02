using System;
using System.Collections.Generic;
using System.IO;
using Galactic1.UI.Core;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;


namespace Galactic1
{
    public static partial class Galactic1FrameworkExtension
    {
        
        
        static System.Random rdm = new System.Random();

        public static T Random<T>(this List<T> arr) => arr[UnityEngine.Random.Range(0, arr.Count)];
        
        
        public static T RandomByChance<T>(this List<T> arr) where T: IRandom
        {

            var total = 0f;
            var probs = new float[arr.Count];

            var l = probs.Length;
            for (int i = 0; i < l; i++)
            {
                probs[i] = arr[i].Chance;
                total += probs[i];
            }

            var randomPoint = (float)rdm.NextDouble() * total;

            for (int i = 0; i < l; i++)
            {
                if (randomPoint < probs[i])
                    return arr[i];
                randomPoint -= probs[i];
            }
            
            return arr[0];
        }

        /// <summary>
        /// true - шанс выпал (0 - 100)
        /// </summary>
        /// <param name="chance"></param>
        /// <returns></returns>
        public static bool Chance(this int chance)
            => UnityEngine.Random.Range(0, 100) <= chance;





        #region >>> NEW

        public static float MAT_Distance(this Vector3 a, Vector3 b) => Vector3.Distance(a, b);

        public static Vector3 MAT_Direction(this Vector3 a, Vector3 b) => a - b;

        /// <summary>
        /// Вернет точку столкновения двух коллайдеров
        /// <br/>(for OnTriggerEnter2D)
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="collider"></param>
        /// <returns></returns>
        public static Vector2 COL_TriggerHitPoint(this Collider2D collider, Vector2 owner) => collider.ClosestPoint(owner);
        
        /// <summary>
        /// Вернет точку столкновения двух коллайдеров
        /// <br/>(for OnTriggerEnter)
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="collider"></param>
        /// <returns></returns>
        public static Vector3 COL_TriggerHitPoint(this Collider collider, Vector3 owner) => collider.ClosestPoint(owner);


        /// <summary>
        /// Создает куб
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static GameObject CREATE_Cube(this GameObject obj) => GameObject.CreatePrimitive(PrimitiveType.Cube);



        /// <summary>
        /// Поиск ближайших коорд из списка
        /// </summary>
        /// <param name="center"></param>
        /// <param name="list"></param>
        /// <param name="id"></param>
        public static void FIND_NearestCoord_ID(this Vector2 center, List<Vector2> list, out int id)
        {
            id = 0;
            float nearest = float.MaxValue;
            float distance;

            var l = list.Count;
            for (int i = 0; i < l; i++)
            {
                distance = Vector2.Distance(center, list[i]);
                if (distance < nearest)
                {
                    nearest = distance;
                    id = i;
                }
            }
        }
        
        /// <summary>
        /// Поиск ближайших коорд из списка
        /// </summary>
        /// <param name="center"></param>
        /// <param name="list"></param>
        /// <param name="id">возвращает id элемента из списка</param>
        public static void FIND_NearestCoord_ID(this Vector3 center, List<Vector2> list, out int id)
        {
            id = 0;
            float nearest = float.MaxValue;
            float distance;

            var l = list.Count;
            for (int i = 0; i < l; i++)
            {
                distance = Vector2.Distance(center, list[i]);
                if (distance < nearest)
                {
                    nearest = distance;
                    id = i;
                }
            }
        }
        
        /// <summary>
        /// Поиск ближайших коорд из списка
        /// </summary>
        /// <param name="center"></param>
        /// <param name="list"></param>
        /// <param name="id">возвращает id элемента из списка</param>
        /// <param name="coord"></param>
        public static void FIND_NearestCoord_ID(this Vector3 center, List<Collider2D> list, out int id, out Vector2 coord)
        {
            id = 0;
            coord = Vector2.zero;
            float nearest = float.MaxValue;
            float distance;

            var l = list.Count;
            for (int i = 0; i < l; i++)
            {
                coord = list[id].COL_TriggerHitPoint(center);
                distance = Vector2.Distance(center, coord);
                if (distance < nearest)
                {
                    nearest = distance;
                    id = i;
                }
            }
        }
        
        
        /// <summary>
        /// Найдет или создаст объект
        /// </summary>
        /// <param name="root">где искать</param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static GameObject FindCreateObject(this GameObject root, string name)
        {
            var obj = root.transform.Find(name);
            if (obj == null)
                obj = new GameObject(name).transform;
            return obj.gameObject;
        }
        
        #endregion


        #region HELPER

        /// <summary>
        /// Выбор между 0 и 1 из массива
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static int SelectFloorTop(this int val, int arLegth)
            => val == 0 ? arLegth - 1 : 0;
        
        
        public static Color RandomColor(this Color color) 
            => new Color(UnityEngine.Random.Range(0, 1.0f), UnityEngine.Random.Range(0, 1.0f), UnityEngine.Random.Range(0, 1.0f));
        
        
        /// <summary>
        /// Пропускает отсутствующие компоненты.
        /// </summary>
        public static T[] CollectChildren<T>(this Transform root)
                where T : Component
            {
                var result = new T[root.childCount];
        
                for (int i = 0; i < root.childCount; i++)
                    result[i] = root.GetChild(i).GetComponent<T>();
        
                return result;
            }
        
        /// <summary>
        /// Падает ошибкой если компонент отсутствует.
        /// </summary>
        public static T[] CollectChildrenStrict<T>(this Transform root)
            where T : Component
        {
            List<T> result = new();

            for (int i = 0; i < root.childCount; i++)
            {
                if (root.GetChild(i).TryGetComponent<T>(out var component))
                    result.Add(component);
            }

            return result.ToArray();
        }

        #endregion
        


        #region MATHF

        /// <summary>
        /// Вернет значение скорости, если нужно действие за конкретное время
        /// </summary>
        /// <param name="value"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public static float TimeToSpeed(this float value, float time) => value / time;
        
        /// <summary>
        /// true - каждые n раз
        /// </summary>
        /// <param name="val">что проверяем</param>
        /// <param name="target">нужное число</param>
        /// <returns></returns>
        public static bool EachNumber(this int val, int target) => val % target == 0;
        /// <summary>
        /// true - каждые n раз
        /// </summary>
        /// <param name="val">что проверяем</param>
        /// <param name="target">нужное число</param>
        /// <returns></returns>
        public static bool EachNumber(this byte val, byte target) => val % target == 0;

        /// <summary>
        /// Переводит число в процент (0.1/0.67/1.25)
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static float ToPercent(this int val) => (float)val / 100;
        /// <summary>
        /// Переводит число в процент (0.1/0.67/1.25)
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static float ToPercent(this float val) => val / 100;

        /// <summary>
        /// Покажет число процента в формате (0-100%)
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static string ShowPercent(this int val) => (val * 100).ToString();
        /// <summary>
        /// Покажет число процента в формате (0-100%)
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static string ShowPercent(this float val) => (val * 100).ToString();
        
        /// <summary>
        /// Значение в процентах, по отношению к макс. значению (вернет 0.4)
        /// </summary>
        /// <param name="curr">значение для перевода в проценты</param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static float PercentFrom(this float curr, float max) => ((curr * 100) / max) / 100;
        /// <summary>
        /// Значение в процентах, по отношению к макс. значению (вернет 0.4)
        /// </summary>
        /// <param name="curr">значение для перевода в проценты</param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static float PercentFrom(this int curr, float max) => ((curr * 100) / max) / 100;
        
        /// <summary>
        /// Разделяет число на целое и остаток
        /// </summary>
        /// <param name="val"></param>
        /// <param name="_int"></param>
        /// <param name="residue"></param>
        public static void SeparateFloat(this float val, out int _int, out float residue)
        {
            _int = (int) val;
            residue = val - _int;
        }
        
        /// <summary>
        /// Обрежет число после запятой на один символ (будет как этo 22.4 или 0.3)
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static string ShortFloat(this float val)
        {
            //DLog.Alert($"{val}");
            string str = val.ToString();
            var l = str.Length;
            for (int i = 0; i < l; i++)
            {
                if (str[i] == '.')
                    //return str.Substring(0, i + 3 > l ? i + 2 : i + 3);
                    return str.Substring(0, int.Parse($"{str[i+1]}") == 0 ? i : i+2);
                
            }
            return str; 
        }
        /// <summary>
        /// Обрежет число после запятой на два символа (будет как этo 30.09 или 2.01)
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static string ShortFloatDetail(this float val)
        {
            //DLog.Alert($"{val}");
            string str = val.ToString();
            var l = str.Length;
            for (int i = 0; i < l; i++)
            {
                if (str[i] == '.')
                    return str.Substring(0, l-(i+1) >= 2 ? i+3 : i+2);
                
            }
            return str; 
        }

        /// <summary>
        /// Добавляет 0 вперед числа  если  меньше 10  (01,02 ...)
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static string NumberWithNull(this int val) => val < 10 ? $"0{val}" : $"{val}";
        
        

        /// <summary>
        /// ДБ звука в liner
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static float DBToFloat(this float val)
            => Mathf.Log10(val) * 20;

        /// <summary>
        /// true - если угол между двумя объектами не превышает переданное значение
        /// </summary>
        /// <param name="center"></param>
        /// <param name="target"></param>
        /// <param name="maxAvailableAngle"></param>
        /// <returns></returns>
        public static bool AngleForYAxis(this Transform center, Transform target, float maxAvailableAngle)
        {
            Vector3 a, b;
            a = target.position;
            b = center.position;
            a.y = b.y = 0;
            Vector3 dir = a - b;
            dir = center.InverseTransformDirection(dir);
            var angle = Mathf.Atan2(dir.z, dir.x) * Mathf.Rad2Deg;
            // 90 градусов == 0
            //Debug.Log("angle "+angle);
            return angle > 0 && angle < (90 + maxAvailableAngle / 2) && angle > (90 - maxAvailableAngle / 2);
        }
        
        /// <summary>
        /// true - если угол между двумя объектами не превышает переданное значение
        /// </summary>
        /// <param name="center"></param>
        /// <param name="target"></param>
        /// <param name="maxAvailableAngle"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public static bool AngleForYAxis(this Transform center, 
            Transform target, 
            float maxAvailableAngle,
            out Vector3 direction)
        {
            Vector3 a, b;
            a = target.position;
            b = center.position;
            a.y = b.y = 0;
            Vector3 dir = a - b;
            direction = dir;
            dir = center.InverseTransformDirection(dir);
            var angle = Mathf.Atan2(dir.z, dir.x) * Mathf.Rad2Deg;
            //Debug.Log("angle "+angle);
            return angle > 0 && angle < (90 + maxAvailableAngle / 2) && angle > (90 - maxAvailableAngle / 2);
        }
        /// <summary>
        /// возвращает угол между двумя объектами
        /// </summary>
        /// <param name="center"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static float AngleForYAxis(this Transform center, Transform target)
        {
            Vector3 a, b;
            a = target.position;
            b = center.position;
            a.y = b.y = 0;
            Vector3 dir = a - b;
            dir = center.InverseTransformDirection(dir);
            var angle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
            //Debug.Log("angle "+angle);
            return angle;
        }
        
        
        /// <summary>
        /// возвращает угол между двумя объектами
        /// </summary>
        /// <param name="center"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static float AngleForYAxis2D(this Transform center, Transform target)
        {
            // расчет по направлению Х и в право
            // 0 - 180
            Vector3 dir = target.position - center.position;
            var angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            //Debug.Log("angle "+angle);
            return angle;
        }
        
        /// <summary>
        /// Вернет угол между векторами (0 - 180)
        /// </summary>
        /// <param name="vec1"></param>
        /// <param name="vec2"></param>
        /// <returns></returns>
        public static float AngleBetweenVectors_180(this Vector2 vec1, Vector2 vec2)
        {
            // расчет по направлению Х и в право
            // 0 - 180
            Vector2 diference = vec2 - vec1;
            float sign = (vec2.y < vec1.y)? -1.0f : 1.0f;
            return Vector2.Angle(Vector2.right, diference) * sign;
        }
        /// <summary>
        /// Вернет угол между векторами (0 - 360)
        /// </summary>
        /// <param name="vec1"></param>
        /// <param name="vec2"></param>
        /// <returns></returns>
        public static float AngleBetweenVectors_360(this Vector2 vec1, Vector2 vec2)
        {
            // расчет по направлению Х и в право
            // 0 - 360
            Vector2 diference = vec2 - vec1;
            float sign = (vec2.y < vec1.y)? -1.0f : 1.0f;
            float angle = Vector2.Angle(Vector2.right, diference) * sign;
            if(angle < 0) angle = 360 - angle * -1;
            return angle;
        }

        
        public static Vector3 GetVectorFromAngle(float angle)
        {
            float angleRad = angle * (Mathf.PI / 180f);
            return new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
        }
        
        
        /// <summary>
        /// Возвращает дистанцию
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static float Distance(this Vector3 a, Vector3 b) => (a - b).magnitude;
        /// <summary>
        /// Вернет строку в формате таймера 00:00 (min + sec)
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static string FormatTime(this float time)
        {
            string minutes = Mathf.Floor(time / 60).ToString("00");
            string seconds = Mathf.Floor(time % 60).ToString("00");
            return $"{minutes}:{seconds}";
        }
        
        /// <summary>
        /// Время в приблизительном формате ( 1m )
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static string FormatTimeRaw(this int val)
        {
            if (val < 60)
                return "<1m";
            if (val < 3600)
                return $"{val/60}m";
            
            TimeSpan time = TimeSpan.FromSeconds(val);
            return $"{time.ToString("hh")}h";
        }
        /// <summary>
        /// Время в приблизительном формате ( 1m )
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static string FormatTimeRaw(this short val)
        {
            if (val < 60)
                return "<1m";
            if (val < 3600)
                return $"{val/60}m";
            
            TimeSpan time = TimeSpan.FromSeconds(val);
            return $"{time.ToString("hh")}h";
        }
        /// <summary>
        /// Время в приблизительном формате ( 1m )
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static string FormatTimeRaw(this long val)
        {
            if (val < 60)
                return "<1m";
            if (val < 3600)
                return $"{val/60}m";
            
            TimeSpan time = TimeSpan.FromSeconds(val);
            return $"{time.ToString("hh")}h";
        }
        
        /// <summary>
        /// Вернет строку в формате 00:00:00 (hours + min + sec)
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static string FormatTimeLong(this long val)
        {
            TimeSpan time = TimeSpan.FromSeconds(val);
            return time.ToString("hh':'mm':'ss");
        }
        
        /// <summary>
        /// Вернет строку в формате 00:00 (hours + min)
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static string FormatTimeShort(this long val)
        {
            TimeSpan time = TimeSpan.FromSeconds(val);
            return time.ToString("hh':'mm");
        }
        /// <summary>
        /// Вернет строку в формате 00:00 (hours + min)
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static string FormatTimeShort(this short val)
        {
            TimeSpan time = TimeSpan.FromSeconds(val);
            return time.ToString("hh':'mm");
        }
        /// <summary>
        /// Вернет строку в формате 00:00 (hours + min)
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static string FormatTimeShort(this int val)
        {
            TimeSpan time = TimeSpan.FromSeconds(val);
            return time.ToString("hh':'mm");
        }
        /// <summary>
        /// Вернет строку в формате 00:00 (min + sec)
        /// </summary>
        public static string FormatTimeMinuts(this int val)
        {
            TimeSpan time = TimeSpan.FromSeconds(val);
            return time.ToString("mm':'ss");
        }
        /// <summary>
        /// Вернет строку в формате 00:00 (min + sec)
        /// </summary>
        public static string FormatTimeMinuts(this short val)
        {
            TimeSpan time = TimeSpan.FromSeconds(val);
            return time.ToString("mm':'ss");
        }
        /// <summary>
        /// Вернет строку в формате 1D 15H or 1H 23M
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static string FormatTimeShort_arcade(this int val)
        {
            var str = "";
            var v = val / TimeManagement.dayInSeconds;
            if (v > 0)    // format D:H
            {
                str = $"{v}d {(int)((val - v * TimeManagement.dayInSeconds) / TimeManagement.hourInSeconds)}H";
            }
            else            // format H:M
            {
                v = val / TimeManagement.hourInSeconds;
                str = $"{v}h {(int)((val - v * TimeManagement.hourInSeconds) / 60)}m";
            }

            return str;
        }
        /// <summary>
        /// Вернет строку в формате 00:00:00 (hours + min + sec)
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static string FormatTimeLong(this int val)
        {
            TimeSpan time = TimeSpan.FromSeconds(val);
            return time.ToString("hh':'mm':'ss");
        }
        /// <summary>
        /// Вернет строку в формате 00:00:00 (hours + min + sec)
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static string FormatTimeLong(this short val)
        {
            TimeSpan time = TimeSpan.FromSeconds(val);
            return time.ToString("hh':'mm':'ss");
        }


        /// <summary>
        /// 2d 1h / 12h 45m / 4m 9s / 17s
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static string FormatTimeArcade(this short val)
        {
            int v = val;
            return v.FormatTimeArcade();
        }
        
        /// <summary>
        /// 2d 1h / 12h 45m / 4m 9s / 17s
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static string FormatTimeArcade(this int val)
        {
            var day = val / TimeManagement.dayInSeconds;
            var hour = val / TimeManagement.hourInSeconds;
            var minutes = val / 60;
            var seconds = val % 60;

            var str = "";
            if (day > 0)
            {
                str = $"{day}d {(int)((val - day * TimeManagement.dayInSeconds) / TimeManagement.hourInSeconds)}h";
                
                // если ровно 24часа, то показываем 24h 00m
                if(val - day * TimeManagement.dayInSeconds == 0)
                    str = $"{hour}h 00m";
            }
            
            else if (hour > 0)
            {
                str = $"{hour}h {(int)((val - hour * TimeManagement.hourInSeconds) / 60)}m";
            }
            else if (minutes > 0)
            {
                str = $"{minutes}m {(int)((val - minutes * 60) % 60)}s";
            }
            else
            {
                str = $"{seconds}s";
            }

            return str;
        }

        #endregion


        #region MATHF LERP
        
        
        /// <summary>
        /// 3апускается медленно
        /// </summary>
        public static float EaseIn(this float t) => t * t;

        /// <summary>
        /// Старт с задержкой, потом быстро
        /// </summary>
        public static float DelayIn(this float t) => t * t * (3f - 2f * t);

        /// <summary>
        /// Подбросить
        /// </summary>
        public static float Flip(this float x) => 1 - x;
        
        /// <summary>
        /// Зеркальное, для кнопок
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static float Spike(this float t)
        {
            if (t <= .5f)
                return EaseIn(t / .5f);
 
            return EaseIn(Flip(t)/.5f);
        }
        
        
        

        #endregion
        
        #region Vectors

        /// <summary>
        /// true - координаты рвзные
        /// </summary>
        /// <param name="start"></param>
        /// <param name="curr"></param>
        /// <returns></returns>
        public static bool DifferentCoord(this Vector3 start, Vector3 curr)
            => start.x != curr.x || start.y != curr.y || start.z != curr.z;
        /// <summary>
        /// true - координаты рвзные
        /// </summary>
        /// <param name="start"></param>
        /// <param name="curr"></param>
        /// <returns></returns>
        public static bool DifferentCoord(this Vector2 start, Vector2 curr)
            => start.x != curr.x || start.y != curr.y;
        
        
        /// <summary>
        /// Добавить значение по оси Y
        /// </summary>
        /// <param name="coord"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static Vector3 ADD_TO_Y(this Vector3 coord, float offset)
        {
            coord.y += offset;
            return coord;
        }
        
        public static Vector3 SET_COORD(this Vector3 coord, Vector3 add)
        {
            coord = add;
            return coord;
        }

        /// <summary>
        /// Сдвигает координаты по random Y
        /// </summary>
        /// <param name="target"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static Vector3 OffsetY(this Vector3 coord, float offset)
        {
            coord.y += UnityEngine.Random.Range(-offset, offset);
            return coord;
        }
        
        /// <summary>
        /// Меняет положение точки-цели влево/вправо относительно точки
        /// </summary>
        /// <param name="point"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static Vector3 OffsetLR(this Transform point, float offset)
            => point.position + point.right * offset;

        /// <summary>
        /// Меняет положение точки-цели влево/вправо 
        /// </summary>
        /// <param name="point"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static Vector3 OffsetLR(this Vector3 point, float offset)
            => point + Vector3.right * offset;

        /// <summary>
        /// Случайные координаты (-1/1)
        /// </summary>
        /// <returns></returns>
        public static Vector2 GetRandomDirection()
            => new Vector2(UnityEngine.Random.Range(-2, 2), UnityEngine.Random.Range(-2, 2)).normalized;

        /// <summary>
        /// Координаты в случайном направлении
        /// </summary>
        /// <param name="coord"></param>
        /// <returns></returns>
        public static Vector3 GetPositionFromRandomDirection(this Vector3 coord, float range = 3)
        {
            Vector2 c = coord;
            return c + GetRandomDirection() * range;
        }

        /// <summary>
        /// Координаты в направлении цели но на нужную длинну
        /// </summary>
        /// <param name="center"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        public static Vector3 GetPositionFromDirection(this Vector3 center, Vector3 target, float range)
            => (center + (target - center).normalized * range);



        /// <summary>
        /// Вернет координаты между двумя точками на нужном расстоянии от target
        /// </summary>
        /// <param name="distance">на какое рсстояние от точки A</param>
        /// <param name="a">target</param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Vector3 GetPointBetweenVectors(this float distance, Vector3 a, Vector3 b)
        {
            var ab = b - a;
            ab = ab.normalized;
            return a + distance * ab;
        }

        /// <summary>
        /// Кнвертирует обычный вектор в Vector2Int
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static Vector2Int ConvertVector2(this Vector2 v) => new Vector2Int((int)v.x, (int)v.y);
        /// <summary>
        /// Кнвертирует обычный вектор в Vector2Int
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static Vector2Int ConvertVector2(this Vector3 v) => new Vector2Int((int)v.x, (int)v.y);
        
        
        /// <summary>
        /// Кнвертирует обычный вектор в Vector3Int
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static Vector3Int ConvertVector3(this Vector3 v) => new Vector3Int((int)v.x, (int)v.y);
        
        #endregion

        #region Rotate
        
        /// <summary>
        /// Поворот в сторону цели
        /// </summary>
        /// <param name="center"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static Quaternion LookToTargte2D(this Transform center, Vector3 target)
        {
            var dir = target - center.position;
            var angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            return Quaternion.AngleAxis(angle, Vector3.forward);
        }

        #endregion
        
        #region Ray
        
        /// <summary>
        /// Вернет коллайдер на пути луча
        /// </summary>
        /// <param name="startRay"></param>
        /// <param name="direction"></param>
        /// <param name="layer"></param>
        /// <param name="distanceRay"></param>
        /// <returns></returns>
        public static RaycastHit2D Ray2d(this Vector3 startRay, Vector2 direction, float distanceRay, LayerMask layer)
        {
            Debug.DrawRay(startRay, direction * distanceRay, Color.green, 2f);
            return Physics2D.Raycast(startRay, direction, distanceRay, layer);
        }
        
        /// <summary>
        /// True - луч столкнулся с целью
        /// </summary>
        /// <returns></returns>
        public static bool Ray2d(this Transform center, GameObject target, LayerMask layer)
        {
            Vector3 direction = target.transform.position - center.position;
            var hit = Physics2D.Raycast(center.position, direction, direction.magnitude, layer);
            if (!hit) return false;
            return hit.collider.gameObject == target;
        }
        
        /// <summary>
        /// Возвращает объект столкновения
        /// </summary>
        /// <param name="startRay"></param>
        /// <param name="target"></param>
        /// <param name="layer"></param>
        /// <param name="distance">to target</param>
        /// <returns></returns>
        public static GameObject Ray2d_get_object(
            this Vector3 startRay, 
            Vector3 target, 
            LayerMask layer, 
            out float distance,
            byte unitLog)
        {
            distance = float.MaxValue;
            Vector3 direction = target - startRay;
            var hit = Physics2D.Raycast(startRay, direction, direction.magnitude, layer);
            
            Color rayColor = hit.collider != null ? Color.green : Color.red;
            Debug.DrawRay(startRay, direction, rayColor, .1f);
            if (!hit)
            {
                DLog.Alert("Physics2D.Raycast >>> NULL", EDlogColor.RED, unitLog);
                return null;
            }
            //Debug.Log($">>>>>  {hit.transform}", hit.collider.gameObject);
            distance = hit.distance;
            return hit.collider.gameObject;
        }
        
        /// <summary>
        /// Возвращает точку столкновения с целью, игнорируя другие коллайдеры
        /// </summary>
        /// <param name="center"></param>
        /// <param name="target"></param>
        /// <param name="layer"></param>
        /// <returns></returns>
        public static Vector3 RayPoint(this Transform center, GameObject target, LayerMask layer)
        {
            Vector3 direction = target.transform.position - center.position;
            RaycastHit[] hits = Physics.RaycastAll(center.position, direction, direction.magnitude, layer);
            var l = hits.Length;
            for (int i = 0; i < l; i++)
            {
                if (hits[i].transform.gameObject == target)
                    return hits[i].point;
            }

            return target.transform.position;
        }
        
        public static float RayDistance(this Vector3 startRay, GameObject target, LayerMask layer)
        {
            Vector3 direction = target.transform.position - startRay;
            direction.y += .2f;
            Debug.DrawRay(startRay, direction, Color.green, .1f);
            RaycastHit[] hits = Physics.RaycastAll(startRay, direction, 100);
            var l = hits.Length;
            for (int i = 0; i < l; i++)
            {
                DLog.Alert($">>> ray hit : {hits[i].transform}");
                if (hits[i].transform.gameObject == target)
                    return hits[i].distance;
            }

            DLog.Alert($">>> ray hit all : {hits.Length}");
            return 10000;
        }
        
        
        
        /// <summary>
        /// Вернет все объекты попавшие в луч
        /// </summary>
        /// <param name="startRay"></param>
        /// <param name="endRay"></param>
        /// <param name="layer"></param>
        /// <returns></returns>
        public static RaycastHit2D[] GetObjInRay(this Transform startRay, Vector3 endRay, float distance, LayerMask layer)
        {
            Vector3 direction = endRay - startRay.position;
            Debug.DrawLine(startRay.position, direction * distance, Color.green, 2f);
            RaycastHit2D[] hits = Physics2D.RaycastAll(startRay.position, direction, distance, layer);
            return hits;
        }
        
        /// <summary>
        /// Вернет все объекты попавшие в луч
        /// </summary>
        /// <param name="startRay"></param>
        /// <param name="direction"></param>
        /// <param name="distance"></param>
        /// <param name="layer"></param>
        /// <returns></returns>
        public static RaycastHit2D[] GetObjInRay(this Vector3 startRay, Vector3 direction, float distance, LayerMask layer)
        {
            Debug.DrawRay(startRay, direction * distance, Color.green, 2f);
            RaycastHit2D[] hits = Physics2D.RaycastAll(startRay, direction, distance, layer);
            return hits;
        }
        
        /// <summary>
        /// Вернет все объекты попавшие в луч (ищет в трех слоях)
        /// </summary>
        /// <param name="startRay"></param>
        /// <param name="endRay"></param>
        /// <param name="layer">3 слоя для врагов</param>
        /// <returns></returns>
        public static RaycastHit2D[] GetObjInRay(this Transform startRay, Vector3 endRay, LayerMask[] layer)
        {
            //Debug.DrawLine(startRay.position, endRay, Color.green, .2f);
            Vector3 direction = endRay - startRay.position;
            RaycastHit2D[] hits = Physics2D.RaycastAll(startRay.position, direction, direction.magnitude,
                layer[0] | layer[1] | layer[2]);
            return hits;
        }

        #endregion
        
        #region GameObject
        
        /// <summary>
        /// Скрывает вложенные элементы
        /// </summary>
        /// <param name="hold"></param>
        public static void MakeHidden(this Transform hold)
        {
            var l = hold.childCount;
            for (int i = l-1; i >= 0; i--)
                hold.GetChild(i).gameObject.SetActive(false);
        }

        /// <summary>
        /// Очищает от вложенных элементов
        /// <br/>Для UI не рекомендовано! Удаление мдет в конце кадра
        /// </summary>
        /// <param name="hold"></param>
        public static void MakeEmpty(this Transform hold)
        {
            var l = hold.childCount;
            F f = new F();
            for (int i = l-1; i >= 0; i--)
                f.RemoveObj(hold.GetChild(i).gameObject);
        }
        
        /// <summary>
        /// Очищает от вложенных элементов
        /// <br/>Безопастно для UI
        /// </summary>
        /// <param name="hold"></param>
        public static void MakeEmptyImmediate(this Transform hold)
        {
            for (int i = hold.childCount - 1; i >= 0; i--)
                GameObject.DestroyImmediate(hold.GetChild(i).gameObject);
        }
        
        /// <summary>
        /// Очищает от детей
        /// </summary>
        /// <param name="hold"></param>
        /// <param name="el_save">элемент который должен остаться</param>
        public static void MakeEmpty(this Transform hold, int el_save)
        {
            var l = hold.childCount;
            F f = new F();
            for (int i = l-1; i >= 0; i--)
                if (i != el_save)
                    f.RemoveObj(hold.GetChild(i).gameObject);
        }
        /// <summary>
        /// Перенос всех детей в другой объект
        /// </summary>
        /// <param name="hold"></param>
        /// <param name="garbage"></param>
        public static void MakeEmpty(this Transform hold, Transform target)
        {
            var l = hold.childCount;
            for (int i = l-1; i >= 0; i--)
                hold.GetChild(i).SetParent(target);
        }
        /// <summary>
        /// Очищает от детей в редакторе
        /// </summary>
        /// <param name="hold"></param>
        public static void MakeEmptyEditor(this Transform hold)
        {
            var l = hold.childCount;
            F f = new F();
            for (int i = l-1; i >= 0; i--)
                f.RemoveObjEditor(hold.GetChild(i));
        }

        /// <summary>
        /// Создает GO и добавляет его в родительский объект (Prefab)
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static GameObject CreateGO(this GameObject prefab, Transform parent)
        {
            F f = new F();
            GameObject go = f.Instance(prefab, parent);
            return go;
        }
        /// <summary>
        /// Создает GO и добавляет его в родительский объект (Resources.Load)
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="asset"></param>
        /// <returns></returns>
        public static GameObject CreateGO(this string asset, Transform parent)
        {
            F f = new F();
            GameObject go = f.Instance(asset, parent);
            return go;
        }
        /// <summary>
        /// Удаление объекта
        /// </summary>
        /// <param name="g"></param>
        public static void DestroyGO(this GameObject g)
        {
            F f = new F();
            f.RemoveObj(g);
        }
        
        
        /// <summary>
        /// Скрывает/показывает всех детей 
        /// </summary>
        /// <param name="hold"></param>
        /// <param name="y"></param>
        public static void HideChilds(this Transform hold, bool y)
        {
            var l = hold.childCount;
            for (byte i = 0; i < l; i++)
                hold.GetChild(i).gameObject.SetActive(!y);
        }


        public static GameObject GetChild(this GameObject parent, int id0)
            => parent.transform.GetChild(id0).gameObject;
        public static GameObject GetChild(this GameObject parent, int id0, int id1)
            => parent.transform.GetChild(id0).GetChild(id1).gameObject;
        public static GameObject GetChild(this GameObject parent, int id0, int id1, int id2)
            => parent.transform.GetChild(id0).GetChild(id1).GetChild(id2).gameObject;
        public static GameObject GetChild(this GameObject parent, int id0, int id1, int id2, int id3)
            => parent.transform.GetChild(id0).GetChild(id1).GetChild(id2).GetChild(id3).gameObject;
        
        
        public static GameObject GetChild(this Transform parent, int id0)
            => parent.GetChild(id0).gameObject;
        public static GameObject GetChild(this Transform parent, int id0, int id1)
            => parent.GetChild(id0).GetChild(id1).gameObject;
        public static GameObject GetChild(this Transform parent, int id0, int id1, int id2)
            => parent.GetChild(id0).GetChild(id1).GetChild(id2).gameObject;
        public static GameObject GetChild(this Transform parent, int id0, int id1, int id2, int id3)
            => parent.GetChild(id0).GetChild(id1).GetChild(id2).GetChild(id3).gameObject;
        

        public static GameObject GetParent(this GameObject go)
            => go.transform.parent.gameObject;
        public static GameObject GetParent2(this GameObject go)
            => go.transform.parent.parent.gameObject;
        public static GameObject GetParent3(this GameObject go)
            => go.transform.parent.parent.parent.gameObject;

        #endregion

        #region Array
        
        
    
        /// <summary>
        /// Увеличить массив на один элемент, с добавлением элемента
        /// </summary>
        /// <param name="arr"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T[] AddElement<T>(this T[] arr, T newElement)
        {
            if (arr == null) arr = new T[0];
            T[] cash = arr;
            var l = cash.Length;
            arr = new T[l+1];
            for (int i = 0; i < l; i++)
                arr[i] = cash[i];
            arr[l] = newElement;
            return arr;
        } 
        /// <summary>
        /// Увеличить массив на один элемент, с добавлением элемента и передачей его ID
        /// </summary>
        /// <param name="arr"></param>
        /// <param name="newElement"></param>
        /// <param name="index"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T[] AddElement<T>(this T[] arr, T newElement, out int index)
        {
            if (arr == null) arr = new T[0];
            T[] cash = arr;
            var l = cash.Length;
            arr = new T[l+1];
            for (int i = 0; i < l; i++)
                arr[i] = cash[i];
            arr[l] = newElement;
            index = l;
            return arr;
        }
        /// <summary>
        /// Уменьшает массив на один элемент
        /// </summary>
        /// <param name="arr"></param>
        /// <param name="removeID"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T[] RemoveElement<T>(this T[] arr, int removeID)
        {
            T[] cash = arr;
            var l = cash.Length;
            arr = new T[l-1];
            int n = 0;
            for (int i = 0; i < l; i++)
            {
                if (i != removeID)
                {
                    arr[n] = cash[i];
                    n++;
                }
            }
            return arr;
        }
        
        /// <summary>
        /// Вставляет эелемент в свобоный слот массива (без расширения)
        /// </summary>
        /// <param name="arr"></param>
        /// <param name="newElement"></param>
        /// <param name="index"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T[] InsertElement<T>(this T[] arr, T newElement, out int index)
        {
            index = 0;
            var l = arr.Length;
            for (int i = 0; i < l; i++)
            {
                if (arr[i] == null)
                {
                    index = i;
                    arr[i] = newElement;
                    break;
                }
            }
            return arr;
        }
        
        
        /// <summary>
        /// Копирует массив
        /// </summary>
        /// <param name="target">копируемый массив</param>
        /// <param name="arr"></param>
        /// <typeparam name="T"></typeparam>
        public static void Copy_To<T>(this T[] target, out T[] arr)
        {
            var l = target.Length;
            arr = new T[l];
            for (int i = 0; i < l; i++)
                arr[i] = target[i];
        }
        /// <summary>
        /// Копирует список
        /// </summary>
        /// <param name="target"></param>
        /// <param name="arr"></param>
        /// <typeparam name="T"></typeparam>
        public static void Copy_To<T>(this List<T> target, out List<T> arr)
        {
            var l = target.Count;
            arr = new List<T>(l);
            for (int i = 0; i < l; i++)
                arr.Add(target[i]);
        }
        /// <summary>
        /// Копирует массив
        /// </summary>
        /// <param name="target">копируемый массив</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T[] Copy_From<T>(this T[] target)
        {
            var l = target.Length;
            T[] arr = new T[l];
            for (int i = 0; i < l; i++)
                arr[i] = target[i];
            return arr;
        }
        
        /// <summary>
        /// Копирует список
        /// </summary>
        /// <param name="target">копируемый массив</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<T> Copy_From<T>(this List<T> target)
        {
            var l = target.Count;
            List<T> arr = new List<T>();
            for (int i = 0; i < l; i++)
                arr.Add(target[i]);
            
            return arr;
        }
        
        /// <summary>
        /// Добавляет список 
        /// </summary>
        /// <param name="target"></param>
        /// <param name="add"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<T> AddElements<T>(this List<T> target, List<T> add)
        {
            var l = target.Count;
            var s = add.Count;
            List<T> arr = new List<T>();
            for (int i = 0; i < l; i++)
                arr.Add(target[i]);
            
            for (int i = 0; i < s; i++)
                arr.Add(add[i]);
            
            return arr;
        }


        /// <summary>
        /// Сортаровка по стоимости (от меньшей)
        /// </summary>
        /// <param name="arr"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<T> SortFromLowCost<T>(this List<T> arr) where T : ISort
        {
            T t;
            var l = arr.Count;
            for (int j = 0; j <= l-2; j++)
            {
                for (int i = 0; i <= l-2; i++)
                {
                    if (arr[i].cost > arr[i + 1].cost)
                    {
                        t = arr[i + 1];
                        arr[i + 1] = arr[i];
                        arr[i] = t;
                    }
                }
            }

            return arr;
        }
        
        /// <summary>
        /// Сортировка от большего к меньшему
        /// </summary>
        /// <param name="arr"></param>
        /// <typeparam name="T"></typeparam>
        public static T[] SortFromBig<T>(this T[] arr) where T: ISort
        {
            T t;
            var l = arr.Length;
            for (int j = 0; j <= l-2; j++)
            {
                for (int i = 0; i <= l-2; i++)
                {
                    if (arr[i].cost < arr[i + 1].cost)
                    {
                        t = arr[i + 1];
                        arr[i + 1] = arr[i];
                        arr[i] = t;
                    }
                }
            }

            return arr;
        }


        /// <summary>
        /// Просто отключает все эелементы в массиве
        /// </summary>
        /// <param name="ar"></param>
        public static void AllElementsOff(this GameObject[] ar)
        {
            var l = ar.Length;
            for (int i = 0; i < l; i++)
                if (ar[i])
                    ar[i].SetActive(false);
        }

        /// <summary>
        /// Найдет первый пустой элемент
        /// </summary>
        /// <param name="ar"></param>
        /// <returns></returns>
        public static int FindEmptyElement(this int[] ar)
        {
            var l = ar.Length;
            for (int i = 0; i < l; i++)
                if (ar[i] == -1)
                    return i;

            return -1;
        } 

        #endregion

        #region Assist
        /// <summary>
        /// Остановит корутину
        /// </summary>
        /// <param name="coroutine"></param>
        /// <param name="owner"></param>
        /// <typeparam name="T"></typeparam>
        public static void Stop<T>(this Coroutine coroutine, T owner) where T: MonoBehaviour
        {
            if(coroutine != null)
                owner.StopCoroutine(coroutine);
        }


        #endregion

        #region Scroll View
        
        public static void ScrollRectResetCenter(this ScrollRect scroll)
        {
            scroll.content.GetRectTr().anchoredPosition = Vector2.zero;     // что бы панель не ловила баг
            scroll.horizontalNormalizedPosition = .5f;
            scroll.verticalNormalizedPosition = .5f;
        }

        /// <summary>
        /// Устанавливает позицию контента 
        /// </summary>
        /// <param name="scroll"></param>
        /// <param name="p">for horizontal = -1</param>
        public static void ScrollRectResetH(this ScrollRect scroll, float p = -1)
        {
            scroll.content.GetRectTr().anchoredPosition = Vector2.zero;     // что бы панель не ловила баг
            scroll.horizontalNormalizedPosition = p;
        }

        /// <summary>
        /// Устанавливает позицию контента 
        /// </summary>
        /// <param name="scroll"></param>
        /// <param name="p">for vertical = 1</param>
        public static void ScrollRectResetV(this ScrollRect scroll, float p = 1)
        {
            scroll.content.GetRectTr().anchoredPosition = Vector2.zero;     // что бы панель не ловила баг
            scroll.verticalNormalizedPosition = p;
        }


        /// <summary>
        /// Задает размер контента по кол-ву елементов
        /// <br/>с учетом VerticalLayoutGroup / HorizontalLayoutGroup (должен быть на content)
        /// </summary>
        public static void SetSizeContentLayoutGroup(
            this ScrollRect scroll,
            bool isVertical,
            Transform root = null,
            bool minSize = false,
            bool active_only = false)
        {
            var content= scroll.content;
            var container = root == null ? content : root;
            var l = container.childCount;
            var n = l;

            if (l == 0) return;

            Vector2 sizeDelta = Vector2.zero;
            
            if(active_only)     // * для подсчета только активных элементов
            {
                n = 0;
                for (int i = 0; i < l; i++)
                {
                    if (container.GetChild(i).gameObject.activeSelf)
                    {
                        n++;
                        sizeDelta += container.GetChild(i).CMP_RectTr().sizeDelta;
                    }
                }
            }
            else
            {
                for (int i = 0; i < l; i++)
                    sizeDelta += container.GetChild(i).CMP_RectTr().sizeDelta;
            }

            //var sizeDelta = container.GetChild(0).GetRectTr().sizeDelta;
            Vector2 applySize = sizeDelta;
            
            if (isVertical)
            {
                var vlg = container.GetComponent<VerticalLayoutGroup>();
                applySize.y += vlg.spacing * n;
                applySize.y += vlg.padding.top;
                applySize.y += vlg.padding.bottom;
                
                if (minSize && applySize.y < scroll.gameObject.CMP_RectTr().rect.height)
                    applySize.y = scroll.gameObject.CMP_RectTr().rect.height;
                applySize.x = scroll.gameObject.CMP_RectTr().rect.width;
            }
            else
            {
                var hlg = container.GetComponent<HorizontalLayoutGroup>();
                applySize.x += hlg.spacing * n;
                applySize.x += hlg.padding.left;
                applySize.x += hlg.padding.right;
                
                if (minSize && applySize.x < scroll.gameObject.CMP_RectTr().rect.width)
                    applySize.x = scroll.gameObject.CMP_RectTr().rect.width;
                applySize.y = scroll.gameObject.CMP_RectTr().rect.height;
            }

            // ======
            content.CMP_RectTr().sizeDelta = applySize;
        }

        /// <summary>
        /// Задает размер контента по кол-ву елементов
        /// <br/>с учетом GridLayoutGroup (должен быть на content)
        /// </summary>
        public static void SetSizeContentGridLayoutGroup(
            this ScrollRect scroll,
            bool isVertical,
            bool innerContainer = false,
            bool minSize = false,
            bool active_only = false)
        {
            var content = scroll.content;
            var container = !innerContainer ? content : content.GetChild(0);

            var grid = container.GetComponent<GridLayoutGroup>();
            if (grid == null)
                return;

            int totalChildren = container.childCount;
            if (totalChildren == 0)
                return;

            int elementCount = 0;

            if (active_only)
            {
                for (int i = 0; i < totalChildren; i++)
                {
                    if (container.GetChild(i).gameObject.activeSelf)
                        elementCount++;
                }
            }
            else
            {
                elementCount = totalChildren;
            }

            if (elementCount == 0)
                elementCount = 1;

            int rows = 0;
            int columns = 0;

            switch (grid.constraint)
            {
                case GridLayoutGroup.Constraint.FixedColumnCount:
                    columns = grid.constraintCount;
                    rows = Mathf.CeilToInt((float)elementCount / columns);
                    break;

                case GridLayoutGroup.Constraint.FixedRowCount:
                    rows = grid.constraintCount;
                    columns = Mathf.CeilToInt((float)elementCount / rows);
                    break;

                case GridLayoutGroup.Constraint.Flexible:
                default:
                    if (isVertical)
                    {
                        float availableWidth = scroll.gameObject.CMP_RectTr().rect.width;
                        float totalCellWidth = grid.cellSize.x + grid.spacing.x;
                        columns = Mathf.Max(1,
                            Mathf.FloorToInt(
                                (availableWidth - grid.padding.left - grid.padding.right + grid.spacing.x)
                                / totalCellWidth));
                        rows = Mathf.CeilToInt((float)elementCount / columns);
                    }
                    else
                    {
                        float availableHeight = scroll.gameObject.CMP_RectTr().rect.height;
                        float totalCellHeight = grid.cellSize.y + grid.spacing.y;
                        rows = Mathf.Max(1,
                            Mathf.FloorToInt(
                                (availableHeight - grid.padding.top - grid.padding.bottom + grid.spacing.y)
                                / totalCellHeight));
                        columns = Mathf.CeilToInt((float)elementCount / rows);
                    }

                    break;
            }

            float width =
                grid.padding.left +
                grid.padding.right +
                (columns * grid.cellSize.x) +
                ((columns - 1) * grid.spacing.x);

            float height =
                grid.padding.top +
                grid.padding.bottom +
                (rows * grid.cellSize.y) +
                ((rows - 1) * grid.spacing.y);

            Vector2 applySize = new Vector2(width, height);

            var parentRect = scroll.gameObject.CMP_RectTr().rect;

            if (isVertical)
            {
                if (minSize && applySize.y < parentRect.height)
                    applySize.y = parentRect.height;

                applySize.x = parentRect.width;
            }
            else
            {
                if (minSize && applySize.x < parentRect.width)
                    applySize.x = parentRect.width;

                applySize.y = parentRect.height;
            }

            content.CMP_RectTr().sizeDelta = applySize;
        }


        /// <summary>
        /// Устанавливает дочерние элементы в ряд с верху вниз
        /// <br/>с учетом размера элемента и возможного личного отступа
        /// <br/>Первый элемент начинает с 0 позиции + возможный отступ
        /// <br/>Для элементов должен быть Anchors = top
        /// <br/>
        /// <br/>!Размер панели не меняет!
        /// <br/>Для имзенения использовать height
        /// <br/>
        /// <br/>*** spacing[] ***
        /// <br/>#1 null без отступа
        /// <br/>#2 spacing.length = 1 число из [0] будет взято для оступа всех элементов
        /// <br/>#3 spacing.length под кол-во элементов, где [i] = -1 отступ элементу не нужен
        /// </summary>
        /// <param name="content"></param>
        /// <param name="height">итоговый размер панели</param>
        /// <param name="spacing"></param>
        public static void SetSizeContentWithChildsV(this Transform content, out float height, float[] spacing = null)
        {
            Vector2 pos = Vector2.zero;
            var qu = 0;

            // * считаем активние кaрточки
            var _l = content.childCount;
            for (int i = 0; i < _l; i++)
                if (content.GetChild(i).gameObject.activeSelf)
                    qu++;
            
            // *** устанавливаем одинаковый отступ для всех панелей (spacing #2)
            if (spacing != null && spacing.Length == 1 && qu > 1)
            {
                var cash = spacing[0];
                spacing = new float[qu];
                for (int i = 0; i < qu; i++)
                    spacing[i] = cash;
            }


            // расстановка
            bool element_0 = false;         // для установки первой активной карточки 
            int element_end = 0;            // id последней активной карточки в расстановке
            var l = content.childCount;
            for (int i = 0; i < l; i++)
            {
                if (!content.GetChild(i).gameObject.activeSelf) continue;

                element_end = i;
                
                if(!element_0)//if (i == 0)
                {
                    element_0 = true;
                    
                    // *** если элемент имеет динамичную высоту, его pivot.y = 1
                    // тогда первый элемент встает по Y = 0
                    // если элемент не меняет размер, его pivot.y = 0.5 и Y = половине его высоты
                    // это нужно что бы первый элемент ровно вставал вначале контента
                    if (content.GetChild(i).GetRectTr().pivot.y != 1)
                        pos.y -= content.GetChild(i).GetRectTr().sizeDelta.y / 2;
                    //
                    
                    
                    if (spacing != null && spacing.Length > i && spacing[i] != -1)
                        pos.y -= spacing[i];

                    pos.x = content.GetChild(i).GetRectTr().anchoredPosition.x;
                    content.GetChild(i).GetRectTr().anchoredPosition = pos;
                }
                else
                {
                    pos.y -= content.GetChild(i - 1).GetRectTr().rect.height;
                    if (spacing != null && spacing.Length > i && spacing[i] != -1)
                        pos.y -= spacing[i];
                    
                    pos.x = content.GetChild(i).GetRectTr().anchoredPosition.x;
                    content.GetChild(i).GetRectTr().anchoredPosition = pos;
                }
            }

            // итоговый размер панели берется по координатам последней активной карточки 
            height = Mathf.Abs(content.GetChild(element_end).transform.GetRectTr().anchoredPosition.y);
            height += content.GetChild(element_end).GetRectTr().rect.height;
        }

        /// <summary>
        /// Координаты для след. элемента в ряду (cur element coord+size)
        /// <br/>(*для ручной расстановки элементов сверху-вниз/слева-направо внутри панели)
        /// </summary>
        /// <param name="tr"></param>
        /// <returns></returns>
        public static Vector2 GetCoordInRow(this Transform tr)
        {
            var coord = tr.localPosition;
            coord.x += tr.GetRectTr().sizeDelta.x;
            coord.y -= tr.GetRectTr().sizeDelta.y;
            return coord;
        }

        #endregion

        #region Mouse

        public static bool LeftClick(this PointerEventData eventData)
            => eventData.button == PointerEventData.InputButton.Left;
        public static bool RightClick(this PointerEventData eventData)
            => eventData.button == PointerEventData.InputButton.Right;
        public static bool MiddleClick(this PointerEventData eventData)
            => eventData.button == PointerEventData.InputButton.Middle;
    
        #endregion

        #region GetPosition
        /// <summary>
        /// Координаты курсора на экране
        /// </summary>
        /// <param name="camera"></param>
        /// <returns></returns>
        public static Vector3 GetMouseWorldPosZ(this Camera camera)
        {
            Vector3 v = Vector3.zero;
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_STANDALONE_OSX
            v = camera.ScreenToWorldPoint(Input.mousePosition); 
#else
            v = camera.ScreenToWorldPoint(Input.touches[0].position);
#endif
            v.z = 0;
            return v;
        }
        
        public static Vector3 GetMouseWorldPosZ(this Camera camera, int touchID)
        {
            Vector3 v = Vector3.zero;
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_STANDALONE_OSX
            v = camera.ScreenToWorldPoint(Input.mousePosition); 
#else
            v = camera.ScreenToWorldPoint(Input.touches[touchID].position);
#endif
            v.z = 0;
            return v;
        }

        #endregion

        #region Progress Bar

        /// <summary>
        /// Прогресс бар для sprite/quad
        /// <br/>Объект должен быть обычного размера и в 0 coord
        /// </summary>
        /// <param name="go"></param>
        /// <param name="currVal"></param>
        /// <param name="maxVal"></param>
        public static void FillBar(this GameObject go, float currVal, float maxVal)
        {
            var fill = (currVal%10)/maxVal;
            Vector3 v = new Vector3(fill, 1,1);
            go.transform.localScale = v;
            v.x = (fill-1)/2;
            v.y = v.z = 0; 
            go.transform.localPosition = v; 
        }

        #endregion

        #region GUI


        /// <summary>
        /// Устанавоивает прозрачность для цвета
        /// </summary>
        /// <returns></returns>
        public static Color SetAlpha(this Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }

        /// <summary>
        /// Установление прозрачности для картинки
        /// </summary>
        /// <param name="img"></param>
        /// <param name="alpha"></param>
        public static void SetAlpha(this Image img, float alpha)
        {
            var c = img.color;
            c.a = alpha;
            img.color = c;
        }
        /// <summary>
        /// Управление прозрачностью и blocksRaycasts
        /// </summary>
        /// <param name="element"></param>
        /// <param name="show"></param>
        public static void SetCanvasGroup(this GameObject element, bool show)
        {
            if (!element.GetComponent<CanvasGroup>())
            {
                Debug.LogError($"На объекте < {element} > нет компонента CanvasGroup!");
                return;
            }
            element.GetComponent<CanvasGroup>().alpha = show ? 1 : 0;
            element.GetComponent<CanvasGroup>().blocksRaycasts = show;
        }
        
        /// <summary>
        /// Установление позиции элемента в канвасе
        /// </summary>
        /// <param name="element"></param>
        /// <param name="coord"></param>
        public static void SetUIPosition(this GameObject element, Vector3 coord)
        {
            element.GetComponent<RectTransform>().anchoredPosition = coord;
        }
        /// <summary>
        /// Установление позиции элемента в канвасе from world coord
        /// </summary>
        /// <param name="element"></param>
        /// <param name="coord"></param>
        public static void SetUIPositionScene(this GameObject element, Vector3 worldCoord)
        {
            element.transform.position = Camera.main.WorldToScreenPoint(worldCoord);
        }
        
        
        /// <summary>
        /// Установление размера
        /// </summary>
        /// <param name="element"></param>
        /// <param name="size"></param>
        public static void SetUISize(this GameObject element, Vector2 size) 
            => element.GetComponent<RectTransform>().sizeDelta = size;

        /// <summary>
        /// Установление размера
        /// </summary>
        /// <param name="element"></param>
        /// <param name="size"></param>
        public static void SetUISize(this Transform element, Vector2 size) 
            => element.GetComponent<RectTransform>().sizeDelta = size;
        
        /// <summary>
        /// Изменение цвета
        /// </summary>
        public static string SetText(this string s, string color)
            => $"<color=#{color}>{s}</color>";

        /// <summary>
        /// Изменение цвета
        /// </summary>
        public static string SetText(this string s, EDlogColor color)
            => $"<color={DLog.color[(byte)color]}>{s}</color>";
        
        /// <summary>
        /// Изменения размера
        /// </summary>
        public static string SetText(this string s, int size)
            => $"<size={size}>{s}</size>";
        
        /// <summary>
        /// Изменение цвета и размера
        /// </summary>
        public static string SetText(this string s, string color, int size)
            => $"<color=#{color}><size={size}>{s}</size></color>";

        
        
        /// <summary>
        /// Установить значение для шейдера (FLASH) image canvas
        /// </summary>
        /// <param name="img"></param>
        /// <param name="flash_value"></param>
        public static void SetShaiderFlash(this Image img, float flash_value)
        {
            var f = new F();
            var clone = f.GetMaterial(img.material);
            clone.SetFloat("_FlashAmount", flash_value);
            img.material = clone;
        }
        
        /// <summary>
        /// Установить значение для шейдера (FLASH + COLOR) image canvas
        /// </summary>
        /// <param name="img"></param>
        /// <param name="flash_value"></param>
        /// <param name="color"></param>
        public static void SetShaiderFlash(this Image img, float flash_value, Color color)
        {
            var f = new F();
            var clone = f.GetMaterial(img.material);
            clone.SetFloat("_FlashAmount", flash_value);
            clone.SetColor("_FlashColor", color);
            img.material = clone;
        }
        
        /// <summary>
        /// Установить значение для шейдера (FLASH + COLOR) image canvas
        /// </summary>
        /// <param name="img"></param>
        /// <param name="flash_value"></param>
        /// <param name="color"></param>
        public static void SetShaiderFlash(this SpriteRenderer spriteRenderer, float flash_value, Color color)
        {
            var f = new F();
            var clone = f.GetMaterial(spriteRenderer.material);
            clone.SetFloat("_FlashAmount", flash_value);
            clone.SetColor("_FlashColor", color);
            spriteRenderer.material = clone;
        }
        
        
        #endregion

        #region GET
        
        /// <summary>
        /// Return color from string
        /// </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
        public static Color Hex(this string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out var color);
            return color;
        }

        /// <summary>
        /// Получение центра экрана устройства
        /// </summary>
        /// <param name="coord"></param>
        /// <returns></returns>
        public static Vector2 GetCenterScreen(this Vector2 coord)
        {
            coord.x = Screen.width / 2;
            coord.y = Screen.height / 2;
            return coord;
        }

        /// <summary>
        /// Получает компонент RectTransform
        /// </summary>
        public static RectTransform GetRectTr(this GameObject g) => g.GetComponent<RectTransform>();
        
        /// <summary>
        /// Получает компонент RectTransform
        /// </summary>
        public static RectTransform GetRectTr(this Transform g) => g.GetComponent<RectTransform>();

        
        /// <summary>
        /// Получает компонент CoreBtn
        /// </summary>
        /// <param name="g"></param>
        /// <returns></returns>
        public static CoreBtn GetBtn(this GameObject g) => g.GetComponent<CoreBtn>();
        
        /// <summary>
        /// Получает компонент CoreBtn
        /// </summary>
        /// <param name="g"></param>
        /// <returns></returns>
        public static CoreBtn GetBtn(this Transform g) => g.GetComponent<CoreBtn>();

        #endregion

        #region SET

        public static void ButtonSetInteractable(this GameObject button, bool interactable)
            => button.GetComponent<BaseUIButton>().SetInteractable(interactable);
        
        public static void RegisterButtonClick(this GameObject button, UnityAction a, DFuncResponse onLock = null)
        {
            var b = button.GetComponent<BaseUIButton>();
            b.events.onClick.RemoveAllListeners();
            b.events.onClick.AddListener(a);
            b.onLock = onLock;
        }
        public static void RegisterButtonDoubleClick(this GameObject button, UnityAction a, DFuncResponse onLock = null)
        {
            var b = button.GetComponent<BaseUIButton>();
            b.events.onDoubleClick.RemoveAllListeners();
            b.events.onDoubleClick.AddListener(a);
            b.onLock = onLock;
        }
        public static void RegisterButtonLongPress(this GameObject button, UnityAction a, DFuncResponse onLock = null)
        {
            var b = button.GetComponent<BaseUIButton>();
            b.events.onLongPress.RemoveAllListeners();
            b.events.onLongPress.AddListener(a);
            b.onLock = onLock;
        }
        public static void RegisterButtonHold(this GameObject button, UnityAction a, DFuncResponse onLock = null)
        {
            var b = button.GetComponent<BaseUIButton>();
            b.events.onHold.RemoveAllListeners();
            b.events.onHold.AddListener(a);
            b.onLock = onLock;
        }


        public static void ButtonSetText(this GameObject button, string text)
        {
            button.GetChild(0).CMP_Text().text = text;
        }




        /// <summary>
        /// Подписывается на событие кнопки (_event)
        /// <br/>Для нескольких подписчиков
        /// </summary>
        /// <param name="g"></param>
        /// <param name="a"></param>
        public static void EventBtn_old(this GameObject g, UnityAction a) => g.GetComponent<CoreBtn>()._event.AddListener(a);
        
        /// <summary>
        /// Подписывается на событие кнопки (_event)
        /// <br/>Для одной подписки
        /// </summary>
        /// <param name="g"></param>
        /// <param name="a"></param>
        public static void EventBtnOne_old(this GameObject g, UnityAction a)
        {
            g.GetComponent<CoreBtn>()._event.RemoveAllListeners();
            g.GetComponent<CoreBtn>()._event.AddListener(a);
        }


        
        /// <summary>
        /// Оставляет нужное кол-во дочерних объектов видимыми
        /// </summary>
        /// <param name="box"></param>
        /// <param name="number"></param>
        public static void QuBox(this GameObject holder, int number, bool reverse)
        {
            var l = holder.transform.childCount;

            if (reverse)
            {
                for (int i = l - 1; i >= 0; i--)
                    holder.GetChild(i).SetActive((l - 1)-i < number);
            }
            else
            {
                for (int i = 0; i < l; i++)
                    holder.GetChild(i).SetActive(i < number);
            }
        }
        
        /// <summary>
        /// Оставляет нужное кол-во дочерних объектов видимыми
        /// <br/>остальные делает прозрачынми
        /// </summary>
        /// <param name="box"></param>
        /// <param name="number"></param>
        public static void QuBoxAlpha(this GameObject holder, int number, bool reverse, float alpha = .5f)
        {
            var l = holder.transform.childCount;
            Color col;
            col = holder.GetChild(0).GetComponent<Image>().color;

            if (reverse)
            {
                for (int i = l - 1; i >= 0; i--)
                {
                    col.a = (l - 1) - i < number ? 1 : alpha;
                    holder.GetChild(i).GetComponent<Image>().color = col;
                }
            }
            else
            {
                for (int i = 0; i < l; i++)
                {
                    col.a = i < number ? 1 : alpha;
                    holder.GetChild(i).GetComponent<Image>().color = col;
                }
            }
        }
        

        #endregion

        #region GET COMPONENT

        /// <summary>
        /// Получение компонента TextMeshProUGUI
        /// </summary>
        /// <param name="g"></param>
        /// <returns></returns>
        public static TMP_Text CMP_Text(this GameObject g) => g.GetComponent<TMP_Text>();
        
        /// <summary>
        /// Получение компонента Image
        /// </summary>
        /// <param name="g"></param>
        /// <returns></returns>
        public static Image CMP_Image(this GameObject g) => g.GetComponent<Image>();
        
        /// <summary>
        /// Получение компонента CoreBtn
        /// </summary>
        /// <param name="g"></param>
        /// <returns></returns>
        public static BaseUIButton CMP_Btn(this GameObject g) => g.GetComponent<BaseUIButton>();
        
        /// <summary>
        /// Получение компонента CanvasGroup
        /// </summary>
        /// <param name="g"></param>
        /// <returns></returns>
        public static CanvasGroup CMP_CG(this GameObject g) => g.GetComponent<CanvasGroup>();
        
        /// <summary>
        /// Получение компонента SpriteRenderer
        /// </summary>
        /// <param name="g"></param>
        /// <returns></returns>
        public static SpriteRenderer CMP_SpriteR(this GameObject g) => g.GetComponent<SpriteRenderer>();
        
        /// <summary>
        /// Получение компонента Animator
        /// </summary>
        /// <param name="g"></param>
        /// <returns></returns>
        public static Animator CMP_Animator(this GameObject g) => g.GetComponent<Animator>();
        
        
        
        
        
        
        /// <summary>
        /// Получение компонента RectTransform
        /// </summary>
        /// <param name="g"></param>
        /// <returns></returns>
        public static RectTransform CMP_RectTr(this GameObject g) => g.GetComponent<RectTransform>();
        
        /// <summary>
        /// Получение компонента RectTransform
        /// </summary>
        /// <param name="g"></param>
        /// <returns></returns>
        public static RectTransform CMP_RectTr(this Transform g) => g.GetComponent<RectTransform>();

        
        
        #endregion
        
        #region JSON
        
        /// <summary>
        /// Выгружает
        /// </summary>
        /// <param name="file"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T[] FromJson<T>(this string file)
        {
            string data = File.ReadAllText(Path.Combine(Application.streamingAssetsPath, $"{file}.json"));
            Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(data);
            return wrapper.items;
        }
        /// <summary>
        /// Сохраняет
        /// </summary>
        /// <param name="array"></param>
        /// <param name="file"></param>
        /// <typeparam name="T"></typeparam>
        public static void ToJson<T>(this T[] array, string file)
        {
            Wrapper<T> wrapper = new Wrapper<T>();
            wrapper.items = array;
            var data = JsonUtility.ToJson(wrapper);
            File.WriteAllText(Path.Combine(Application.streamingAssetsPath, $"{file}.json"), data);
        }

        [Serializable]
        private class Wrapper<T>
        {
            public T[] items;
        }

        #endregion
    }

    public interface ISort
    {
        float cost { set; get; }
    }

    public interface IRandom
    {
        float Chance { get; }
    }

    public interface IArrMerge
    {
        int ID { set; get; }
        int amount { set; get; }
    }




    public class F : MonoBehaviour
    {
        public void RemoveObj(GameObject obj)
        {
            Destroy(obj.gameObject);
        }

        public void RemoveObjEditor(Transform obj)
        {
            DestroyImmediate(obj.gameObject);
        }


        public GameObject Instance(GameObject prefab, Transform parent) 
            => Instantiate(prefab, parent,false);

        public GameObject Instance(string asset, Transform parent)
            => Instantiate(Resources.Load(asset), parent,false)as GameObject;

        public Material GetMaterial(Material material) => Instantiate(material);
    }
}