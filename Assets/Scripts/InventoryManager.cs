using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;
using static UnityEngine.Rendering.DebugUI.Table;

public class InventoryManager : MonoBehaviour {

    #region Properties

    [SerializeField]
    private GameObject InventoryItemPrefab; // Prefab used for spawning items in the inventory
    [SerializeField]
    private RectTransform InventoryGridTransform; // Transform component of the inventory grid, items will be instantiated as a child of this transform

    [SerializeField]
    private int _gridWidth = 320; // Should be the width of the RectTransform of the inventorygrid
    [SerializeField]
    private int _gridHeight = 320; // Should be the width of the RectTransform of the inventorygrid

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
        // Tries to add picked up item to inventory.
        // 1. Tries to add item to existing stacks
        // 2. If stacking fails, creates new stacks

        // Try to stack items
        for (int column = 0; column < _gridColumns; column++) { // Loop through columns
            for (int row = 0; row < _gridRows; row++) { // Loop through rows
                // Accessing the array with row as X and column as Y, so the horizontal rows fill be filled before moving to next column

                if (
                    _inventorySlots[row, column].IsOoccupied() && // If slot is occupied
                    worldItem.GetItem().isStackable && // If item being added is stackable
                    _inventorySlots[row, column].GetOccupyingItem().GetItem() == worldItem.GetItem() && // If item being stacked is same type as the on in the slot
                    (_inventorySlots[row, column].GetOccupyingItem().GetStackSize() + worldItem.StackSize) <= _inventorySlots[row, column].GetOccupyingItem().GetItem().maxStackSize // Check if can increase the stack size
                    ) {

                    Debug.Log("Increased existing stack size");
                    _inventorySlots[row, column].GetOccupyingItem().IncreaseStackSize(worldItem.StackSize);

                    // Exit the checking loop
                    return;
                }
            }
        }

        Debug.Log("Stacking failed. Trying to create a new stack...");

        // If stacking fails, Try to create a new stack

        Vector2Int itemSize = worldItem.GetItem().inventorySize;

        // If item takes only one slot, find empty slot and place it down   
        if (itemSize.x == 1 && itemSize.y == 1) {
            for (int column = 0; column < _gridColumns; column++) {
                for (int row = 0; row < _gridRows; row++) {
                    if (!_inventorySlots[row, column].IsOoccupied()) {
                        // If the slot is not occupied, place the item

                        Debug.Log("Created a new stack succesfully");

                        // Place item in slot
                        PlaceItemInSlot(worldItem, new Vector2Int(row, column));
                        Debug.Log("Placed item in slot: [" + row + ", " + column + "]");

                        // Return so item won't be placed on all open slots
                        return;
                    }
                }
            }
            // If item takes more than one slot, find empty slot and check surrounding slots before placing
        } else {
            int horizontalSlotsNeeded = itemSize.x;
            int verticalSlotsNeeded = itemSize.y;


            for (int column = 0; column < _gridColumns; column++) {
                for (int row = 0; row < _gridRows; row++) {

                    if (CheckSlots(row, column, verticalSlotsNeeded, horizontalSlotsNeeded)) {
                        // If origin slot + surrounding slots are empty, place down the object

                        Debug.Log("Created a new stack succesfully");

                        // Place item in slot
                        PlaceItemInSlot(worldItem, new Vector2Int(row, column));
                        Debug.Log("Placed item in slot: [" + row + ", " + column + "]");

                        // Return so item won't be placed on all open slots
                        return;
                    }
                }
            }
        }





        // If item takes more than one slot, check surrounding slots and place it down


        Debug.Log("Cannot pick up. No empty slots left");
        return;
    }

    private bool CheckSlots(int originSlotX, int originSlotY, int verticalSlotsToCheck, int horizontalSlotsToCheck) {
        // Checks if origin slot and needed surrounding slots are empty

        int emptyVerticalSlots = 0;

        // Check vertical slots on each row below the origin
        for (int y = 0; y < verticalSlotsToCheck; y++) {

            // Check if slot is occupied
            if (!_inventorySlots[originSlotX, (originSlotY + y)].IsOoccupied()) {
                emptyVerticalSlots++;
            }
        }

        // If needed vertical slots are empty, continue to check horizontal slots
        if (emptyVerticalSlots == verticalSlotsToCheck) {

            int emptyHorizontalSlots = 1;

            for (int y = 0; y < verticalSlotsToCheck; y++) { // Loop through each row
                // Check if slot is occupied

                for (int x = 1; x < horizontalSlotsToCheck; x++) { // Loop through each column, start at 1 because we already checked 0 before

                    if (!_inventorySlots[(originSlotX + x), (originSlotY + y)].IsOoccupied()) {
                        emptyHorizontalSlots++;
                    }
                }
            }

            if (emptyHorizontalSlots == horizontalSlotsToCheck) {
                return true;
            } else {
                return false;
            }

        } else { 
            return false; 
        }
    }

    private void PlaceItemInSlot(WorldItem item, Vector2Int slot) {
        // Places item in a inventory slot on the grid, doesn't care if the slot is occupied. Occupied check is done in TryToPickUp()

        // Spawn the item, set parent to inventorygrid
        GameObject _spawnedItem = Instantiate(InventoryItemPrefab, InventoryGridTransform);

        // Initialize the item (Set icon, stack size in inventory, etc.)
        _spawnedItem.GetComponent<InventoryItem>().InitializeItem(item.GetItem(), item.StackSize);

        // Store InventoryItem component, it will be used for occupying the slot
        InventoryItem _spawnedItemInvComponent = _spawnedItem.GetComponent<InventoryItem>();

        // Set item position
        _spawnedItem.GetComponent<RectTransform>().localPosition = new Vector3(slot.x * _slotSize, -slot.y * _slotSize, 0f);

        // TODO Set other slots to occupied 
        // Needed: x and y coordinates for each slot to be occupied
        // 
        // 1. Occupy vertical slots 
        // 2. Occupy horizontal slots on all vertical slot rows

        int horizontalSlotsToOccupy = item.GetItem().inventorySize.x;
        int verticalSlotsToOccupy = item.GetItem().inventorySize.y;

        // Occupy vertical slots on each row below the origin
        for (int i = 0; i < verticalSlotsToOccupy; i++) {
            _inventorySlots[slot.x, (slot.y + i)].SetOccupied(true);
            _inventorySlots[slot.x, (slot.y + i)].SetOccupyingItem(_spawnedItemInvComponent);
            Debug.Log("Occupied slot: [" + slot.x + ", " + (slot.y + i) + "]");
        }

        // Occupy horizontal slots on each vertical row
        for (int v = 0; v < verticalSlotsToOccupy; v++) {
            for (int h = 1; h < horizontalSlotsToOccupy; h++) {
                _inventorySlots[(slot.x + h), (slot.y + v)].SetOccupied(true);
                _inventorySlots[(slot.x + h), (slot.y + v)].SetOccupyingItem(_spawnedItemInvComponent);
                Debug.Log("Occupied slot: [" + (slot.x + h) + ", " + (slot.y + v) + "]");
            }
        }

        // Set occupied item type for stacking
        _inventorySlots[slot.x, slot.y].SetOccupyingItem(_spawnedItemInvComponent);
    }

    private class InventorySlot {
        // Class used for the inventory slots inside the inventory grid

        private bool _occupied = false;
        private InventoryItem _occupyingItem;

        public bool IsOoccupied() {
            return _occupied;
        }

        public void SetOccupied(bool newOccupied) {
            _occupied = newOccupied;
        }

        public InventoryItem GetOccupyingItem() {
            return _occupyingItem;
        }

        public void SetOccupyingItem(InventoryItem item) {
            _occupyingItem = item;
        }


    }
}
