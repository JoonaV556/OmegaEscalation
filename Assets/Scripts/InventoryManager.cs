using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour {
    // Core of the inventory system. Sets up the virtual inventory grid and slots. Handles managing items in the inventory slots

    #region Properties

    public static InventoryManager Instance;

    [SerializeField]
    private GameObject InventoryItemPrefab; // Prefab used for spawning items in the inventory
    [SerializeField]
    private GameObject InventorySlotPrefab; // Prefab used for visual representation of the inventory slots
    [SerializeField]
    private RectTransform InventoryGridTransform; // Transform component of the inventory grid, items will be instantiated as a child of this transform
    [SerializeField]
    private RectTransform InventorySlotsContainer; // Transform component of the inventory slots container, slots will be instantiated as a child of this transform


    [SerializeField]
    private int _gridWidth = 320; // Should be the width of the RectTransform of the inventorygrid
    [SerializeField]
    private int _gridHeight = 320; // Should be the width of the RectTransform of the inventorygrid

    private int _gridRows;
    private int _gridColumns;

    private int _slotSize = 32; // This is the size of the inventory slots on the inventory grid


    private InventorySlot[,] _inventorySlots; // Array of the slots

    #endregion

    private void Awake() {
        // Singleton pattern
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(this);
        }
        
        // Init
        Initialize();
    }

    private void Start() {
        InitializeInventoryGrid();
    }

    #region Methods

    private void Initialize() {
        // Subscribe to important callbacks
        Looting.OnTryToPickUp += OnTryToPickup;

        _gridColumns = _gridWidth / _slotSize;
        _gridRows = _gridHeight / _slotSize;

    }

    private void OnTryToPickup(WorldItem item) {
        if (TryToPickup(item)) {
            Debug.Log("Picked up item succesfully");
        } else { Debug.Log("Cannot pick up, no slots or stacks left"); }
    }

    private void InitializeInventoryGrid() {
        // Init array
        _inventorySlots = new InventorySlot[_gridColumns, _gridRows];

        // If inventory UI is not active, activate it - Send event to UiManager
        if (!UiManager._isInventoryOpen) {
            EventManager.TriggerOnOpenOrCloseInventory(true);
        }

        // Add new InventorySlots to each position on the array
        // Loop through columns 
        for (int column = 0; column < _gridColumns; column++) {
            // Loop throug rows
            for (int row = 0; row < _gridRows; row++) {
                
                // Instantiate new InventorySlot
                InventorySlot newSlot = Instantiate(InventorySlotPrefab, InventorySlotsContainer).GetComponent<InventorySlot>();

                // Set position info
                newSlot.SetGridPositionInfo(column, row);

                // Set the world position of the visual slot object
                newSlot.transform.localPosition = GetPositionOnGrid(column, row);
                
                // Add the new InventorySlot in the correct array position
                _inventorySlots[column, row] = newSlot;
            }
        }
    }

    private bool TryToPickup(WorldItem worldItem) {
        // Tries to add picked up item to inventory.
        // 1. Tries to add item to existing stacks
        // 2. If stacking fails, creates new stacks

        // Try to stack items
        for (int column = 0; column < _gridColumns; column++) { // Loop through rows
            for (int row = 0; row < _gridRows; row++) { // Loop through columns
                // Accessing the array with row as X and column as Y, so the horizontal rows will be filled before moving to next column

                if (
                    _inventorySlots[row, column].IsOoccupied() && // If slot is occupied
                    worldItem.GetItem().isStackable && // If item being added is stackable
                    _inventorySlots[row, column].GetOccupyingItem().GetItem() == worldItem.GetItem() && // If item being stacked is same type as the on in the slot
                    (_inventorySlots[row, column].GetOccupyingItem().GetStackSize() + worldItem.StackSize) <= _inventorySlots[row, column].GetOccupyingItem().GetItem().maxStackSize // Check if can increase the stack size
                    ) {

                    _inventorySlots[row, column].GetOccupyingItem().IncreaseStackSize(worldItem.StackSize);

                    // Exit the checking loop
                    return true;
                }
            }
        }

        // If stacking fails, Try to create a new stack
        Vector2Int itemSize = worldItem.GetItem().inventorySize;

        // Check item size and try to place it down
        bool isBiggerThan1x1 = (!(itemSize.x == 1) || !(itemSize.y == 1)); // If item size is bigger than 1x1

        if (isBiggerThan1x1) { // If Item is bigger than 1x1, check all nearby required slots
            // Debug.Log("Item bigger than 1x1");
            
            int horizontalSlotsNeeded = itemSize.x;
            int verticalSlotsNeeded = itemSize.y;

            for (int column = 0; column < _gridColumns; column++) {
                for (int row = 0; row < _gridRows; row++) {

                    if (CheckSlots(row, column, verticalSlotsNeeded, horizontalSlotsNeeded)) {
                        // If origin slot + surrounding slots are empty, place down the object

                        // Place item in slot
                        PlaceItemInSlot(worldItem, new Vector2Int(row, column));
                        // Debug.Log("Placed item in slot: [" + row + ", " + column + "]");

                        // Return so item won't be placed on all open slots
                        return true;
                    }
                }
            }
            return false;
        }

        // If item size is 1x1, check for free slot and try to place down
        for (int column = 0; column < _gridColumns; column++) {
            for (int row = 0; row < _gridRows; row++) {
                if (!_inventorySlots[row, column].IsOoccupied()) {
                    // If the slot is not occupied, place the item

                    // Place item in slot
                    PlaceItemInSlot(worldItem, new Vector2Int(row, column));
                    // Debug.Log("Placed item in slot: [" + row + ", " + column + "]");

                    // Return so item won't be placed on all open slots
                    return true;
                }
            }
        }

        // Cannot find empty slot, cancel trying
        return false;
    }

    public bool CheckSlots(int originSlotX, int originSlotY, int verticalSlotsToCheck, int horizontalSlotsToCheck) {
        // Checks if origin slot and needed surrounding slots are empty

        // Check if needed amount of slots exist on each side before checking if they are occupied (i.e. Prevent placing items partially outside of inventory)
        int remainingWidth = _gridColumns - (originSlotX + (horizontalSlotsToCheck - 1));
        int remainingHeight = _gridRows - (originSlotY + (verticalSlotsToCheck - 1));

        bool notEnoughSlotsNearby = (remainingWidth < 1) || (remainingHeight < 1);

        if (notEnoughSlotsNearby) {
            // Debug.Log("Not enough slots near the checked slot");
            return false;
        }

        // Check if the slots exist (are not outside of the inventory grid), and are empty 

        for (int y = 0; y < verticalSlotsToCheck; y++) {
            for (int x = 0; x < horizontalSlotsToCheck; x++) {
                // Check if slot is occupied
                if (_inventorySlots[originSlotX + x, originSlotY + y].IsOoccupied()) {
                    // Debug.Log("Slot [" + (originSlotX + x) + ", " + (originSlotY + y) + "] is occupied");
                    return false;
                }
            }
        }

        return true;

    }

    public bool CheckSlots(InventoryItem item, int originSlotX, int originSlotY, int horizontalSlotsToCheck, int verticalSlotsToCheck) {
        // Checks if the origin inventory slot and it's required surrounding slots are empty or contain the same item


        // Check if the slots are empty or contain the same item
        for (int y = 0; y < verticalSlotsToCheck; y++) {
            for (int x = 0; x < horizontalSlotsToCheck; x++) {
                // Store the slot for clarity
                int slotX = originSlotX + x;
                int slotY = originSlotY + y;

                // Check if the slot exists
                bool doesSlotExist = DoesSlotExist(slotX, slotY);

                // Exit if the slot does not exist (i.e. is outside of the grid)
                if (!doesSlotExist) {
                    return false;
                }

                // ----> Slot exists, continue checking
                
                // Calculate the slot to check
                InventorySlot slotToCheck = _inventorySlots[slotX, slotY];

                // Check if slot is occupied and the item is not the same
                if (slotToCheck.IsOoccupied() && slotToCheck.GetOccupyingItem() != item) {
                    return false;
                }
            }
        }

        return true;
    }

    public List<InventorySlot> GetInventorySlots(Vector2Int originSlot, int itemSizeX, int itemSizeY) {
        // Returns instances of inventory slots when supplied an origin slot coordinate and item size

        List<InventorySlot> slotsToReturn = new();

        for (int y = 0; y < itemSizeY; y++) {
            for (int x = 0; x < itemSizeX; x++) {
                
                int slotX = originSlot.x + x;
                int slotY = originSlot.y + y;

                // Check if the slot exists (Is not accessing a slot outside the grid)
                bool doesSlotExist = DoesSlotExist(slotX, slotY);

                // Add the slot to the list if
                if (doesSlotExist) {
                    slotsToReturn.Add(_inventorySlots[originSlot.x + x, originSlot.y + y]);
                }
            }
        }
       
        return slotsToReturn;
    }

    private bool DoesSlotExist(int x, int y) {
        // Check if the slot exists (Is not accessing a slot outside the grid)
        bool doesExist = x < _gridColumns && y < _gridRows;

        if (!doesExist) {
            return false;
        } else {
            return true;
        }
    }

    private void PlaceItemInSlot(WorldItem item, Vector2Int slot) {
        // Places item in a inventory slot on the grid, doesn't care if the slot is occupied. Occupied check is done in TryToPickUp()


        // Spawn the item, set parent to inventorygrid
        GameObject _spawnedItem = Instantiate(InventoryItemPrefab, InventoryGridTransform);

        // Store InventoryItem component, it will be used for occupying the slot
        InventoryItem _spawnedItemInvComponent = _spawnedItem.GetComponent<InventoryItem>();

        // Initialize the item (Set icon, stack size in inventory, etc.)
        _spawnedItemInvComponent.InitializeItem(item.GetItem(), item.StackSize);

        // Set item position
        _spawnedItem.GetComponent<RectTransform>().localPosition = new Vector3(slot.x * _slotSize, -slot.y * _slotSize, 0f);

        // Update item initial drag position
        _spawnedItemInvComponent._itemOldPosition = _spawnedItem.GetComponent<RectTransform>().localPosition;

        // Get the amount of slots the item will occupy
        int horizontalSlotsToOccupy = item.GetItem().inventorySize.x;
        int verticalSlotsToOccupy = item.GetItem().inventorySize.y;

        // Occupy the slots
        for (int y = 0; y < verticalSlotsToOccupy; y++) {
            for (int x = 0; x < horizontalSlotsToOccupy; x++) {
                _inventorySlots[(slot.x + x), (slot.y + y)].SetOccupied(true);
                _inventorySlots[(slot.x + x), (slot.y + y)].SetOccupyingItem(_spawnedItemInvComponent);

                // Add the slots to the occupied slots list on the item (used for moving the item to other slots by dragging)
                _spawnedItemInvComponent.AddToOccupiedSlots(_inventorySlots[(slot.x + x), (slot.y + y)]);

                // Debug.Log("Occupied slot: [" + (slot.x + h) + ", " + (slot.y + v) + "]");
            }
        }
    }

    public Vector3 GetPositionOnGrid(int gridPosX, int gridPosY) {
        // Returns a position on the grid based on grid coordinates
        Vector3 gridPosition = new Vector3(gridPosX * _slotSize, -gridPosY * _slotSize, 0f);

        return gridPosition;
    }

    #endregion

}
