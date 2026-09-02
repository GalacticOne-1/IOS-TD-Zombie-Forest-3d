using System;
using Galactic1.Core.Systems.GameLoopSession;
using UnityEngine;

namespace Galactic1.Code.WorldMap
{
    /// <summary>
    /// Управление визуальной иконкой транспорта.
    /// </summary>
    public class VehicleIconController : MonoBehaviour
    {
        [SerializeField] private Transform vehicleTransform;
        [SerializeField] private float moveSpeed = 5f;

        private WorldMapService mapService;
        private MapNode targetNode;
        
        private Vector3 targetPosition;
        private bool isMoving;
        private GameObject modelInstance;

        public bool IsMoving => isMoving;
        
        
        public event Action OnMoveStarted;
        public event Action OnMoveFinished;


        /// <summary>
        /// Привязка сервиса карты
        /// </summary>
        public void Bind(WorldMapService service)
        {
            mapService = service;

            var prefab = ServiceLocator.Current.Get<GameSession>().GameLoopContext.PlayerTransport.GetPrefab();
            modelInstance = $"{AppConstants.PATH_ENTITIES}{prefab}".CreateGO(vehicleTransform);
            modelInstance.transform.localScale = Vector3.one * .35f;
            modelInstance.transform.localPosition = Vector3.zero;
            modelInstance.SetActive(false);
        }
        

        /// <summary>
        /// Запуск движения к ноде
        /// </summary>
        public void MoveTo(MapNode node)
        {
            if (node == null || mapService == null)
                return;

            targetNode = node;
            targetPosition = node.transform.position;
            
            // поворот транспорта к цели
            Vector3 direction = targetPosition - vehicleTransform.position;
            direction.y = 0f; 
            vehicleTransform.rotation = Quaternion.LookRotation(direction);
            modelInstance.SetActive(true);
            
            isMoving = true;
            OnMoveStarted?.Invoke();
        }

        private void Update()
        {
            if (!isMoving) return;

            vehicleTransform.position = Vector3.MoveTowards(
                vehicleTransform.position, 
                targetPosition, 
                moveSpeed * Time.deltaTime);
            
            
            if (Vector3.Distance(vehicleTransform.position, targetPosition) < 0.01f)
            {
                isMoving = false;
                modelInstance.SetActive(false);
                
                // Меняем текущую ноду в сервисе
                mapService.SetCurrentNode(targetNode);
                targetNode = null;
                OnMoveFinished?.Invoke();
            }
        }
    }
}