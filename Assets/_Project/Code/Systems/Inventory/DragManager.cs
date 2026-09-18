
using System.Collections.Generic;
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Code.Gameplay.Combat.Events;
using Galactic1.Code.Inventory.Abstractions;
using Galactic1.Mobile.EventBus;
using Galactic1.UI.Audio;
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
        private InventoryPanelAudioConfig _audioConfig;

        // Drag
        private DragIcon dragIcon;
        private InventorySlotView draggedSlot;
        private bool dragStarted;
        private bool droppedOnSameSlot = false;
        
        private RuntimeId draggedItemId;
        private IInventorySource draggedSource;
        private int draggedSlotIndex;

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
        

        
        
        public DragManager(
            Canvas canvas, 
            DragIcon iconPrefab,
            InventoryManagementWindow window, 
            TooltipInventoryUI tooltip,
            InventoryPanelAudioConfig audioConfig)
        {
            this.canvas = canvas;
            this.dragIconPrefab = iconPrefab;
            this.window = window;
            this.tooltip = tooltip;

            _audioConfig = audioConfig;
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
            droppedOnSameSlot = false;
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
        
        /// <summary>Live-запрос "что сейчас перетаскивается" для guidance-условий тутора
        /// (см. ItemDraggedGuidanceCondition). Null — драг не идёт, либо порог dragThreshold
        /// ещё не превышен (намеренно: клик "с дрожащей рукой" не должен читаться как драг).</summary>
        public RuntimeId DraggedItemId
        {
            get
            {
                if (!dragStarted || draggedSlot == null) return null;
                var slot = draggedSlot.ParentUI.GetSlot(draggedSlot.SlotIndex);
                return slot.IsEmpty ? null : slot.Item.Id;
            }
        }

        // ---- Update ----
        public void Update()
        {
            // ---- Hold для Tooltip ----
            if (!dragStarted && pointerDownSlot != null && !tooltipShown)
            {
                holdTimer += Time.deltaTime;
                
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
            
            //EventBus<InventorySlotDragEvent>.Raise(new InventorySlotDragEvent()); ???

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
            
            draggedItemId = slotData.Item.Id;
            draggedSource = fromSlot.ParentUI._source;
            draggedSlotIndex = fromSlot.SlotIndex;
            
            EventBus<InventoryDragStartedEvent>.Raise(new InventoryDragStartedEvent(
                draggedSource,
                draggedItemId,
                draggedSlotIndex));
            
            EventBus<AudioUIEvent>.Raise(new AudioUIEvent(_audioConfig.itemDrag.ToData()));

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
                    EndDrag(InventoryDragEndReason.Drop);
                    return;
                }
            }

            EndDrag(InventoryDragEndReason.PointerReleased);
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


        private void EndDrag(InventoryDragEndReason reason)
        {
            if (dragIcon != null)
                GameObject.Destroy(dragIcon.gameObject);

            if (droppedOnSameSlot)
                reason = InventoryDragEndReason.Cancel;
            
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
            
            EventBus<InventoryDragEndedEvent>.Raise(
                new InventoryDragEndedEvent(
                    draggedSource,
                    draggedItemId,
                    draggedSlotIndex,
                    reason));
        }
    }

}