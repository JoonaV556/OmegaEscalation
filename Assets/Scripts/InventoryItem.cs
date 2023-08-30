using System.Collections;
using System.Collections.Generic;
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

        // Get the cursor offset relative to the origin of the item
        _cursorOffset = eventData.position - new Vector2(_rectTransform.position.x, _rectTransform.position.y);
    }

    public void OnDrag(PointerEventData eventData) {
        
        // Update item position
        _rectTransform.position = eventData.position - _cursorOffset;
    }

    public void OnEndDrag(PointerEventData eventData) {
        
    }

    #endregion


}
