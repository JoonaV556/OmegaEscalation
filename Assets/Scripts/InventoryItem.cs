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

    public void InitializeItem(Item item, int stackSize) {
        _item = item;

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
