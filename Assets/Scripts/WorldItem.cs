using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldItem : MonoBehaviour
{
    [SerializeField]
    private Item Item; // Ref to the item object which holds all the common information related to loot items
   
    public int StackSize = 1;

    public Item GetItem() {
        return Item;
    }
}
