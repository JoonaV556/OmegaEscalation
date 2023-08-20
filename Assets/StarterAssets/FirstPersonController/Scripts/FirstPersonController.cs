using DG.Tweening;
using System;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets {
    [RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM
    [RequireComponent(typeof(PlayerInput))]
#endif
    public class FirstPersonController : MonoBehaviour {

        // Controller uses this enum to make changes between movement states cleaner to code
        // No movement state for standing still because that is not needed
        public enum MovementState {
            Walking,
            Sprinting,
            Crouching,
            Sneaking
        }

        // Crouch start and exit are done by subscribing to the input asset callbacks in the Awake() method

        #region SerializedProperties

        [Header("Input")]
        [SerializeField]
        private InputActionAsset InputActionAsset;

        [Space(10)]
        [Header("Player")]
        [Tooltip("Move speed of the character in m/s")]
        public float MoveSpeed = 4.0f;
        [Tooltip("Sprint speed of the character in m/s")]
        public float SprintSpeed = 6.0f;
        [Tooltip("Sneak & crouch speed of the character in m/s")]
        public float SneakSpeed = 2.5f;
        [Tooltip("Rotation speed of the character")]
        public float RotationSpeed = 1.0f;
        [Tooltip("Acceleration and deceleration")]
        public float SpeedChangeRate = 10.0f;
        [SerializeField]
        private MovementState CurrentMovementState = MovementState.Walking;

        [Space(10)]
        [Header("Stamina")]
        [SerializeField]
        private float MaxStamina = 100f;
        [SerializeField]
        [Tooltip("Stamina regen amount per second. Regen only when not sprinting or jumping")]
        private float StaminaRegenAmount = 8f;
        [SerializeField]
        [Tooltip("Sprint stamina cost per second")]
        private float SprintStaminaCost = 10f;
        [SerializeField]
        [Tooltip("Jump stamina cost, a flat amount per jump")]
        private float JumpStaminaCost = 20f;


        [Space(10)]
        [Tooltip("The height the player can jump")]
        public float JumpHeight = 1.2f;
        [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
        public float Gravity = -15.0f;
        [SerializeField]
        private float StandingHeight = 2f;
        [SerializeField]
        private float CrouchedHeight = 1f;
        [SerializeField]
        [Tooltip("Crouch speed in seconds - i.e. how long it takes to enter full crouch")]
        private float CrouchSpeed = 0.05f;

        [Space(10)]
        [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
        public float JumpTimeout = 0.1f;
        [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
        public float FallTimeout = 0.15f;

        [Header("Player Grounded")]
        [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
        public bool Grounded = true;
        [Tooltip("Useful for rough ground")]
        public float GroundedOffset = -0.14f;
        [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
        public float GroundedRadius = 0.5f;
        [Tooltip("What layers the character uses as ground")]
        public LayerMask GroundLayers;

        [Header("Cinemachine")]
        [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
        public GameObject CinemachineCameraTarget;
        [Tooltip("How far in degrees can you move the camera up")]
        public float TopClamp = 90.0f;
        [Tooltip("How far in degrees can you move the camera down")]
        public float BottomClamp = -90.0f;

        #endregion

        #region PrivateProperties

        // Input
        private InputActionMap _playerActionMap; // Use this for disabling specific actions or action maps

        // cinemachine
        private float _cinemachineTargetPitch;

        // player
        private float _speed;
        private float _rotationVelocity;
        private float _verticalVelocity;
        private float _terminalVelocity = 53.0f;

        // timeout deltatime
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;

        // Crouch tween reference
        // Assigned when entering crouch, null when exited crouch
        private Tween _crouchTween;

        // Stamina
        private float _currentStamina;




#if ENABLE_INPUT_SYSTEM
        private PlayerInput _playerInput;
#endif
        private CharacterController _controller;
        private StarterAssetsInputs _input;
        private GameObject _mainCamera;

        private const float _threshold = 0.01f;

        #endregion

        private void Initialize() {

            // Set current stamina
            _currentStamina = MaxStamina;

            // reset our timeouts on start
            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;

            // Init important values
            _controller.height = StandingHeight;

            // Get ref to Player actionMap, This map will be disabled when the mouse cursor is unlocked
            _playerActionMap = InputActionAsset.FindActionMap("Player");

            // Subscribe to UiManager callbacks
            UiManager.OnCursorLocked += EnableMovementInput;
            UiManager.OnCursorUnlocked += DisableMovementInput;
        }

        private void Awake() {
            // get a reference to our main camera
            if (_mainCamera == null) {
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }

            // Find references to input actions in the asset
            // Bind action callbacks to methods in this script
            if (InputActionAsset != null) {
                InputActionAsset.FindActionMap("Player").FindAction("Crouch").started += OnCrouchPressed;
                InputActionAsset.FindActionMap("Player").FindAction("Crouch").canceled += OnCrouchReleased;
            } else { Debug.LogError("Input action asset not assigned, movement won't work properly!"); }
        }

        private void Start() {
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<StarterAssetsInputs>();
#if ENABLE_INPUT_SYSTEM
            _playerInput = GetComponent<PlayerInput>();
#else
			Debug.LogError( "Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif

            // Initialize important values
            Initialize();

        }

        private void Update() {
            JumpAndGravity();
            GroundedCheck();
            Move();

            UpdateStamina();
        }

        private void LateUpdate() {
            CameraRotation();
        }

        private void UpdateStamina() {

            // Reduce stamina if sprinting & golding shift & over 4 speed
            if (CurrentMovementState == MovementState.Sprinting && _speed >= 4f) {
                _currentStamina -= SprintStaminaCost * Time.deltaTime;
            }

            // Regen stamina if not sprinting or jumping 
            if (_speed <= 4f && Grounded) {
                _currentStamina += StaminaRegenAmount * Time.deltaTime;
            }

            // Clamp current stamina so it won't go below 0 or higher than max stam
            _currentStamina = Mathf.Clamp(_currentStamina, 0f, MaxStamina);
        }

        private void DisableMovementInput() {
            _playerActionMap.Disable();
        }

        private void EnableMovementInput() {
            _playerActionMap.Enable();
        }

        #region OriginalMethods

        private bool IsCurrentDeviceMouse {
            get {
#if ENABLE_INPUT_SYSTEM
                return _playerInput.currentControlScheme == "KeyboardMouse";
#else
				return false;
#endif
            }
        }

        private void GroundedCheck() {
            // set sphere position, with offset
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
            Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);
        }

        private void CameraRotation() {
            // if there is an input
            if (_input.look.sqrMagnitude >= _threshold) {
                //Don't multiply mouse input by Time.deltaTime
                float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

                _cinemachineTargetPitch += _input.look.y * RotationSpeed * deltaTimeMultiplier;
                _rotationVelocity = _input.look.x * RotationSpeed * deltaTimeMultiplier;

                // clamp our pitch rotation
                _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

                // Update Cinemachine camera target pitch
                CinemachineCameraTarget.transform.localRotation = Quaternion.Euler(_cinemachineTargetPitch, 0.0f, 0.0f);

                // rotate the player left and right
                transform.Rotate(Vector3.up * _rotationVelocity);
            }
        }

        private void Move() {
            // Sets movement state to sprint if sprint is pressed + transitions
            // & Check if enough stamina to sprint
            // & Check if not jumping
            if (
                _input.sprint &&
                _currentStamina > 5f &&
                Grounded &&
                _jumpTimeoutDelta <= 0.0f
                ) {
                switch (CurrentMovementState) {
                    case MovementState.Walking:
                        // Change to sprint
                        CurrentMovementState = MovementState.Sprinting;
                        break;
                    case MovementState.Crouching:
                        // Stop crouching before sprinting
                        ExitCrouch();
                        break;
                    case MovementState.Sneaking:
                        // Stop sneaking before sprinting
                        StopSneak();
                        break;
                    default:
                        // Keep sprinting
                        CurrentMovementState = MovementState.Sprinting;
                        break;
                }
            }

            // Return back to walking if sprint key is no longer pressed
            if (!_input.sprint && CurrentMovementState == MovementState.Sprinting) {

                CurrentMovementState = MovementState.Walking;
            }

            // Stop sprinting if there is no stamina left
            if (CurrentMovementState == MovementState.Sprinting && _currentStamina < 5f) {
                CurrentMovementState = MovementState.Walking;
            }

            // Variable used by the charactercontroller to change movementspeed smoothly using curves
            // Change this to change movement speed: sneaking, sprinting etc.
            float targetSpeed;

            // Sets target speed based on current movement state
            switch (CurrentMovementState) {
                case MovementState.Walking:
                    targetSpeed = MoveSpeed;
                    break;
                case MovementState.Sprinting:
                    targetSpeed = SprintSpeed;
                    break;
                case MovementState.Crouching:
                    targetSpeed = SneakSpeed;
                    break;
                case MovementState.Sneaking:
                    targetSpeed = SneakSpeed;
                    break;
                default:
                    targetSpeed = MoveSpeed;
                    break;
            }

            // a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

            // note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is no input, set the target speed to 0
            if (_input.move == Vector2.zero) targetSpeed = 0.0f;

            // a reference to the players current horizontal velocity
            float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

            float speedOffset = 0.1f;
            float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

            // accelerate or decelerate to target speed
            if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset) {
                // creates curved result rather than a linear one giving a more organic speed change
                // note T in Lerp is clamped, so we don't need to clamp our speed
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);

                // round speed to 3 decimal places
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            } else {
                _speed = targetSpeed;
            }

            // normalise input direction
            Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

            // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is a move input rotate player when the player is moving
            if (_input.move != Vector2.zero) {
                // move
                inputDirection = transform.right * _input.move.x + transform.forward * _input.move.y;
            }

            // move the player
            _controller.Move(inputDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
        }

        private void JumpAndGravity() {
            if (Grounded) {
                // reset the fall timeout timer
                _fallTimeoutDelta = FallTimeout;

                // stop our velocity dropping infinitely when grounded
                if (_verticalVelocity < 0.0f) {
                    _verticalVelocity = -2f;
                }

                // jump timeout
                if (_jumpTimeoutDelta >= 0.0f) {
                    _jumpTimeoutDelta -= Time.deltaTime;
                }

            } else {
                // reset the jump timeout timer
                _jumpTimeoutDelta = JumpTimeout;

                // fall timeout
                if (_fallTimeoutDelta >= 0.0f) {
                    _fallTimeoutDelta -= Time.deltaTime;
                }

                // if we are not grounded, do not jump
                _input.jump = false;
            }

            // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
            if (_verticalVelocity < _terminalVelocity) {
                _verticalVelocity += Gravity * Time.deltaTime;
            }
        }

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax) {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

        private void OnDrawGizmosSelected() {
            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            if (Grounded) Gizmos.color = transparentGreen;
            else Gizmos.color = transparentRed;

            // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
            Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z), GroundedRadius);
        }

        private void OnJump() {
            if (_jumpTimeoutDelta <= 0.0f && (_currentStamina - JumpStaminaCost) >= 0f) {
                // the square root of H * -2 * G = how much velocity needed to reach desired height
                _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

                // Deduct stamina
                _currentStamina -= JumpStaminaCost;

                // Reset jump timeOut to prevent deducting stamina more than once during a single jump
                _jumpTimeoutDelta = JumpTimeout;
            }
        }

        #endregion

        #region Crouching

        private void OnCrouchPressed(InputAction.CallbackContext context) {

            // Cancel crouch if character is jumping
            if (!Grounded && _jumpTimeoutDelta >= 0.0f) { return; }

            if (CurrentMovementState != MovementState.Sprinting) {
                // Start crouch if not sprinting
                StartCrouch();
            } else if (CurrentMovementState == MovementState.Sneaking) {
                // Stop sneaking before crouching
                StopSneak();
                StartCrouch();
            }
        }

        private void OnCrouchReleased(InputAction.CallbackContext context) {
            if (CurrentMovementState == MovementState.Crouching) {
                ExitCrouch();
            }
        }

        private void StartCrouch() {
            // Do crouch stuff
            CurrentMovementState = MovementState.Crouching;


            // Flip crouch direction back to crouch if already in crouch and trying to stop
            if (_crouchTween != null && _crouchTween.IsBackwards()) {
                _crouchTween.Flip();
                return;
            }

            // Crouch
            // Tween CharacterControllers collider height smoothly
            // Autokill is set to false so crouch can be reversed when after entering full crouch
            _crouchTween = DOTween.To(GetControllerHeight, SetControllerHeight, CrouchedHeight, CrouchSpeed)
                .SetEase(Ease.Linear)
                .OnComplete(OnCrouchTweenComplete)
                .SetAutoKill(false)
                .OnRewind(OnCrouchTweenComplete);

            // TODO: Do other crouch stuff...
        }

        private void ExitCrouch() {

            // Reverse crouching action
            if (_crouchTween != null) {
                _crouchTween.Flip();
                _crouchTween.Play();
            }
        }

        private void OnCrouchTweenComplete() {
            // Triggered usually when crouch has ended

            // Kill the tween and set to null if crouching has ended and returned to standing position
            if (_crouchTween.IsBackwards()) {
                _crouchTween.Kill();
                _crouchTween = null;

                // TODO: Stop doing other crouch stuff...
                CurrentMovementState = MovementState.Walking;
            }
        }

        private void SetControllerHeight(float newHeight) {
            _controller.height = newHeight;
        }

        private float GetControllerHeight() {
            return _controller.height;
        }

        #endregion

        #region Sneaking

        // Triggered when sneak button is pressed
        // PlayerInput component sends a message which triggers this
        private void OnSneak() {
            // Stop sneaking if sneaking
            if (CurrentMovementState == MovementState.Sneaking) {
                StopSneak();
                return;
            }

            // Cancel sneak if character is jumping
            if (!Grounded && _jumpTimeoutDelta >= 0.0f) { return; }

            // Sneak if walking (Sneak only allowed when walking)
            if (CurrentMovementState == MovementState.Walking) {
                StartSneak();
                return;
            }
        }

        private void StartSneak() {
            CurrentMovementState = MovementState.Sneaking;

            // TODO: Do other sneak stuff
        }

        private void StopSneak() {
            CurrentMovementState = MovementState.Walking;

            // TODO: Stop Sneak stuff
        }

        #endregion
    }
}