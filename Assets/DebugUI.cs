using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugUI : MonoBehaviour
{
    // TODO: REMOVE, just for testing ui
    private void Update() {
        if (Input.GetKeyDown(KeyCode.I)) {
            if (Cursor.visible == false) {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
        }
    }
}
