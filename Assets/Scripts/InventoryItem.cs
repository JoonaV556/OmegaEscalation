using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {
    // Handles behaviour related to each item in a container (refreshes stack size text, etc.)
    // Each inventory item UI element has this as a component

    #region Properties

    [SerializeField] private Image _itemIcon;
    [SerializeField] private TMP_Text txtStackSize;
    private int _stackSize;
    private Item _item;
    private RectTransform _rectTransform;
    private bool _isItemSizeSet = false;
    private bool _isItemInfoReceived = false;
    private Vector2 _cursorOffset;
    public Vector3 _itemOldPosition; // The position of the item in the grid when it is not being dragged
                                      // This is changed if the item is dragged onto another slot on the grid
    public Vector3 _itemNewPosition;
    private List<InventorySlot> _occupiedSlots = new List<InventorySlot>(); // List of slots which the item is occupying


    #endregion

    
    private void Awake() {
        InitAwake();
    }

    #region Methods

    private void InitAwake() {
        if (GetComponent<RectTransform>()) {
            _rectTransform = GetComponent<RectTransform>();
        } else {
            Debug.Log("Cannot find rectTransform of item!");
        }

        _itemOldPosition = transform.localPosition;
        _itemNewPosition = transform.localPosition;

        // Do a late init if item size has not been set
        // If item gameobject is not active when it gets added to inventory, the transform size cannot be set
        if (!_isItemSizeSet && _isItemInfoReceived) {
            SetItemSize();
        }     
    }

    public void InitializeItem(Item item, int stackSize) {
        // Get item info from picked up item and set the same info into this
        _item = item;

        // Set the item icon size if item is active and rectTransform is found
        if (_rectTransform != null) {
            _rectTransform.sizeDelta = new Vector2(_item.inventorySize.x * 32f, _item.inventorySize.y * 32f); // The hardcoded 32 values should be replaced by universal slotsize number in future
            _isItemSizeSet = true;
        }
       
        // Set sprite and stacksize
        _itemIcon.sprite = _item.icon;
        _stackSize = stackSize;
        txtStackSize.text = stackSize.ToString();

        _isItemInfoReceived = true;
    }

    private void SetItemSize() {
        _rectTransform.sizeDelta = new Vector2(_item.inventorySize.x * 32f, _item.inventorySize.y * 32f); // The hardcoded 32 values should be replaced by universal slotsize number in future
        _isItemSizeSet = true;
    }

    public void IncreaseStackSize(int amountToIncrease) {
        _stackSize += amountToIncrease;
        txtStackSize.text = _stackSize.ToString();
    }

    public Item GetItem() {
        return _item;
    }

    public int GetStackSize() {
        return _stackSize;
    }

    public void OnBeginDrag(PointerEventData eventData) {
        // When the drag starts

        // Get the cursor offset relative to the origin of the item
        _cursorOffset = eventData.position - new Vector2(_rectTransform.position.x, _rectTransform.position.y);

        _itemIcon.raycastTarget = false;
        txtStackSize.raycastTarget = false;
    }

    public void OnDrag(PointerEventData eventData) {
        // When item is being dragged
        


        // Update item position
        _rectTransform.position = eventData.position - _cursorOffset;
    }

    public void OnEndDrag(PointerEventData eventData) {
        _itemIcon.raycastTarget = true;
        txtStackSize.raycastTarget = true;

        if (_itemOldPosition != _itemNewPosition) {
            // Set position to new position
            _rectTransform.localPosition = _itemNewPosition;
            // Update old position
            _itemOldPosition = _itemNewPosition;

        } else {
            // Reset back to old position
            _rectTransform.localPosition = _itemOldPosition;
        }

    }

    public void SetOccupiedSlots(List<InventorySlot> newOccupiedSlots) {
        // Adds slots to the list of occupied slots for this item
        foreach (InventorySlot slot in newOccupiedSlots) {
            _occupiedSlots.Add(slot);
        }
    }

    public void AddToOccupiedSlots(InventorySlot newOccupiedSlot) {
        // Adds slots to the list of occupied slots for this item
          
        _occupiedSlots.Add(newOccupiedSlot);

    }

    public void UnoccupyOldSlots() {
        // Unoccupy all old slots
        foreach (InventorySlot slot in _occupiedSlots) {
            slot.SetOccupied(false);
            slot.SetOccupyingItem(null);
        }

        // Clear old slots from the occupied slots list
        _occupiedSlots.Clear();
    }

    #endregion


}
