using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IDropHandler {
    // Handles behavior related to each inventory slot on the grid 
    // !Notice! Requires a running InventoryManager instance to work properly

    #region Properties

    private Vector2Int _slotGridPosition;
    private bool _occupied = false;
    private InventoryItem _occupyingItem;
    private Image _slotImage;
    [SerializeField]
    private Color _defaultColor;
    [SerializeField]
    private Color _highlightColor;
    [SerializeField]
    private Color _reservedColor;
    private List<InventorySlot> checkedSlots;
    private bool canDropInThisSlot = false;
    private InventoryItem _itemWhichWillBeDropped;
    private InventoryItem draggedItem;

    #endregion

    private void Awake() {
        _slotImage = GetComponent<Image>();
    }

    #region Methods

    public bool IsOoccupied() {
        return _occupied;
    }

    public void SetOccupied(bool newOccupied) {
        _occupied = newOccupied;
    }

    public InventoryItem GetOccupyingItem() {
        return _occupyingItem;
    }

    public void SetOccupyingItem(InventoryItem item) {
        _occupyingItem = item;
    }

    public void SetGridPositionInfo(int posX, int posY) {
        _slotGridPosition.x = posX;
        _slotGridPosition.y = posY;
    }

    public void OnPointerEnter(PointerEventData eventData) {
        // Triggered when cursor starts hovering over the slot

        // Check if pointer (cursor) is currenty dragging an item
        bool isDraggingAnItem = eventData.dragging && eventData.pointerDrag.GetComponent<InventoryItem>();

        // Exit if not dragging
        if (!isDraggingAnItem) {
            return;
        }

        // ----> Dragging an item

        // Check other required slots to determine if item can be dropped in this slot

        // Cache item
        draggedItem = eventData.pointerDrag.GetComponent<InventoryItem>();
        // Cache item size
        Vector2Int draggedItemSize = draggedItem.GetItem().inventorySize;
        
        // Check if item can be dropped in this slot
        canDropInThisSlot = InventoryManager.Instance.CheckSlots(draggedItem, _slotGridPosition.x, _slotGridPosition.y, draggedItemSize.x, draggedItemSize.y);

        // Cache all slots that need to be highlighted
        checkedSlots = InventoryManager.Instance.GetInventorySlots(_slotGridPosition, draggedItemSize.x, draggedItemSize.y);

        // Exit if item cannot be dropped in this slot
        if (!canDropInThisSlot) {
            // Highight all slots that cannot be dropped in
            foreach (InventorySlot slot in checkedSlots) {
                slot._slotImage.color = _reservedColor;
            }

            // Exit
            return;
        }

        // ----> Item can be dropped in this slot

        // Highight all slots that can be dropped in
        foreach (InventorySlot slot in checkedSlots) {
            slot._slotImage.color = _highlightColor;
        }

        // Set dragged item's new grid position to this slot (i.e. Item will be dropped in this slot if dropped)
        Vector2 newItemPosition = InventoryManager.Instance.GetPositionOnGrid(_slotGridPosition.x, _slotGridPosition.y);
        draggedItem._itemNewPosition.x = newItemPosition.x;
        draggedItem._itemNewPosition.y = newItemPosition.y;

        // Set dropped item
        _itemWhichWillBeDropped = draggedItem;
    }

    public void OnPointerExit(PointerEventData eventData) {
        // When cursor exits hovering over the slot
        
        // Reset canDrop
        if (canDropInThisSlot) {
            canDropInThisSlot = false;
        }

        // Reset dragged item's new grid position so it'll be dropped in its old position if dropped
        if (draggedItem != null) {
            draggedItem._itemNewPosition = draggedItem._itemOldPosition;
        }

        // Reset dropped item
        _itemWhichWillBeDropped = null;

        canDropInThisSlot = false;

        // Remove highlight from checked slots (if any)
        if (checkedSlots != null) {
            foreach (InventorySlot slot in checkedSlots) {
                slot._slotImage.color = _defaultColor;
            }

            // Reset checked slots
            checkedSlots = null;
        }
    }

    public void OnDrop(PointerEventData eventData) {
        // If pointer dropped an item succesfully on this slot -> Occupy checked slots

        // Exit if cannot drop
        if (!canDropInThisSlot) {
            return;
        }

        canDropInThisSlot = false;

        // ----> Can drop

        // Unoccupy old slots
        _itemWhichWillBeDropped.UnoccupyOldSlots();

        // Occupy all required slots
        OccupySlots(checkedSlots, _itemWhichWillBeDropped);

        // Add occupied slots to items occupied slots list
        _itemWhichWillBeDropped.SetOccupiedSlots(checkedSlots);

        Debug.Log("Dropped item in slot: " + _slotGridPosition);
    }

    public void OccupySlots(List<InventorySlot> newOccupiedSlots, InventoryItem item) {
        // This should be called by the new slot when the item is dropped onto it

        foreach (InventorySlot slot in newOccupiedSlots) {
            // Occupy new slots
            slot.SetOccupied(true);
            slot.SetOccupyingItem(item);
            slot.canDropInThisSlot = false;
        }
    }

    #endregion
}
