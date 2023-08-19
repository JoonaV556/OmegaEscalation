using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    private int _lootLayerMask = 8; // Checks loot items with a raycast only in this layer
    private int _layerMask;
    private float _screenWidth;
    private float _screenHeight;
    private Transform _camTransform;
    private Camera _mainCamera; // Loot raycast is done from the center point of this camera
    private float _maxLootDistance = 1.5f; // Maximum distance in meters from which the player can pickup loot


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
        // Do a raycast
        // If raycast hits loot item
        // -> Check if player pressed loot key
        // else -> return
        
        RaycastHit hit;
        // Do a raycast from the center point of the screen to the direction the player is looking at
        // If the raycast hits a lootable item, loot indicator appears
        // If the player presses loot key, item is looted
        if (Physics.Raycast(_mainCamera.ScreenToWorldPoint(new Vector3(_screenWidth, _screenHeight, 0f)), _camTransform.forward, out hit, _maxLootDistance, _lootLayerMask)) {
            Debug.DrawRay(_mainCamera.ScreenToWorldPoint(new Vector3(_screenWidth, _screenHeight, 0f)), _camTransform.forward * _maxLootDistance, Color.green, 1f);
            Debug.Log("Hit loot item");
        } else {
            Debug.DrawRay(_mainCamera.ScreenToWorldPoint(new Vector3(_screenWidth, _screenHeight, 0f)), _camTransform.forward * _maxLootDistance, Color.green, 1f);
            return;
        }
    }

    private void TryToPickUp() {

    }
}
