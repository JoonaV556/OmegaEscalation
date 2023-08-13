using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Inventory : MonoBehaviour
{
    [SerializeField] private GameObject InvBag;
    [SerializeField] private GameObject InvBackground;
    [SerializeField] private GameObject[] debugButtons;

    [SerializeField] private PlayerInput inpt;

    void Update() {

        // Check if the "I" key is currently being held down
        if (Input.GetKeyDown(KeyCode.I)) {
            ToggleInventory();
        }

        if (Input.GetKeyDown(KeyCode.M)) {
            ToggleCursor();
        }
    }

    private void ToggleInventory() {

        switch (InvBag.activeInHierarchy) {
            case true:
                InvBag.SetActive(false);
                InvBackground.SetActive(false);
                foreach (GameObject debugButton in debugButtons) { debugButton.SetActive(false); }

                inpt.actions.FindActionMap("Player").Enable();
                break;
            case false:
                InvBag.SetActive(true);
                InvBackground.SetActive(true);
                foreach (GameObject debugButton in debugButtons) { debugButton.SetActive(true); }

                inpt.actions.FindActionMap("Player").Disable();
                break;
        }
    }

    private void ToggleCursor() {
        switch (Cursor.visible) {
            case true:
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked; 
                break;
            case false:
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                break;
        }
    }
}
