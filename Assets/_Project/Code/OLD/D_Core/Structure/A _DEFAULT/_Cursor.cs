using Galactic1;
using UnityEngine;

namespace Galactic1
{

    public static class CURSOR
    {

        private static Vector3 cashCameraPos, cashCursorPos;
        

        // call from update, over Input.MouseButtonDown 
        public static void CashPosition()
        {
            cashCameraPos = CameraControllerOld.I.GameCamera.transform.position;
            cashCursorPos = CameraControllerOld.I.GameCamera.GetMouseWorldPosZ();
        }
        
        
        /// <summary>
        /// true - экран не двигался 
        /// </summary>
        /// <returns></returns>
        public static bool ScreenFrozen()
        {
            var coord = cashCameraPos - CameraControllerOld.I.GameCamera.transform.position;
            var coord2 = cashCursorPos - CameraControllerOld.I.GameCamera.GetMouseWorldPosZ();

            return Mathf.Abs(coord.x) < .3f &&
                   Mathf.Abs(coord.y) < .3f &&
                   Mathf.Abs(coord2.x) < .3f &&
                   Mathf.Abs(coord2.y) < .3f;
        }
        
        /// <summary>
        /// true - курсор не двигался
        /// </summary>
        /// <returns></returns>
        public static bool CursorFrozen()
        {
            var coord = cashCursorPos - CameraControllerOld.I.GameCamera.GetMouseWorldPosZ();
            return Mathf.Abs(coord.x) < .1f && Mathf.Abs(coord.y) < .1f;
        }
    } 
}