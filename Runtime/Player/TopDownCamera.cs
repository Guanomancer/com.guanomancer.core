using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Guanomancer.Player
{
    /// <summary>
    /// Top-down orbit camera controller.
    /// Handles ground panning, camera-aligned movement, keyboard/mouse orbit, and smooth zoom with Cinemachine 3.x.
    /// </summary>
    [SelectionBase]
    [DisallowMultipleComponent]
    public class TopDownOrbitCameraController : MonoBehaviour
    {
        [Header("Cinemachine References")]
        [Tooltip("The CinemachineCamera controlling this view.")]
        [SerializeField] private CinemachineCamera _virtualCamera;

        [Tooltip("The CinemachineOrbitalFollow component on the virtual camera.")]
        [SerializeField] private CinemachineOrbitalFollow _orbitalFollow;

        [Header("Input Action References (New Input System)")]
        [Tooltip("Input Action for camera movement/panning (Vector2, e.g. Player/Move). Falls back to WASD/Arrows if unassigned.")]
        [SerializeField] private InputActionReference _panAction;

        [Tooltip("Input Action for fast panning/sprint (Button, e.g. Player/Sprint). Falls back to Shift if unassigned.")]
        [SerializeField] private InputActionReference _sprintAction;

        [Tooltip("Input Action for keyboard rotation/orbit (Vector2 or Axis, e.g. Q/E keys). Falls back to Q/E if unassigned.")]
        [SerializeField] private InputActionReference _keyboardOrbitAction;

        [Tooltip("Input Action for mouse orbit delta (Vector2, e.g. Player/Look or Pointer Delta). Falls back to Mouse Delta if unassigned.")]
        [SerializeField] private InputActionReference _mouseOrbitDeltaAction;

        [Tooltip("Input Action for mouse orbit hold trigger (Button, e.g. UI/RightClick). Falls back to Right Mouse Button if unassigned.")]
        [SerializeField] private InputActionReference _mouseOrbitHoldAction;

        [Tooltip("Input Action for zooming (Vector2 or Axis, e.g. UI/ScrollWheel). Falls back to Mouse Scroll if unassigned.")]
        [SerializeField] private InputActionReference _zoomAction;

        [Tooltip("Input Action for middle mouse drag-pan hold trigger (Button, e.g. UI/MiddleClick). Falls back to Middle Mouse Button if unassigned.")]
        [SerializeField] private InputActionReference _dragPanHoldAction;

        [Tooltip("Input Action for middle mouse drag-pan delta (Vector2, e.g. Player/Look or Pointer Delta). Falls back to Mouse Delta if unassigned.")]
        [SerializeField] private InputActionReference _dragPanDeltaAction;

        [Header("Pan / Move Settings")]
        [Tooltip("Base speed when panning across the ground.")]
        [SerializeField] private float _panSpeed = 25f;

        [Tooltip("Speed multiplier when holding Shift (Sprint).")]
        [SerializeField] private float _fastPanMultiplier = 2.5f;

        [Tooltip("Smoothing factor for ground panning (higher = more responsive).")]
        [SerializeField] private float _panDamping = 12f;

        [Tooltip("Enable screen edge scrolling when cursor is near the window border.")]
        [SerializeField] private bool _enableEdgePanning = false;

        [Tooltip("Border thickness in pixels for edge panning.")]
        [SerializeField] private float _edgePanBorderThickness = 15f;

        [Tooltip("Enable panning by dragging the Middle Mouse Button.")]
        [SerializeField] private bool _enableMiddleMousePan = true;

        [Tooltip("Sensitivity multiplier for mouse drag panning.")]
        [SerializeField] private float _mousePanSensitivity = 0.04f;

        [Header("Orbit / Rotation Settings")]
        [Tooltip("Rotation speed in degrees per second when using Q/E keys.")]
        [SerializeField] private float _keyboardRotationSpeed = 120f;

        [Tooltip("Mouse sensitivity when orbiting via Right Mouse Button or Middle Mouse Button.")]
        [SerializeField] private float _mouseOrbitSensitivity = 0.5f;

        [Tooltip("Smoothing factor for rotation interpolation.")]
        [SerializeField] private float _rotationDamping = 15f;

        [Tooltip("Allow vertical pitch angle adjustments with mouse orbit.")]
        [SerializeField] private bool _allowPitchAdjustment = true;

        [Tooltip("Minimum pitch angle in degrees (lowest angle from ground).")]
        [SerializeField] private float _minPitch = 25f;

        [Tooltip("Maximum pitch angle in degrees (highest angle, near top-down).")]
        [SerializeField] private float _maxPitch = 75f;

        [Tooltip("Initial pitch angle on start.")]
        [SerializeField] private float _defaultPitch = 50f;

        [Tooltip("Initial yaw angle on start (e.g. 45 degrees for isometric).")]
        [SerializeField] private float _defaultYaw = 45f;

        [Header("Zoom Settings")]
        [Tooltip("Minimum distance from target pivot (closest zoom).")]
        [SerializeField] private float _minZoomDistance = 16f;

        [Tooltip("Maximum distance from target pivot (furthest zoom).")]
        [SerializeField] private float _maxZoomDistance = 45f;

        [Tooltip("Initial zoom distance on start.")]
        [SerializeField] private float _defaultZoomDistance = 22f;

        [Tooltip("Distance changed per scroll step.")]
        [SerializeField] private float _zoomSensitivity = 3.5f;

        [Tooltip("Smoothing factor for zoom interpolation.")]
        [SerializeField] private float _zoomDamping = 10f;

        [Tooltip("Automatically tilt pitch with respect to the player's set pitch when zooming.")]
        [SerializeField] private bool _autoTiltOnZoom = true;

        [Tooltip("How fast the camera tilts to match the zoom level (higher = faster/snappier tilt).")]
        [SerializeField] private float _autoTiltSpeed = 18f;

        [Tooltip("Auto-tilt curve: X axis is normalized zoom (0 = min zoom/close, 1 = max zoom/far), Y axis is normalized tilt factor (0 = min pitch, 1 = max pitch / full player pitch).")]
        [SerializeField] private AnimationCurve _autoTiltCurve = AnimationCurve.EaseInOut(0f, 0.35f, 1f, 1f);

        [Header("World Bounds (Optional)")]
        [Tooltip("Restrict camera movement within world boundaries.")]
        [SerializeField] private bool _enableBounds = false;

        [SerializeField] private Vector2 _minBounds = new Vector2(-100f, -100f);
        [SerializeField] private Vector2 _maxBounds = new Vector2(100f, 100f);

        [Header("Focus / Target Following")]
        [Tooltip("Optional transform to follow (e.g., a selected pawn or building).")]
        [SerializeField] private Transform _followTarget;

        [Tooltip("Speed at which the camera catches up to a followed target.")]
        [SerializeField] private float _followCatchupSpeed = 8f;

        private Vector3 _targetPosition;
        private float _targetYaw;
        private float _targetPitch;
        private float _targetZoom;
        private float _currentZoom;
        private float _playerBasePitchNorm = 0.5f;
        private Camera _mainCameraCache;

        /// <summary>
        /// Normalized player pitch setting (0 = minPitch, 1 = maxPitch).
        /// </summary>
        public float PlayerPitchNormalized
        {
            get => _playerBasePitchNorm;
            set => _playerBasePitchNorm = Mathf.Clamp01(value);
        }

        private void Awake()
        {
            _targetPosition = transform.position;
            _targetYaw = _defaultYaw;
            _targetZoom = _defaultZoomDistance;
            _currentZoom = _defaultZoomDistance;

            _playerBasePitchNorm = Mathf.InverseLerp(_minPitch, _maxPitch, _defaultPitch);

            float initialZoomRatio = Mathf.InverseLerp(_minZoomDistance, _maxZoomDistance, _currentZoom);
            float curveFactor = _autoTiltOnZoom ? _autoTiltCurve.Evaluate(initialZoomRatio) : 1f;
            _targetPitch = Mathf.Lerp(_minPitch, _maxPitch, Mathf.Clamp01(_playerBasePitchNorm * curveFactor));

            _mainCameraCache = Camera.main;

            AutoWireComponents();
        }

        private void OnEnable()
        {
            EnableAction(_panAction);
            EnableAction(_sprintAction);
            EnableAction(_keyboardOrbitAction);
            EnableAction(_mouseOrbitDeltaAction);
            EnableAction(_mouseOrbitHoldAction);
            EnableAction(_zoomAction);
            EnableAction(_dragPanHoldAction);
            EnableAction(_dragPanDeltaAction);
        }

        private void OnDisable()
        {
            DisableAction(_panAction);
            DisableAction(_sprintAction);
            DisableAction(_keyboardOrbitAction);
            DisableAction(_mouseOrbitDeltaAction);
            DisableAction(_mouseOrbitHoldAction);
            DisableAction(_zoomAction);
            DisableAction(_dragPanHoldAction);
            DisableAction(_dragPanDeltaAction);
        }

        private static void EnableAction(InputActionReference actionRef)
        {
            if (actionRef != null && actionRef.action != null)
            {
                actionRef.action.Enable();
            }
        }

        private static void DisableAction(InputActionReference actionRef)
        {
            if (actionRef != null && actionRef.action != null)
            {
                actionRef.action.Disable();
            }
        }

        private void Start()
        {
            ApplyInitialCameraState();
        }

        /// <summary>
        /// Creates or configures the Cinemachine virtual camera with matching components and settings.
        /// </summary>
        [Button(overrideLabel: "Setup CM TopDownOrbitCamera", tooltip: "Creates or configures the CM TopDownOrbitCamera and CinemachineBrain with current colony camera settings.")]
        public void SetupVirtualCamera()
        {
            // Ensure Main Camera has CinemachineBrain
            var mainCameraObj = GameObject.FindWithTag("MainCamera");
            if (mainCameraObj == null)
            {
                var cam = Camera.main;
                if (cam != null) mainCameraObj = cam.gameObject;
            }

            if (mainCameraObj != null)
            {
                var brain = mainCameraObj.GetComponent<CinemachineBrain>();
                if (brain == null)
                {
                    brain = mainCameraObj.AddComponent<CinemachineBrain>();
                    Debug.Log("[ColonyCameraController] Added CinemachineBrain to Main Camera.");
                }
            }
            else
            {
                Debug.LogWarning("[ColonyCameraController] Main Camera not found in scene.");
            }

            // Find or create CM TopDownOrbitCamera
            GameObject vcamObj = null;
            if (_virtualCamera != null)
            {
                vcamObj = _virtualCamera.gameObject;
            }
            else
            {
                vcamObj = GameObject.Find("CM TopDownOrbitCamera");
                if (vcamObj == null)
                {
                    vcamObj = new GameObject("CM TopDownOrbitCamera");
                    Debug.Log("[ColonyCameraController] Created 'CM TopDownOrbitCamera' GameObject.");
                }
            }

            var cmCam = vcamObj.GetComponent<CinemachineCamera>();
            if (cmCam == null)
            {
                cmCam = vcamObj.AddComponent<CinemachineCamera>();
            }

            // Set targets to this target transform
            cmCam.Target.TrackingTarget = transform;
            cmCam.Target.LookAtTarget = transform;

            // Configure CinemachineOrbitalFollow
            var orbital = vcamObj.GetComponent<CinemachineOrbitalFollow>();
            if (orbital == null)
            {
                orbital = vcamObj.AddComponent<CinemachineOrbitalFollow>();
            }

            orbital.Radius = _defaultZoomDistance;
            orbital.HorizontalAxis.Value = _defaultYaw;
            orbital.HorizontalAxis.Center = _defaultYaw;
            orbital.HorizontalAxis.Wrap = true;
            orbital.VerticalAxis.Value = _defaultPitch;
            orbital.VerticalAxis.Center = _defaultPitch;
            orbital.VerticalAxis.Range = new Vector2(_minPitch, _maxPitch);
            orbital.TrackerSettings.PositionDamping = new Vector3(0.05f, 0.05f, 0.05f);

            // Configure CinemachineRotationComposer
            var rotComposer = vcamObj.GetComponent<CinemachineRotationComposer>();
            if (rotComposer == null)
            {
                rotComposer = vcamObj.AddComponent<CinemachineRotationComposer>();
            }
            rotComposer.TargetOffset = Vector3.zero;
            rotComposer.Composition.ScreenPosition = Vector2.zero;
            rotComposer.Damping = new Vector2(0.1f, 0.1f);

            // Wire references back to this controller
            _virtualCamera = cmCam;
            _orbitalFollow = orbital;

#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            EditorUtility.SetDirty(vcamObj);
            if (mainCameraObj != null) EditorUtility.SetDirty(mainCameraObj);
#endif

            Debug.Log("[ColonyCameraController] Successfully setup and wired CM TopDownOrbitCamera!");
        }

        /// <summary>
        /// Automatically finds and assigns Cinemachine camera components if not already referenced.
        /// </summary>
        public void AutoWireComponents()
        {
            if (_virtualCamera == null)
            {
                _virtualCamera = FindAnyObjectByType<CinemachineCamera>();
            }

            if (_virtualCamera != null && _orbitalFollow == null)
            {
                _orbitalFollow = _virtualCamera.GetComponent<CinemachineOrbitalFollow>();
            }

            if (_virtualCamera != null)
            {
                _virtualCamera.Target.TrackingTarget = transform;
                _virtualCamera.Target.LookAtTarget = transform;
            }
        }

        /// <summary>
        /// Applies the initial configuration parameters to the orbital follow component.
        /// </summary>
        public void ApplyInitialCameraState()
        {
            if (_orbitalFollow == null) return;

            _orbitalFollow.Radius = _currentZoom;
            _orbitalFollow.HorizontalAxis.Value = _targetYaw;
            _orbitalFollow.HorizontalAxis.Wrap = true;

            _orbitalFollow.VerticalAxis.Value = _targetPitch;
            _orbitalFollow.VerticalAxis.Range = new Vector2(_minPitch, _maxPitch);
        }

        private void Update()
        {
            if (_mainCameraCache == null)
            {
                _mainCameraCache = Camera.main;
            }

            HandleZoomInput();
            HandleRotationInput();
            HandlePanInput();

            UpdateCameraTransform();
        }

        private void HandlePanInput()
        {
            Vector2 moveInput = Vector2.zero;

            // 1. Read from Pan InputAction if assigned
            if (_panAction != null && _panAction.action != null)
            {
                moveInput = _panAction.action.ReadValue<Vector2>();
            }

            // Fallback to keyboard WASD / Arrow keys if action is unassigned or zero
            if (moveInput.sqrMagnitude < 0.001f && Keyboard.current != null)
            {
                if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) moveInput.y += 1f;
                if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) moveInput.y -= 1f;
                if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) moveInput.x -= 1f;
                if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) moveInput.x += 1f;
            }

            // 2. Edge Panning
            if (_enableEdgePanning && Mouse.current != null)
            {
                Vector2 mousePos = Mouse.current.position.ReadValue();
                if (mousePos.x >= 0 && mousePos.x <= Screen.width && mousePos.y >= 0 && mousePos.y <= Screen.height)
                {
                    if (mousePos.x < _edgePanBorderThickness) moveInput.x -= 1f;
                    if (mousePos.x > Screen.width - _edgePanBorderThickness) moveInput.x += 1f;
                    if (mousePos.y < _edgePanBorderThickness) moveInput.y -= 1f;
                    if (mousePos.y > Screen.height - _edgePanBorderThickness) moveInput.y += 1f;
                }
            }

            // 3. Middle Mouse Drag Panning
            bool isMiddleDragging = false;
            if (_dragPanHoldAction != null && _dragPanHoldAction.action != null)
            {
                isMiddleDragging = _dragPanHoldAction.action.IsPressed();
            }
            else if (Mouse.current != null)
            {
                isMiddleDragging = Mouse.current.middleButton.isPressed;
            }

            if (_enableMiddleMousePan && isMiddleDragging)
            {
                Vector2 mouseDelta = Vector2.zero;
                if (_dragPanDeltaAction != null && _dragPanDeltaAction.action != null)
                {
                    mouseDelta = _dragPanDeltaAction.action.ReadValue<Vector2>();
                }
                else if (Mouse.current != null)
                {
                    mouseDelta = Mouse.current.delta.ReadValue();
                }

                if (mouseDelta.sqrMagnitude > 0.01f)
                {
                    Vector3 forwardDir = GetCameraFlatForward();
                    Vector3 rightDir = GetCameraFlatRight();
                    Vector3 dragMovement = (-rightDir * mouseDelta.x - forwardDir * mouseDelta.y) * _mousePanSensitivity * (_currentZoom / 15f);
                    _targetPosition += dragMovement;
                    _followTarget = null;
                }
            }

            // Apply movement
            if (moveInput.sqrMagnitude > 0.01f)
            {
                _followTarget = null;
                moveInput.Normalize();

                bool isSprint = false;
                if (_sprintAction != null && _sprintAction.action != null)
                {
                    isSprint = _sprintAction.action.IsPressed() || _sprintAction.action.ReadValue<float>() > 0.5f;
                }
                else if (Keyboard.current != null)
                {
                    isSprint = Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed;
                }

                float speed = _panSpeed * (isSprint ? _fastPanMultiplier : 1f);
                float zoomScale = Mathf.Lerp(0.6f, 1.8f, Mathf.InverseLerp(_minZoomDistance, _maxZoomDistance, _currentZoom));
                speed *= zoomScale;

                Vector3 forwardDir = GetCameraFlatForward();
                Vector3 rightDir = GetCameraFlatRight();

                Vector3 movement = (forwardDir * moveInput.y + rightDir * moveInput.x) * speed * Time.unscaledDeltaTime;
                _targetPosition += movement;
            }

            // If tracking a target pawn/building
            if (_followTarget != null)
            {
                _targetPosition = Vector3.Lerp(_targetPosition, _followTarget.position, _followCatchupSpeed * Time.unscaledDeltaTime);
            }

            // Apply world bounds constraint
            if (_enableBounds)
            {
                _targetPosition.x = Mathf.Clamp(_targetPosition.x, _minBounds.x, _maxBounds.x);
                _targetPosition.z = Mathf.Clamp(_targetPosition.z, _minBounds.y, _maxBounds.y);
            }

            // Smoothly move anchor position
            transform.position = Vector3.Lerp(transform.position, _targetPosition, 1f - Mathf.Exp(-_panDamping * Time.unscaledDeltaTime));
        }

        private void HandleRotationInput()
        {
            // 1. Keyboard Orbit (Q / E keys)
            float kbOrbit = 0f;
            if (_keyboardOrbitAction != null && _keyboardOrbitAction.action != null)
            {
                var val = _keyboardOrbitAction.action.ReadValue<Vector2>();
                kbOrbit = Mathf.Abs(val.x) > 0.01f ? val.x : _keyboardOrbitAction.action.ReadValue<float>();
            }

            if (Mathf.Abs(kbOrbit) > 0.01f)
            {
                _targetYaw += kbOrbit * _keyboardRotationSpeed * Time.unscaledDeltaTime;
            }
            else if (Keyboard.current != null)
            {
                if (Keyboard.current.qKey.isPressed)
                {
                    _targetYaw -= _keyboardRotationSpeed * Time.unscaledDeltaTime;
                }
                if (Keyboard.current.eKey.isPressed)
                {
                    _targetYaw += _keyboardRotationSpeed * Time.unscaledDeltaTime;
                }
            }

            // 2. Mouse Orbit (Right-click drag or Alt+Left-click)
            bool isOrbiting = false;
            if (_mouseOrbitHoldAction != null && _mouseOrbitHoldAction.action != null)
            {
                isOrbiting = _mouseOrbitHoldAction.action.IsPressed();
            }
            else if (Mouse.current != null)
            {
                isOrbiting = Mouse.current.rightButton.isPressed;
            }

            if (isOrbiting)
            {
                Vector2 delta = Vector2.zero;
                if (_mouseOrbitDeltaAction != null && _mouseOrbitDeltaAction.action != null)
                {
                    delta = _mouseOrbitDeltaAction.action.ReadValue<Vector2>();
                }
                else if (Mouse.current != null)
                {
                    delta = Mouse.current.delta.ReadValue();
                }

                _targetYaw += delta.x * _mouseOrbitSensitivity;

                if (_allowPitchAdjustment && Mathf.Abs(delta.y) > 0.01f)
                {
                    float pitchDelta = delta.y * _mouseOrbitSensitivity;
                    float pitchRange = Mathf.Max(1f, _maxPitch - _minPitch);
                    _playerBasePitchNorm = Mathf.Clamp01(_playerBasePitchNorm - (pitchDelta / pitchRange));

                    float zoomRatio = Mathf.InverseLerp(_minZoomDistance, _maxZoomDistance, _currentZoom);
                    float curveFactor = _autoTiltOnZoom ? _autoTiltCurve.Evaluate(zoomRatio) : 1f;
                    _targetPitch = Mathf.Lerp(_minPitch, _maxPitch, Mathf.Clamp01(_playerBasePitchNorm * curveFactor));
                }
            }

            // Normalize targetYaw to 0-360 range
            if (_targetYaw > 360f) _targetYaw -= 360f;
            if (_targetYaw < 0f) _targetYaw += 360f;
        }

        private void HandleZoomInput()
        {
            float scroll = 0f;
            if (_zoomAction != null && _zoomAction.action != null)
            {
                Vector2 scrollVec = _zoomAction.action.ReadValue<Vector2>();
                scroll = Mathf.Abs(scrollVec.y) > 0.01f ? scrollVec.y : scrollVec.x;
            }

            if (Mathf.Abs(scroll) < 0.01f && Mouse.current != null)
            {
                scroll = Mouse.current.scroll.ReadValue().y;
            }

            if (Mathf.Abs(scroll) > 0.01f)
            {
                float step = Mathf.Sign(scroll) * _zoomSensitivity;
                _targetZoom = Mathf.Clamp(_targetZoom - step, _minZoomDistance, _maxZoomDistance);
            }

            // Keyboard zoom (+/- or PageUp/PageDown)
            if (Keyboard.current != null)
            {
                if (Keyboard.current.pageUpKey.isPressed || Keyboard.current.equalsKey.isPressed || Keyboard.current.numpadPlusKey.isPressed)
                {
                    _targetZoom = Mathf.Clamp(_targetZoom - _zoomSensitivity * 2f * Time.unscaledDeltaTime, _minZoomDistance, _maxZoomDistance);
                }
                if (Keyboard.current.pageDownKey.isPressed || Keyboard.current.minusKey.isPressed || Keyboard.current.numpadMinusKey.isPressed)
                {
                    _targetZoom = Mathf.Clamp(_targetZoom + _zoomSensitivity * 2f * Time.unscaledDeltaTime, _minZoomDistance, _maxZoomDistance);
                }
            }

            // Smooth zoom interpolation
            _currentZoom = Mathf.Lerp(_currentZoom, _targetZoom, 1f - Mathf.Exp(-_zoomDamping * Time.unscaledDeltaTime));

            // Relational Auto-tilt pitch based on zoom distance (lower angle when close, steeper angle when far)
            bool isMouseOrbiting = (_mouseOrbitHoldAction != null && _mouseOrbitHoldAction.action != null)
                ? _mouseOrbitHoldAction.action.IsPressed()
                : (Mouse.current != null && Mouse.current.rightButton.isPressed);

            if (_autoTiltOnZoom && !isMouseOrbiting)
            {
                float zoomRatio = Mathf.InverseLerp(_minZoomDistance, _maxZoomDistance, _currentZoom);
                float curveFactor = _autoTiltCurve.Evaluate(zoomRatio);
                float desiredPitch = Mathf.Lerp(_minPitch, _maxPitch, Mathf.Clamp01(_playerBasePitchNorm * curveFactor));
                _targetPitch = Mathf.Lerp(_targetPitch, desiredPitch, 1f - Mathf.Exp(-_autoTiltSpeed * Time.unscaledDeltaTime));
            }
        }

        private void UpdateCameraTransform()
        {
            if (_orbitalFollow == null) return;

            // Smoothly update orbital axes and radius
            _orbitalFollow.Radius = _currentZoom;

            float smoothYaw = Mathf.LerpAngle(_orbitalFollow.HorizontalAxis.Value, _targetYaw, 1f - Mathf.Exp(-_rotationDamping * Time.unscaledDeltaTime));
            _orbitalFollow.HorizontalAxis.Value = smoothYaw;

            float smoothPitch = Mathf.Lerp(_orbitalFollow.VerticalAxis.Value, _targetPitch, 1f - Mathf.Exp(-_rotationDamping * Time.unscaledDeltaTime));
            _orbitalFollow.VerticalAxis.Value = smoothPitch;
        }

        /// <summary>
        /// Sets a specific transform to follow (e.g. following a selected colonist).
        /// </summary>
        public void FocusOn(Transform target)
        {
            _followTarget = target;
        }

        /// <summary>
        /// Instantly snaps the camera target to a specified world position.
        /// </summary>
        public void TeleportTo(Vector3 worldPosition)
        {
            _targetPosition = worldPosition;
            transform.position = worldPosition;
        }

        /// <summary>
        /// Returns the camera's forward vector projected on the horizontal XZ ground plane.
        /// </summary>
        public Vector3 GetCameraFlatForward()
        {
            if (_mainCameraCache != null)
            {
                Vector3 forward = _mainCameraCache.transform.forward;
                forward.y = 0f;
                return forward.sqrMagnitude > 0.001f ? forward.normalized : Vector3.forward;
            }
            return Vector3.forward;
        }

        /// <summary>
        /// Returns the camera's right vector projected on the horizontal XZ ground plane.
        /// </summary>
        public Vector3 GetCameraFlatRight()
        {
            if (_mainCameraCache != null)
            {
                Vector3 right = _mainCameraCache.transform.right;
                right.y = 0f;
                return right.sqrMagnitude > 0.001f ? right.normalized : Vector3.right;
            }
            return Vector3.right;
        }
    }
}
