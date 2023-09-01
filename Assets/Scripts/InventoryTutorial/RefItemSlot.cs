using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class RefItemSlot : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData) {
        GameObject dropped = eventData.pointerDrag;
        RefInventoryItem draggedItem = dropped.GetComponent<RefInventoryItem>();
        
        if (IsSlotTaken() == false) {
            draggedItem.parentAfterDrag = transform;
        }     
    }


    public bool IsSlotTaken() {

        bool isSlotTaken;

        if (transform.childCount > 0) {
            isSlotTaken = true;
        } else {
            isSlotTaken = false;
        }

        return isSlotTaken;
    }
}
