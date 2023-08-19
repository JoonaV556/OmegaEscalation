using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TutInventoryManager : MonoBehaviour {
    
    // !! Does not yet have max stack size feature, should probably be added to individual items !!

    public ItemSlot[] invSlots; // Array of inventory slots to look through when adding a new item - Inventory slots must be added manually in the inspector
    public GameObject invItemPrefab;

    // Debug for testing pickups
    public ItemReference[] itemsToPickup;

    public bool TryToPickup(ItemReference item) {
        // Tries to add an item added supplied as parameter to the first found empty inventory slot

        // Try to find existing stack of same item
        for (int i = 0; i < invSlots.Length; i++) {
            ItemSlot slot = invSlots[i]; 
            InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();

            // If the item in the invSlot is the same type as the one we're trying to add, increase the existing stacksize
            if (itemInSlot != null && itemInSlot._item == item) {
                itemInSlot.stackSize++;
                // Refresh the inventoryItems stack size text graphic
                itemInSlot.RefreshStackSizeUI();
                return true;
            }
        }

        // Find first empty slot in inventory
        // Order: 1. Inventory bag slots 2. Toolbar
        for (int i = 0; i < invSlots.Length; i++) {           
            if (invSlots[i].IsSlotTaken() == false) {
                ItemSlot slot = invSlots[i];
                SpawnNewItem(item, slot);

                return true;
            }
        }

        return false;
    }

    void SpawnNewItem(ItemReference itemToSpawn, ItemSlot slot) {
        // Spawns a new item/itemStack inside an inventory slot
        // !! Does not check if the slot is empty !! 

        // Spawn UI Image gameobject as a child of the inventory slot
        GameObject newItemObj = Instantiate(invItemPrefab, slot.transform);
        // Get the items InventoryItem component 
        InventoryItem newInvItem = newItemObj.GetComponent<InventoryItem>();
        // Initialize the new inventoryItems info with the supplied Item ScriptableObject
        newInvItem.InitializeItem(itemToSpawn);
    }

    public void AddToInventory(ItemReference item) {
        TryToPickup(item);
    }
}
