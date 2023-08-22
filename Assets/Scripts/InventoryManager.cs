using UnityEngine;

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
        // Used for picking up items in the world, checks for empty inventory slots before placing items

        // 1. Find empty slot (fill row first, then row on next column 
        // 2. later - If slot is not empty, check if item is same type and stackable -> Stack the item with the existing item
        // 3. Place item in empty slot

        // Loop through columns
        for (int column = 0; column < _gridColumns; column++) {

            // Loop throug rows
            for (int row = 0; row < _gridRows; row++) {

                // Check if slot is occupied - if not - place item in the slot
                // Accessing the array with row as X and column as Y, so the horizontal rows fill be filled before moving to next column
                if (!_inventorySlots[row, column].IsOoccupied()) {
                    // Place item in slot
                    PlaceItemInSlot(worldItem, new Vector2Int(row, column));
                    
                    // Return so item won't be placed on all open slots
                    return;

                } else if (worldItem.GetItem().isStackable && _inventorySlots[row, column].GetOccupyingItem() == worldItem.GetItem()) {
                    // If slot is occupied,
                    // check if item being added is stackable && item being added is the same type as the item in the slot
                    
                    // Add the item to the existing stack 
                    _inventorySlots[row, column].GetOccupyingItem().IncreaseStackSize(worldItem.StackSize);
                    
                    // Exit the checking loop
                    return;
                }
            }
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

        // Set slot to occupied
        _inventorySlots[slot.x, slot.y].SetOccupied(true);
        // Set occupied item type for stacking
        _inventorySlots[slot.x, slot.y].SetOccupyingItem(_spawnedItemInvComponent);
    }

    // Only for testing
    private void AddToInventory(WorldItem worldItem) {

        // Spawn the item & Init values
        GameObject newInventoryItem = Instantiate(InventoryItemPrefab);
        newInventoryItem.GetComponent<InventoryItem>().InitializeItem(worldItem.GetItem(), worldItem.StackSize);

        // Add the item as a child of the inventory grid 
        newInventoryItem.transform.SetParent(InventoryGridTransform, false);
        // Set the item position in the grid
        newInventoryItem.transform.localPosition = new Vector3(0f, 0f, 0f);

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
