using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryBackground : MonoBehaviour, IDropHandler {
    public void OnDrop(PointerEventData eventData) {
        // Drop the stack of dropped items

        GameObject dropped = eventData.pointerDrag;

        if (dropped != null) {
            Destroy(dropped);
        }
    }
}
