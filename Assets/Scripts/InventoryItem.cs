using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryItem : MonoBehaviour
{
    [SerializeField] private Image _itemIcon;
    [SerializeField] private TMP_Text _stackSize;

    public void InitializeItem(Sprite icon, int stackSize) {
        _itemIcon.sprite = icon;
        _stackSize.text = stackSize.ToString();
    }
}
