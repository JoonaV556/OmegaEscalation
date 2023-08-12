using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class ItemSlot : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData) {
        GameObject dropped = eventData.pointerDrag;
        DraggableItem draggedItem = dropped.GetComponent<DraggableItem>();
        
        if (IsSlotTaken() == false) {
            draggedItem.parentAfterDrag = transform;
        }     
    }


    private bool IsSlotTaken() {

        bool isSlotTaken;

        if (transform.childCount > 0) {
            isSlotTaken = true;
        } else {
            isSlotTaken = false;
        }

        return isSlotTaken;
    }
}
