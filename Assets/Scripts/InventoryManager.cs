using UnityEngine;

public class InventoryManager : MonoBehaviour {
    // Core of the inventory system. Sets up the virtual inventory grid and slots. Handles managing items in the inventory slots

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

    private bool CheckSlots(int originSlotX, int originSlotY, int verticalSlotsToCheck, int horizontalSlotsToCheck) {
        // Checks if origin slot and needed surrounding slots are empty

        // Debug.Log("CheckSlots origin (x,y): (" + originSlotX + "," + originSlotY + "), Slots to check (x,y): " + horizontalSlotsToCheck + verticalSlotsToCheck);

        // TODO: 
        // 1. Before checking if actual slots are empty, check if needed slots exist on the right and bottom side of the originSlot


        // Check if needed amount of slots exist on each side before checking if they are occupied (i.e. Prevent placing items partially outside of inventory)
        int remainingWidth = _gridColumns - (originSlotX + (horizontalSlotsToCheck - 1));
        int remainingHeight = _gridRows - (originSlotY + (verticalSlotsToCheck - 1));

        bool notEnoughSlotsNearby = (remainingWidth < 1) || (remainingHeight < 1);

        if (notEnoughSlotsNearby) {
            // Debug.Log("Not enough slots near the checked slot");
            return false;
        }

        // If needed amount of slots exist, proceed to check if the slots are empty

        int emptyVerticalSlots = 0;

        // Check vertical slots on each row below the origin
        for (int y = 0; y < verticalSlotsToCheck; y++) {

            // Check if slot is occupied
            if (!_inventorySlots[originSlotX, (originSlotY + y)].IsOoccupied()) {
                emptyVerticalSlots++;
            }
        }

        bool notEnoughVerticalSlots = emptyVerticalSlots != verticalSlotsToCheck;
        
        // If not enough vertical slots below the origin slot, exit loop
        if (notEnoughVerticalSlots) {
            // Debug.Log("Not enough vertical slots");
            return false;
        }

        // If enough vertical slots below origin, check horizontal slots 

        int emptyHorizontalSlots = 0;

        for (int y = 0; y < verticalSlotsToCheck; y++) { // Loop through each row
                                                         // Check if slot is occupied
            for (int x = 1; x < horizontalSlotsToCheck; x++) { // Loop through each column, start at 1 because we already checked 0 before

                if (!_inventorySlots[(originSlotX + x), (originSlotY + y)].IsOoccupied()) {
                    emptyHorizontalSlots++; // Exception happens on this line
                }
            }
        }

        // Debug.Log("Empty horizontal slots: " + emptyHorizontalSlots);

        int emptyHorizontalSlotsNeeded = (horizontalSlotsToCheck * verticalSlotsToCheck) - verticalSlotsToCheck;    // Take into account that item requires more than the horizontalSlotsToCheck to be placed
                                                                                                                    // (amount of needed horizontal slots on 3x2 item is 6 (4, if two checked verticals are excluded))

        if (emptyHorizontalSlots == emptyHorizontalSlotsNeeded) {
            // Return true if required slots are empty
            return true;
        } else {
            // Required slots are not empty
            // Debug.Log("Not enough horizontal slots");
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



        // Occupy inventory slots
        int horizontalSlotsToOccupy = item.GetItem().inventorySize.x;
        int verticalSlotsToOccupy = item.GetItem().inventorySize.y;

        // Occupy vertical slots on each row below the origin
        for (int i = 0; i < verticalSlotsToOccupy; i++) {
            _inventorySlots[slot.x, (slot.y + i)].SetOccupied(true);
            _inventorySlots[slot.x, (slot.y + i)].SetOccupyingItem(_spawnedItemInvComponent);
            // Debug.Log("Occupied slot: [" + slot.x + ", " + (slot.y + i) + "]");
        }

        // Occupy horizontal slots on each vertical row
        for (int v = 0; v < verticalSlotsToOccupy; v++) {
            for (int h = 1; h < horizontalSlotsToOccupy; h++) {
                _inventorySlots[(slot.x + h), (slot.y + v)].SetOccupied(true);
                _inventorySlots[(slot.x + h), (slot.y + v)].SetOccupyingItem(_spawnedItemInvComponent);
                // Debug.Log("Occupied slot: [" + (slot.x + h) + ", " + (slot.y + v) + "]");
            }
        }

        // Set occupied item type for stacking
        _inventorySlots[slot.x, slot.y].SetOccupyingItem(_spawnedItemInvComponent);
    }

    #endregion

    private class InventorySlot {
        // Stores information about each inventory slot, the virtual grid of inventory slots is built out of these

        #region Properties

        private bool _occupied = false;
        private InventoryItem _occupyingItem;

        #endregion

        #region Methods

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

        #endregion
    }
}
