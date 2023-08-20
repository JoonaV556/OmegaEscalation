using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Scriptable Object/Item")]
public class Item : ScriptableObject
{
    public Sprite icon; // Item icon used in inventory

    public bool isStackable = true; 

    public ItemType type = ItemType.Other; // Item category

    public Vector2Int inventorySize = new Vector2Int(1, 1); // Size in the inventory grid

    public enum ItemType {
        Tool,
        Weapon,
        Material,
        Other
    }
}
