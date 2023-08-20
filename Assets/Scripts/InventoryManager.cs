using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{

    [SerializeField]
    private GameObject InventoryItemPrefab; // Prefab used for spawning items in the inventory
    [SerializeField]
    private Transform InventoryGridTransform; // Transform component of the inventory grid, items will be instantiated as a child of this transform

    private void Awake() {
        Initialize();
    }

    private void Initialize() {
        // Subscribe to important callbacks
        Looting.OnTryToPickUp += TryToPickup;
    }

    private void TryToPickup(WorldItem worldItem) {
        // Used for picking up items in the world


        // Testing
        AddToInventory(worldItem);

    }

    // private bool CheckForEmptySlot() {
    // 
    // 
    // 
    // 
    // }

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
}
