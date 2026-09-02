using System.Collections.Generic;
using Galactic1;
using UnityEngine;

namespace Galactic1
{
    public class HUBObjects
    {
        /*
         *    ЦУ для создания любых объектов в игровой сцене и доступ к ним
         *     (юниты, эффекты и пр) все должно проходить через эту систему
         * 
         */

        // (1t1 with save[])
        /*private static RoomObj[] room = new RoomObj[0];
        /// <summary>
        /// Получить комнату по ее индексу 
        /// </summary>
        public static RoomObj GetRoom(int id) => id < room.Length ? room[id] : null;
        /// <summary>
        /// Получить ассет комнаты
        /// </summary>
        /// <param name="floor"></param>
        /// <returns></returns>
        public static AssetRoom GetRoomAsset(int floor)
            => LibController.I.room[room[floor].assetCAT].list[room[floor].assetID];
        public static int countFloor => room.Length;
        static List<int> appart = new List<int>();
        
        
        // unit
        private static UnitObj[] unit = new UnitObj[0];
        public static void SetArUnit(int amount) => unit = new UnitObj[amount];
        public static int countUnits => unit.Length;
        public static UnitObj GetUnit(int id) => unit[id];*/
        
        

        // -------------- ^ для доступа ^


        #region ROOM

        /// <summary>
        /// Для добавления комнаты в массив
        /// </summary>
        /// <param name="Data"></param>
        // public static void AddRoom(RoomObj Data)
        // {
        //     room = room.AddElement(Data);
        //     if(Data.appart)
        //         appart.Add(Data.ID);
        // }
        //
        // /// <summary>
        // /// Для замены комнаты
        // /// </summary>
        // /// <param name="newRoom"></param>
        // public static void ChangeRoom(int id, RoomObj newRoom)
        // {
        //     room[id] = newRoom;
        //     if(newRoom.appart)
        //         appart.Add(newRoom.ID);
        // }
        //
        // /// <summary>
        // /// Вернет рабочую комнату
        // /// </summary>
        // /// <returns></returns>
        // public static int GetRandomRoom()
        // {
        //     if (room.Length == 1) return -1;
        //     
        //     var r = Random.Range(1, countFloor);
        //     if (!room[r].available || room[r].appart)
        //     {
        //         var l = room.Length;
        //         for (int i = 1; i < l; i++)
        //         {
        //             if (room[i].available && !room[i].appart)
        //                 return i;
        //             
        //         }
        //
        //         return -1;
        //     }
        //
        //     
        //     return r;
        // }
        //
        // /// <summary>
        // /// Вернет случайные аппартаменты
        // /// </summary>
        // /// <returns></returns>
        // public static int GetAppartRoom()
        // {
        //     if (appart.Count == 0) return -1;
        //     
        //     return appart[Random.Range(0, appart.Count)];
        // }
        //
        //
        // /// <summary>
        // /// Посчитает кол-во комнат по категориям
        // /// </summary>
        // /// <returns></returns>
        // public static int[] GetQuRoom()
        // {
        //     int[] qu = new int[6];
        //     var l = countFloor;
        //     for (int i = 1; i < l; i++)
        //     {
        //         if (room[i].assetCAT > 0)
        //             qu[room[i].assetCAT - 1]++;
        //     }
        //
        //     return qu;
        // }

        #endregion



        #region UNIT    
        
        /// <summary>
        /// Добавление юнита из сохранеия
        /// </summary>
        /// <param name="Data"></param>
        /// <param name="index"></param>
        // public static void AddUnit(UnitObj Data, short index)
        // {
        //     unit[index] = Data;
        //     unit[index].ID = index;
        // }
        //
        // /// <summary>
        // /// Для добавления unit в массив
        // /// </summary>
        // /// <param name="Data"></param>
        // public static void AddUnit(UnitObj Data, out short index)
        // {
        //     var l = unit.Length;
        //     for (short i = 0; i < l; i++)        
        //     {
        //         if (unit[i] == null)        // find free slot
        //         {
        //             unit[i] = Data;
        //             unit[i].ID = i;
        //             index = i;
        //             //DLog.Alert($"A {index}","yellow");
        //             return;
        //         }
        //
        //         //Debug.Log("Unit", unit[i].gameObject);
        //
        //     }
        //     
        //     // or add
        //     unit = unit.AddElement(Data);
        //     unit[unit.Length - 1].ID = (short)(unit.Length - 1);
        //     index = (short)(unit.Length - 1);
        //     //DLog.Alert($"B {index}","yellow");
        // }
        //
        // /// <summary>
        // /// Удалить юнит
        // /// </summary>
        // /// <param name="newRoom"></param>
        // public static void RemoveUnit(int id)
        // {
        //     unit[id] = null;
        // }
        

        #endregion
    }
}