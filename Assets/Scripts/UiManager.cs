using StarterAssets;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class UiManager : MonoBehaviour {
    // General manager for various UI elements (Opens and closes the inventory on key presses, for example)

    #region Properties

    public static event Action OnCursorUnlocked;
    public static event Action OnCursorLocked;
    public static event Action OnInventoryOpened;

    [SerializeField]
    private InputActionAsset _inputActions;

    [SerializeField]
    private GameObject _inventoryGrid;
    [SerializeField]
    private GameObject _pickupIndicator;

    public static bool _isInventoryOpen { private set; get; } // Whether or not the inventory UI is open 

    #endregion

    private void OnEnable() {
        // Subscribe to important callbacks
        Looting.OnCanLootChanged += OnCanLootChanged;
        EventManager.OnOpenOrCloseInventory += OnRequestOpenInventory;
    }

    private void OnDisable() {
        // Unsubscribe from important callbacks
        Looting.OnCanLootChanged -= OnCanLootChanged;
        EventManager.OnOpenOrCloseInventory -= OnRequestOpenInventory;
    }

    private void Awake() {
        Initialize();
    }

    #region Methods

    private void Initialize() {
        // Find references to input actions in the asset
        // Bind action callbacks to methods in this script
        if (_inputActions != null) {

            // Enable inputActions
            _inputActions.Enable();

            // Bind actions to methods in the script
            _inputActions.FindActionMap("Inventory").FindAction("ToggleInventory").started += OnInventoryKeyPressed;
            _inputActions.FindActionMap("Inventory").FindAction("ToggleCursor").started += OnToggleCursorPressed;
        } else { Debug.LogError("Input action asset not assigned, movement won't work properly!"); }

        // Initialize public variables
        switch (_inventoryGrid.activeInHierarchy) {
            case true:
                _isInventoryOpen = true;
                break;
            case false:
                _isInventoryOpen = false;
                break;
        }
    }

    private void OnInventoryKeyPressed(InputAction.CallbackContext context) {
        ToggleInventory();
    }

    private void OnToggleCursorPressed(InputAction.CallbackContext context) {
        switch (Cursor.visible) {
            case true:
                // Disable player input
                OnCursorLocked?.Invoke();
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                break;
            case false:
                // Disable player input
                OnCursorUnlocked?.Invoke();
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                break;
        }
    }

    private void OnCanLootChanged(bool canloot) {

        // Disable or enable pickup indicator if the player can or cannot loot item
        if (canloot && !_pickupIndicator.activeInHierarchy) {
            _pickupIndicator.SetActive(true);
        } else if (!canloot && _pickupIndicator.activeInHierarchy) {
            _pickupIndicator.SetActive(false);
        }
    }

    public void ToggleInventory() {

        switch (_inventoryGrid.activeInHierarchy) {
            case true:
                _inventoryGrid.SetActive(false);
                _isInventoryOpen = false;
                break;
            case false:
                _inventoryGrid.SetActive(true);
                _isInventoryOpen = true;
                OnInventoryOpened?.Invoke();
                break;
        }
    }

    private void OnRequestOpenInventory(bool openOrClose) {
        switch (openOrClose) {
            case true:
                OpenInventory();
                break;
            case false:
                CloseInventory();
                break;
        }
    }

    private void OpenInventory() {
        _inventoryGrid.SetActive(true);
        _isInventoryOpen = true;
        OnInventoryOpened?.Invoke();
    }

    private void CloseInventory() {
        _inventoryGrid.SetActive(false);
        _isInventoryOpen = false;
    }

    #endregion
}
