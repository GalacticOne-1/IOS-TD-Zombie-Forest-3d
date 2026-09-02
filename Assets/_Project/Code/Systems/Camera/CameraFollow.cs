using System;
using Galactic1.Repository;
using Galactic1;
using Galactic1.AbstractFactory;
using UnityEngine;
using UnityEngine.Serialization;

namespace Galactic1
{
    public class CameraFollow : Singleton<CameraFollow>, IUpdate, IFixedUpdate
    {

        [SerializeField] private bool applyBorder;
        [SerializeField] private Vector2 cameraBorder;

        public float SetBorder_X(float val) => cameraBorder.x = val;
        public float SetBorder_Y(float val) => 0;//=> cameraBorder.y = val; 
        
        public float smoothSpeed = 0.125f;
        public Vector3 offset;
        private Vector3 desiredPosition;
        private Transform tr;

        private Transform objToFollow;

        public Transform ObjToFollow => objToFollow;
        public void SetObjectForFollowing(GameObject obj) => objToFollow = obj.transform;

        
        public Vector2 currPos => tr.position;

        private Vector3 velocity = Vector3.zero;

        public bool STOP;
        
        
        
        public void Activator()
        {
            tr = transform;
            objToFollow = ServiceLocator.Current.Get<PlayerRepository>().GetController.Tr;
            tr.position = objToFollow.position + offset;
            ServiceLocator.Current.Get<MonoBehaviourMaster>().update.Add(this);
            ServiceLocator.Current.Get<MonoBehaviourMaster>().fixedUpdate.Add(this);
        }

        public void IUpdateClear()
        {
            DLog.Alert("Update clear : cameraFollow", EDlogColor.YELLOW, AppConstants.show_log_scene_clear_event);
            ServiceLocator.Current.Get<MonoBehaviourMaster>().update.Remove(this);
            ServiceLocator.Current.Get<MonoBehaviourMaster>().fixedUpdate.Remove(this);
        }
        
        public void UpdateM(){}
        public void FixedUpdateM()  // FixedUpdate << обязательно если юнит управляется через Rigidbody
        {
            if (STOP) return;
            //if (ServiceLocator.Current.Get<Bootstrap>().STATE != EGameState.LEVEL_PLAY) return;
            
            desiredPosition = objToFollow.position + offset;
            desiredPosition.z = -10;

            if (applyBorder)
            {
                // border
                desiredPosition.x = Mathf.Clamp(desiredPosition.x + tr.position.x, -cameraBorder.x, cameraBorder.x);
                desiredPosition.x -= tr.position.x;
                desiredPosition.y = Mathf.Clamp(desiredPosition.y + tr.position.y, -cameraBorder.y, cameraBorder.y);
                desiredPosition.y -= tr.position.y;
            }

            
            tr.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
            //tr.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothSpeed);

            //tr.LookAt(HUBLink.player.tr);
        }


        public void ResetPos()
        {
            desiredPosition = Vector3.zero;
            desiredPosition.z = -10;
            tr.position = desiredPosition;
        }


        public void SetPosition(Vector3 coord) => tr.position = coord + offset;

    }
}


