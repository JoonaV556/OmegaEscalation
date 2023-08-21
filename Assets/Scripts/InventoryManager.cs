using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    #region Properties

    [SerializeField]
    private GameObject InventoryItemPrefab; // Prefab used for spawning items in the inventory
    [SerializeField]
    private RectTransform InventoryGridTransform; // Transform component of the inventory grid, items will be instantiated as a child of this transform

    [SerializeField]
    private int _gridWidth = 64; // Should be the width of the RectTransform of the inventorygrid
    [SerializeField]
    private int _gridHeight = 64; // Should be the width of the RectTransform of the inventorygrid

    private int _gridRows;
    private int _gridColumns;

    private int _slotSize = 32;


    private InventorySlot[,] _inventorySlots; // Array of the slots

    #endregion

    private void Awake() {
        Initialize();
    }

    private void Start() {
        InitializeInventoryGrid();
    }

    private void Initialize() {
        // Subscribe to important callbacks
        Looting.OnTryToPickUp += TryToPickup;

        _gridColumns = _gridWidth / _slotSize;
        _gridRows = _gridHeight / _slotSize;

    }

    private void InitializeInventoryGrid() {
        // Init array
        _inventorySlots = new InventorySlot[_gridColumns, _gridRows];
                
        // Add new InventorySlots to each position on the array
        // Loop through columns 
        for (int column = 0; column < _gridColumns; column++) {
            // Loop throug rows
            for (int row = 0; row < _gridRows; row++) {
                // Create new InventorySlot in the array position
                _inventorySlots[column, row] = new InventorySlot();
            }

        }

    }

    private void TryToPickup(WorldItem worldItem) {
        // Used for picking up items in the world

        // 1. Find empty slot
        // 2. later - If slot is not empty, check if item is same type and stackable -> Stack the item with the existing item
        // 3. Place item in empty slot

        // 1. Find empty slot
        // Loop through columns
        for (int column = 0; column < _gridColumns; column++) {
            // Loop throug rows
            for (int row = 0; row < _gridRows; row++) {

                // if (_inventorySlots[i, j].CheckIfOccupied)
                Debug.Log("Checked slot: [" + column + "," + row + "]");
                Debug.Log("Is slot occupied: " + _inventorySlots[column, row].CheckIfOccupied());
            }

        }




        // Testing - Adds new inventory item as child of the grid object, NO SLOT functionality
        AddToInventory(worldItem);

    }

    private void AddToInventory(WorldItem item, Vector2Int slot) {

    }

    // Only for testing
    private void AddToInventory(WorldItem worldItem) {

        // Get item stack size 
        int stackSize = worldItem.StackSize;

        // Save the sprite icon
        Sprite icon = worldItem.GetItem().icon;
        
        
        // Spawn the item & Init values
        GameObject newInventoryItem = Instantiate(InventoryItemPrefab);
        newInventoryItem.GetComponent<InventoryItem>().InitializeItem(icon, stackSize);

        // Add the item as a child of the inventory grid 
        newInventoryItem.transform.SetParent(InventoryGridTransform, false);
        // Set the item position in the grid
        newInventoryItem.transform.localPosition = new Vector3(0f, 0f, 0f);
        
    }

    private class InventorySlot {
        private bool _occupied = false;

        public bool CheckIfOccupied() {
            return _occupied;
        }
    }
}
