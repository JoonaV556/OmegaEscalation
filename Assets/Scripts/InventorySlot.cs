using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    // Handles behavior related to each inventory slot on the grid 

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

        // if - (dragging == true && pointer is dragging an item)
        // -> Set item grid position to this slot
        
        bool canDropInThisSlot;

        // If _occupied is true, set canDropInThisSlot to false
        if (_occupied) {
            canDropInThisSlot = false;
        } else {
            canDropInThisSlot = true;
        }

        // If item can be dropped in this slot && 
        // Pointer is draggin an item &&
        // Dragged item has an InventoryItem component
        // -> Set dragged item's grid position to this slot
        if (canDropInThisSlot && eventData.dragging && eventData.pointerDrag.GetComponent<InventoryItem>()) {
            InventoryItem draggedItem = eventData.pointerDrag.GetComponent<InventoryItem>();

            Vector2 newItemPosition = InventoryManager.Instance.GetPositionOnGrid(_slotGridPosition.x, _slotGridPosition.y);
            draggedItem._itemNewPosition.x = newItemPosition.x;
            draggedItem._itemNewPosition.y = newItemPosition.y;
        }




        if (_occupied && eventData.dragging) {
            _slotImage.color = _reservedColor;
        } else if (eventData.dragging) {
            _slotImage.color = _highlightColor;
        }

        Debug.Log("Entered slot: " + _slotGridPosition);
    }

    public void OnPointerExit(PointerEventData eventData) {
        // When cursor exits hovering over the slot
        _slotImage.color = _defaultColor;
       
    }

    #endregion
}
