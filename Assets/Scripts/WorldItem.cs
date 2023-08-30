using UnityEngine;

public class WorldItem : MonoBehaviour
{
    // Holds information related to each physical item which can be picked up in the world

    #region Properties

    [SerializeField]
    private Item Item; // Ref to the item object which holds all the common information related to loot items
   
    public int StackSize = 1;

    #endregion



    #region Methods 

    public Item GetItem() {
        return Item;
    }

    #endregion
}
