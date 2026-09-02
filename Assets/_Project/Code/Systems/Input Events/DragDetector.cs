using System;
using UnityEngine;

namespace Galactic1.Structure
{
    
    public class DragDetector
    {
        private bool isDragging = false;
        private Vector3 dragStartMousePosition;
        private float dragThreshold;

        public Action OnBeginDrag;
        public Action OnDragging;
        public Action OnEndDrag;

        public DragDetector(float dragThreshold = 5f)
        {
            this.dragThreshold = dragThreshold;
        }

        /// <summary>
        /// Вызывать каждый кадр из Update().
        /// </summary>
        public void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                dragStartMousePosition = Input.mousePosition;
                isDragging = false;
            }

            if (Input.GetMouseButton(0))
            {
                float distance = Vector3.Distance(Input.mousePosition, dragStartMousePosition);

                if (!isDragging && distance > dragThreshold)
                {
                    isDragging = true;
                    OnBeginDrag?.Invoke();
                }

                if (isDragging)
                {
                    OnDragging?.Invoke();
                }
            }

            if (Input.GetMouseButtonUp(0))
            {
                if (isDragging)
                {
                    OnEndDrag?.Invoke();
                }

                isDragging = false;
            }
        }
    }


}