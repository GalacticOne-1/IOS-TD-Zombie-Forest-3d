
using Galactic1.Code.Systems;
using UniRx;
using UnityEngine;

namespace Galactic1.Code.Cameras
{
    public class RTSCameraController : MonoBehaviour, IGameService, IUpdate
    {
        [Header("Movement BasicSettings")]
        [SerializeField] private float moveSpeed = 10f;
        [SerializeField] private float edgeScrollSpeed = 15f;
        [SerializeField] private float dragSpeed = .2f;
        
        [Header("Inertia BasicSettings")]
        [SerializeField] private float inertiaDamping = 5f;
        [SerializeField] private float minInertiaSpeed = 0.05f;
        
        [Header("Zoom BasicSettings")]
        [SerializeField] private float zoomSpeed = 5f;
        [SerializeField] private float minZoom = 3f;
        [SerializeField] private float maxZoom = 10f;
        
        [Header("Edge Scrolling")]
        [SerializeField] private bool enableEdgeScrolling = true;
        [SerializeField] private float edgeScrollBorder = 20f;
        
        [Header("Camera Bounds")]
        [SerializeField] private Vector3 startPosition;
        [SerializeField] private bool useCameraBounds = true;
        [SerializeField] private Vector2 minBounds = new Vector2(-50f, -50f);
        [SerializeField] private Vector2 maxBounds = new Vector2(50f, 50f);
        
        [Header("Input BasicSettings")]
        [SerializeField] private KeyCode dragButton = KeyCode.Mouse2;
        [SerializeField] private bool invertDrag = false;


        private UIDetector _uiDetector;
        private Camera cam;
        private Vector3 velocity;
        private Vector3 lastMousePosition;
        private bool isDragging;
        private Transform tr;
        
        public void ClearDragging() => isDragging = false;
        public ReactiveProperty<bool> Freeze { get; set; }
        public DFuncResponse OnFreeze;
        
        
        
        public Camera Camera 
        {
            get
            {
                if (cam == null)
                {
                    cam = gameObject.GetComponentInChildren<Camera>();
                }

                return cam;
            }
        }
        
        public void Activator()
        {
            cam = gameObject.GetComponentInChildren<Camera>();
            if (cam == null)
            {
                Debug.LogError("RTSCameraController requires a Camera component!");
                enabled = false;
                return;
            }

            _uiDetector = ServiceLocator.Current.Get<UIDetector>();

            Freeze = new();
            Freeze.Skip(1).Subscribe(_ =>
            {
                isDragging = false;
                lastMousePosition = Input.mousePosition;
                velocity = Vector3.zero;
            });
            
            tr = transform;
            tr.position = startPosition;
            ServiceLocator.Current.Get<MonoBehaviourMaster>().update.Add(this);
        }


        public void IUpdateClear()
            => ServiceLocator.Current.Get<MonoBehaviourMaster>().update.Remove(this);
        
        public void UpdateM()
        {
            if (CanProcessMouseInput())
            {
                //HandleKeyboardInput();
                HandleMouseInput();
                //HandleZoom();
                ApplyInertia();
                ApplyMovement();
            }
        }
        
        
        private bool CanProcessMouseInput()
        {
            if (Freeze.Value)
                return false;
            
            var f = false;
            if (OnFreeze != null)
                f = OnFreeze.Invoke();
            if (velocity == Vector3.zero && f)
                return false;
            
            if (isDragging || velocity != Vector3.zero) 
                return true;
            
            // Проверяем, есть ли UI под курсором
            return !_uiDetector.IsPointerOverUI && !_uiDetector.HasUIUnderCursor();
        }
        
        private void HandleKeyboardInput()
        {
            Vector3 inputVector = Vector3.zero;
            
            // WASD and Arrow key movement
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
                inputVector.y += 1f;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
                inputVector.y -= 1f;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
                inputVector.x -= 1f;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
                inputVector.x += 1f;
            
            if (inputVector != Vector3.zero)
            {
                velocity += inputVector.normalized * moveSpeed * Time.deltaTime;
            }
        }
        
        private void HandleMouseInput()
        {
            //HandleEdgeScrolling();
            HandleMouseDrag();
        }
        
        private void HandleEdgeScrolling()
        {
            if (!enableEdgeScrolling) return;
            
            Vector3 mousePosition = Input.mousePosition;
            Vector3 edgeVector = Vector3.zero;
            
            // Check screen edges
            if (mousePosition.x <= edgeScrollBorder)
                edgeVector.x -= 1f;
            if (mousePosition.x >= Screen.width - edgeScrollBorder)
                edgeVector.x += 1f;
            if (mousePosition.y <= edgeScrollBorder)
                edgeVector.y -= 1f;
            if (mousePosition.y >= Screen.height - edgeScrollBorder)
                edgeVector.y += 1f;
            
            if (edgeVector != Vector3.zero)
            {
                velocity += edgeVector.normalized * edgeScrollSpeed * Time.deltaTime;
            }
        }
        
        private void HandleMouseDrag()
        {
            if (Input.GetKeyDown(dragButton))
            {
                isDragging = true;
                lastMousePosition = Input.mousePosition;
            }
            
            if (Input.GetKeyUp(dragButton))
            {
                isDragging = false;
            }
            
            if (isDragging)
            {
                Vector3 currentMousePosition = Input.mousePosition;
                Vector3 mouseDelta = lastMousePosition - currentMousePosition;
                
                // Convert screen space to world space movement
                Vector3 worldDelta = cam.ScreenToWorldPoint(new Vector3(mouseDelta.x, mouseDelta.y, cam.nearClipPlane));
                worldDelta = worldDelta - cam.ScreenToWorldPoint(Vector3.zero);
                
                if (invertDrag)
                    worldDelta = -worldDelta;
                
                velocity += worldDelta * dragSpeed;
                lastMousePosition = currentMousePosition;
            }
        }
        
        private void HandleZoom()
        {
            float scrollInput = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scrollInput) > 0.01f)
            {
                float newSize = cam.orthographicSize - scrollInput * zoomSpeed;
                cam.orthographicSize = Mathf.Clamp(newSize, minZoom, maxZoom);
            }
        }
        
        private void ApplyInertia()
        {
            // Apply damping to create inertia effect
            velocity = Vector3.Lerp(velocity, Vector3.zero, inertiaDamping * Time.fixedDeltaTime);
            
            // Stop very small movements to prevent infinite tiny movements
            if (velocity.magnitude < minInertiaSpeed)
            {
                velocity = Vector3.zero;
            }
        }
        
        private void ApplyMovement()
        {
            if (velocity.magnitude > 0.01f)
            {
                Vector3 newPosition = tr.position + velocity;
                
                // Apply camera bounds if enabled
                if (useCameraBounds)
                {
                    newPosition.x = Mathf.Clamp(newPosition.x, minBounds.x, maxBounds.x);
                    newPosition.y = Mathf.Clamp(newPosition.y, minBounds.y, maxBounds.y);
                }

                newPosition.z = 0;
                tr.position = newPosition;
            }
        }
        
        // Public methods for external control
        public void SetCameraPosition(Vector3 position)
        {
            tr.position = new Vector3(position.x, position.y, tr.position.z);
            velocity = Vector3.zero;
        }
        
        public void AddForce(Vector3 force)
        {
            velocity += force;
        }
        
        public void SetBounds(Vector2 min, Vector2 max)
        {
            minBounds = min;
            maxBounds = max;
        }
        
        public void FocusOnPosition(Vector3 targetPosition, float duration = 1f)
        {
            StartCoroutine(SmoothMoveToPosition(targetPosition, duration));
        }
        
        private System.Collections.IEnumerator SmoothMoveToPosition(Vector3 targetPosition, float duration)
        {
            Vector3 startPosition = tr.position;
            targetPosition.z = startPosition.z; // Maintain Z position
            
            float elapsed = 0f;
            velocity = Vector3.zero; // Stop current movement
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                t = t * t * (3f - 2f * t); // Smoothstep for easing
                
                tr.position = Vector3.Lerp(startPosition, targetPosition, t);
                yield return null;
            }
            
            tr.position = targetPosition;
        }
    }
}
