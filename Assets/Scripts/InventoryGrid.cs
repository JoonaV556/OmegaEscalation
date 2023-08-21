using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryGrid : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
{
    [SerializeField]
    private RectTransform gridTransform;
    private bool isCursorOnGrid = false;

    public void OnPointerEnter(PointerEventData eventData) {
        isCursorOnGrid = true;
    }

    public void OnPointerExit(PointerEventData eventData) {
        isCursorOnGrid = false;
    }



    // WORKS !!!!
    public void OnPointerMove(PointerEventData eventData) {
        if (isCursorOnGrid) { 
            // This returns the mouse position relative to the RectTransform of the inventoryGrid
            // If "Pivot" of RectTransform is 0x0, and the mouse is located at the pivot position, the localposition will be 0x0
            // If mouse is at the opposite corner, position will be 320x320
            // NOTE:
            // If canvas mode is "Screenspace - overlay", leave camera parameter null

            RectTransformUtility.ScreenPointToLocalPointInRectangle(gridTransform, new Vector2(Input.mousePosition.x, Input.mousePosition.y), null, out Vector2 point);
            Debug.Log("Cursor point on grid: " + point);
        }
    }

}
