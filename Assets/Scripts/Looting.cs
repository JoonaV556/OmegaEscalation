using System;
using UnityEngine;

public class Looting : MonoBehaviour
{
    public static event Action<bool> OnCanLootChanged;

    #region Properties

    private int _lootLayerMask = 8; // Checks loot items with a raycast only in this layer
    private int _layerMask;
    private float _screenWidth;
    private float _screenHeight;
    private Transform _camTransform;
    private Camera _mainCamera; // Loot raycast is done from the center point of this camera
    private float _maxLootDistance = 1.5f; // Maximum distance in meters from which the player can pickup loot

    private bool _wasPickupPressedThisFrame;
    private bool _canPickUp = false;

    #endregion

    private void Start() {
        Initialize();
    }

    private void Initialize() {
        _screenWidth = Screen.width;
        _screenHeight = Screen.height;
        _mainCamera = Camera.main;
        _camTransform = _mainCamera.transform;
        _layerMask = 1 << _lootLayerMask;
    }

    private void FixedUpdate() {
        CheckForLoot();
    }
    
    private void CheckForLoot() {

        RaycastHit hit;

        // Do a raycast from the center point of the screen to the direction the player is looking at
        // If the raycast hits a lootable item, loot indicator appears
        // If the player presses loot key, item is looted
        if (Physics.Raycast(_mainCamera.ScreenToWorldPoint(new Vector3(_screenWidth, _screenHeight, 0f)), _camTransform.forward, out hit, _maxLootDistance, _lootLayerMask)) {
            // Debug, remove
            Debug.DrawRay(_mainCamera.ScreenToWorldPoint(new Vector3(_screenWidth, _screenHeight, 0f)), _camTransform.forward * _maxLootDistance, Color.green, 1f);
            Debug.Log("Hit loot item");

            // Store reference to the item to be picked up
            WorldItem itemToPickUp = hit.transform.gameObject.GetComponent<WorldItem>();

            // Ivoke event if item can be picked up
            // Pickup UI indicator is activated by UIManager
            if (!_canPickUp) {
                _canPickUp = true;
                OnCanLootChanged?.Invoke(true);
            }

            // Pick up the item if pressed loot key
            if (_wasPickupPressedThisFrame) {
                Debug.Log("Tried to pick up loot");

                TryToPickUp(itemToPickUp);
            }

        } else {
            // Debug, remove
            Debug.DrawRay(_mainCamera.ScreenToWorldPoint(new Vector3(_screenWidth, _screenHeight, 0f)), _camTransform.forward * _maxLootDistance, Color.green, 1f);

            // Ivoke event if item cannot be picked up
            // Pickup UI indicator is disabled by UIManager
            if (_canPickUp) {
                _canPickUp = false;
                OnCanLootChanged?.Invoke(false);
            }

            // Disable picking up for the next frame
            _wasPickupPressedThisFrame = false;

            return;
        }

        // Disable picking up for the next frame
        _wasPickupPressedThisFrame = false;
    }

    private void TryToPickUp(WorldItem item) {
        // Pick up item logic
    }

    private void OnPickup() {
        // Triggered when player presses the pickup key
        _wasPickupPressedThisFrame = true;
    }
}
