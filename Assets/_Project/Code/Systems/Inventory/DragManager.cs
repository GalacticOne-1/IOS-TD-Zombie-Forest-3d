
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


namespace Galactic1.Code.UI.Inventory
{
    public class DragManager
    {
        private readonly Canvas canvas;
        private readonly DragIcon dragIconPrefab;
        private readonly InventoryManagementWindow window;
        private readonly TooltipInventoryUI tooltip;

        // Drag
        private DragIcon dragIcon;
        private InventorySlotView draggedSlot;
        private bool dragStarted;
        private bool droppedOnSameSlot = false;

        // Pointer
        private Vector2 pointerDownPos;
        private InventorySlotView pointerDownSlot;
        
        // Tooltip
        private const float holdTime = 0.3f;
        private float holdTimer = 0f;
        private bool tooltipLoaded = false;
        private bool tooltipShown = false;

        private Vector3 smoothVelocity;
        private const float smoothSpeed = 20f;
        private const float dragThreshold = 8f;
        

        public DragManager(Canvas canvas, DragIcon iconPrefab, InventoryManagementWindow window, TooltipInventoryUI tooltip)
        {
            this.canvas = canvas;
            this.dragIconPrefab = iconPrefab;
            this.window = window;
            this.tooltip = tooltip;
        }

        // ---- API, вызываемая из InventorySlotUI ----
        public void OnPointerDown(InventorySlotView slot, PointerEventData eventData)
        {
            pointerDownSlot = slot;
            pointerDownPos = eventData.position;

            holdTimer = 0f;
            tooltipLoaded = false;
            tooltipShown = false;
            dragStarted = false;
        }

        public void OnPointerUp(InventorySlotView slot, PointerEventData eventData)
        {
            // скрыть подсказку
            tooltip.Hide();
            
            // если drag не начался — это обычный клик
            if (!dragStarted && pointerDownSlot == slot)
            {
                // обычный клик
                window.ClearAllSelections();

                var source = slot.ParentUI._source;
                if (!source.GetSlot(slot.SlotIndex).IsEmpty)
                    slot.ParentUI.SelectSlot(slot);
            }

            pointerDownSlot = null;
        }

        public void OnDrag(PointerEventData eventData)
        {
            // если tooltip успел появиться — отменяем drag
            //if (tooltipShown) 
                //return;
            
            if (pointerDownSlot != null && !dragStarted)
            {
                Vector2 delta = eventData.position - pointerDownPos;
                if (delta.sqrMagnitude >= dragThreshold * dragThreshold)
                {
                    tooltip.Hide();
                    StartDrag(pointerDownSlot);
                }
            }
        }

        // ---- Update ----
        public void Update()
        {
            // ---- Hold для Tooltip ----
            if (!dragStarted && pointerDownSlot != null && !tooltipShown)
            {
                holdTimer += UnityEngine.Time.deltaTime;
                
                if(!tooltipLoaded)
                    LoadTooltip(pointerDownSlot);

                if (holdTimer >= holdTime)
                    ShowTooltip(pointerDownSlot);
            }

            // ---- Drag update ----
            if (!dragStarted || dragIcon == null) return;

            dragIcon.transform.position = Vector3.SmoothDamp(
                dragIcon.transform.position,
                Input.mousePosition,
                ref smoothVelocity,
                1f / smoothSpeed
            );

            if (Input.GetMouseButtonUp(0))
                TryDrop();
        }
        
        
        // -----------------------------
        // TOOLTIP
        // -----------------------------
        void LoadTooltip(InventorySlotView slot)
        {
            var slotData = slot.ParentUI._source.GetSlot(slot.SlotIndex);
            if (slotData.IsEmpty) return;

            tooltip.LoadData(slotData.Item, slotData.Durability);
            tooltipLoaded = true;
        }
        private void ShowTooltip(InventorySlotView slot)
        {
            var slotData = slot.ParentUI._source.GetSlot(slot.SlotIndex);
            if (slotData.IsEmpty) return;

            tooltip.Show(slot.gameObject.CMP_RectTr());
            tooltipShown = true;
        }


        // ---- Drag logic ----

        private void StartDrag(InventorySlotView fromSlot)
        {
            var slotData = fromSlot.ParentUI._source.GetSlot(fromSlot.SlotIndex);
            if (slotData.IsEmpty)
            {
                pointerDownSlot = null;
                return;
            }
            
            tooltip.Hide();

            window.ClearAllSelections();

            draggedSlot = fromSlot;
            dragStarted = true;

            dragIcon = GameObject.Instantiate(dragIconPrefab, canvas.transform);
            dragIcon.transform.position = fromSlot.transform.position;
            dragIcon.SetSprite(slotData.Item.Header.icon);

            fromSlot.SetDimmed(true);
            fromSlot.SetHighlight(true);
            pointerDownSlot = null;

            fromSlot.ParentUI.HighlightEquipmentSlots(slotData.Item, true);
        }

        private void TryDrop()
        {
            var pointer = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };

            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointer, results);

            foreach (var hit in results)
            {
                var targetSlot = hit.gameObject.GetComponent<InventorySlotView>();
                if (targetSlot != null)
                {
                    Drop(targetSlot);
                    EndDrag();
                    return;
                }
            }

            EndDrag();
        }

        private void Drop(InventorySlotView target)
        {
            droppedOnSameSlot = false;

            if (draggedSlot == null) return;

            // same slot = cancel (но НЕ сбрасываем draggedSlot!)
            if (draggedSlot == target ||
                (draggedSlot.ParentUI == target.ParentUI &&
                 draggedSlot.SlotIndex == target.SlotIndex))
            {
                droppedOnSameSlot = true;
                return;
            }

            window.controller.MoveItem(
                draggedSlot.ParentUI._source,
                draggedSlot.SlotIndex,
                target.ParentUI._source,
                target.SlotIndex
            );
        }


        private void EndDrag()
        {
            if (dragIcon != null)
                GameObject.Destroy(dragIcon.gameObject);

            // если дроп на тот же слот — НИЧЕГО НЕ СБРАСЫВАЕМ
            if (draggedSlot != null)
            {
                draggedSlot.SetDimmed(false);
                
                var item = draggedSlot.ParentUI._source.GetSlot(draggedSlot.SlotIndex).Item;
                draggedSlot.ParentUI.HighlightEquipmentSlots(item, false);
                
                draggedSlot.SetHighlight(droppedOnSameSlot);
            }

            dragIcon = null;
            draggedSlot = null;
            dragStarted = false;
            droppedOnSameSlot = false;
        }
    }

}