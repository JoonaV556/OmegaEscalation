using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Object/Item")]
public class Item : ScriptableObject
{
    public Sprite image;

    public bool stackable = true;


    public enum ItemType {
        Tool,
        Weapon,
        Material
    }
}
