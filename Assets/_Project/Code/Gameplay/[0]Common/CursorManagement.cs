using System;
using Galactic1;
using UnityEngine;

namespace Galactic1
{
    /*
     *   Отвечает за изменение курсора при наведении на разные объекты
     */
    public class CursorManagement : Singleton<CursorManagement>
    {
        
        [SerializeField] private CursorResolution _regular;
        [SerializeField] private CursorResolution _drag;
        [SerializeField] private CursorResolution _drop;
        [SerializeField] private CursorResolution _attack;
        
        
        private Texture2D regular, drag, drop, attack;

        [HideInInspector]
        public ECursorMode mode;
        
        
        
        private Vector3 mouseCoord;
        private Vector2Int mouseCoordInt;
        private Vector2 hotSpot = Vector2.zero;





        void Awake()
        {
            //Debug.Log(Screen.width+" width");
            if (Screen.width < 2000)
            {
                regular = _regular.small;
                drag = _drag.small;
                drop = _drop.small;
                attack = _attack.small;
            }
            else if (Screen.width < 2600)
            {
                regular = _regular.mid;
                drag = _drag.mid;
                drop = _drop.mid;
                attack = _attack.mid;
            }
            
            //SetCursor(ECursorSprite.regular);     // для установки спрайта курсора
        }
        


        #region Coord Cursor

        /// <summary>
        /// Вернет поз курсора (for scene)
        /// </summary>
        /// <returns></returns>
        public Vector3 CursorCoord() => CameraControllerOld.I.GameCamera.GetMouseWorldPosZ();
        
        /// <summary>
        /// Вернет поз курсора строго INT (for scene)
        /// </summary>
        /// <returns></returns>
        public Vector2Int CursorCoordInt(Vector2 offset)
        {
            mouseCoord = CameraControllerOld.I.GameCamera.GetMouseWorldPosZ();
            mouseCoordInt.x = Mathf.FloorToInt(mouseCoord.x+offset.x);
            mouseCoordInt.y = Mathf.FloorToInt(mouseCoord.y+offset.y);
            return mouseCoordInt;
        }
        /// <summary>
        /// Вернет поз курсора + offset
        /// </summary>
        /// <returns></returns>
        public Vector2 CursorCoordOffset()
        {
            mouseCoord = CameraControllerOld.I.GameCamera.GetMouseWorldPosZ();
            mouseCoord.x = Mathf.FloorToInt(mouseCoord.x)+.5f;
            mouseCoord.y = Mathf.FloorToInt(mouseCoord.y)+.5f;
            return mouseCoordInt;
        }

        #endregion



        #region Sprite Cursor

        public void SetCursor(ECursorSprite type)
        {
            Texture2D tex = regular;
            switch (type)
            {
                case ECursorSprite.drag:
                    tex = drag;
                    break;
                case ECursorSprite.drop:
                    tex = drop;
                    break;
                case ECursorSprite.attack:
                    tex = attack;
                    break;
                
                dafault:
                    tex = regular;
            }
            
            DLog.Alert($"Select cursor {type}");
            Cursor.SetCursor(tex, hotSpot, CursorMode.Auto);
        }

        #endregion

        
    }

    public enum ECursorMode
    {
        regular, command
    }
    
    public enum ECursorSprite
    {
        regular, 
        drag,
        drop, 
        attack
    }

    [System.Serializable]
    public struct CursorResolution
    {
        public Texture2D small, mid;
    }
}