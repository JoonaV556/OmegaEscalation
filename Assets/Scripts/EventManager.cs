using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public static event Action<bool> OnOpenOrCloseInventory;

    public static void TriggerOnOpenOrCloseInventory(bool openOrClose) { // True for open, false for close
        OnOpenOrCloseInventory?.Invoke(openOrClose);
    }
}
