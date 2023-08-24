using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryItem : MonoBehaviour {
    [SerializeField] private Image _itemIcon;
    [SerializeField] private TMP_Text txtStackSize;
    private int _stackSize;
    private Item _item;
    private RectTransform _rectTransform;

    // TODO
    // Setup item size in init
    private void Awake() {
        Init();
    }

    private void Init() {
        if (GetComponent<RectTransform>()) {
            _rectTransform = GetComponent<RectTransform>();
        } else {
            Debug.Log("Cannot find rectTransform of item!");
        }
    }

    public void InitializeItem(Item item, int stackSize) {
        // Setup item
        _item = item;

        // Set the size of the item 
        if (_rectTransform != null) {
            _rectTransform.sizeDelta = new Vector2 (_item.inventorySize.x * 32f, _item.inventorySize.y * 32f); // The hardcoded 32 values should be replaced by universal slotsize number in future
        } else {
            Debug.LogError("InventoryItem: RectTransform null! Cannot set size!");
        }
       
        // Seutp sprite and stacksize
        _itemIcon.sprite = _item.icon;
        _stackSize = stackSize;
        txtStackSize.text = stackSize.ToString();
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
}
