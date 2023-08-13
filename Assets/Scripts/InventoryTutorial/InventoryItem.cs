using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Item _item;

    public Transform parentAfterDrag;
    public Image img;
    public TMP_Text stackSizeTxt;
    public int stackSize = 1;

    public void InitializeItem(Item item) {
        _item = item;
        img.sprite = _item.image;
        RefreshStackSizeUI();
    }

    public void RefreshStackSizeUI() {
        // Updates the number on the stacksize text
        stackSizeTxt.text = stackSize.ToString();

        // If stack size is just 1, Hide the stack size text
        bool textActive = stackSize > 1;
        stackSizeTxt.gameObject.SetActive(textActive);
    }

    public void OnBeginDrag(PointerEventData eventData) {
        parentAfterDrag = transform.parent;
        transform.SetParent(transform.root);
        transform.SetAsLastSibling();

        img.raycastTarget = false;
    }

    public void OnDrag(PointerEventData eventData) {
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData) {
        transform.SetParent(parentAfterDrag);
        img.raycastTarget = true;
    }

}
