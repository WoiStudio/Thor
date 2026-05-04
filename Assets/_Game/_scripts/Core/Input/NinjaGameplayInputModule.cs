using UnityEngine;
using UnityEngine.InputSystem;

namespace Woi.Ninja.Core.Input
{
    /// <summary>
    /// Samples gameplay actions via the <b>new Input System</b>.
    /// </summary>
    /// <remarks>
    /// <para>When an <see cref="InputActionAsset"/> is assigned, the <b>Gameplay</b> map should contain
    /// <b>Move</b>, <b>Dash</b>, <b>Interact</b>, <b>Attack</b>, and <b>Throw</b>.</para>
    /// <para>If no asset is assigned, actions are built in code.</para>
    /// Requires package <c>com.unity.inputsystem</c>.
    /// </remarks>
    [DefaultExecutionOrder(-100)]
    public sealed class NinjaGameplayInputModule : MonoBehaviour, INinjaGameplayInput
    {
        [Header("Input Actions asset (recommended)")]
        [Tooltip("When set, actions come from this asset. Code fallback ignored.")]
        [SerializeField] private InputActionAsset _inputActionAsset;

        [SerializeField] private string _actionMapName = "Gameplay";

        [SerializeField] private string _moveActionName = "Move";

        [SerializeField] private string _dashActionName = "Dash";

        [SerializeField] private string _interactActionName = "Interact";

        [SerializeField] private string _attackActionName = "Attack";

        [SerializeField] private string _throwActionName = "Throw";

        [Header("Code fallback (no asset)")]
        [Tooltip("Only used when Input Action Asset is null.")]
        [SerializeField] private NinjaInputBindingProfile _bindingProfile;

        [SerializeField] [Min(0f)] private float _moveDeadZone = 0.01f;

        private InputAction _moveAction;
        private InputAction _dashAction;
        private InputAction _interactAction;
        private InputAction _attackAction;

        private InputAction _throwAction;

        private bool _disposeActionsOnDestroy;

        private Vector2 _moveInput;
        private bool _dashPressed;
        private bool _interactPressed;
        private bool _attackPressed;
        private bool _throwPressed;

        private InputDevice _lastInputDevice;

        public Vector2 MoveInput => _moveInput;

        public bool HasMoveInput => _moveInput.sqrMagnitude > _moveDeadZone * _moveDeadZone;

        public bool DashPressed => _dashPressed;

        public bool InteractPressed => _interactPressed;

        public bool AttackPressed => _attackPressed;

        public bool ThrowPressed => _throwPressed;

        /// <summary>Device that most recently produced meaningful input this frame (for UI prompts).</summary>
        public InputDevice LastInputDeviceUsed => _lastInputDevice;

        public bool LastInputWasFromGamepad => _lastInputDevice is Gamepad;

        private void Awake()
        {
            if (_inputActionAsset != null)
            {
                TryBindFromAsset();
            }
            else
            {
                var resolved = _bindingProfile != null
                    ? NinjaResolvedBindings.FromProfile(_bindingProfile)
                    : NinjaResolvedBindings.BuiltIn;

                BuildActionsInCode(resolved);
                _disposeActionsOnDestroy = true;
            }
        }

        private void TryBindFromAsset()
        {
            var map = _inputActionAsset.FindActionMap(_actionMapName, throwIfNotFound: false);
            if (map == null)
            {
                Debug.LogError(
                    "[NinjaGameplayInputModule] Action map '" + _actionMapName + "' not found on asset '" +
                    _inputActionAsset.name + "'. Falling back to code bindings.",
                    this);
                BuildActionsInCode(NinjaResolvedBindings.BuiltIn);
                _disposeActionsOnDestroy = true;
                return;
            }

            _moveAction = map.FindAction(_moveActionName, throwIfNotFound: false);
            _dashAction = map.FindAction(_dashActionName, throwIfNotFound: false);
            _interactAction = map.FindAction(_interactActionName, throwIfNotFound: false);
            _attackAction = map.FindAction(_attackActionName, throwIfNotFound: false);
            _throwAction = map.FindAction(_throwActionName, throwIfNotFound: false);

            if (_moveAction == null || _dashAction == null || _interactAction == null || _attackAction == null
                || _throwAction == null)
            {
                Debug.LogError(
                    "[NinjaGameplayInputModule] Expected actions '" + _moveActionName + "', '" + _dashActionName +
                    "', '" + _interactActionName + "', '" + _attackActionName + "', '" + _throwActionName +
                    "' in map '" + _actionMapName + "'. Missing: " + DescribeMissing() +
                    ". Falling back to code bindings.",
                    this);
                _moveAction = null;
                _dashAction = null;
                _interactAction = null;
                _attackAction = null;
                _throwAction = null;
                BuildActionsInCode(NinjaResolvedBindings.BuiltIn);
                _disposeActionsOnDestroy = true;
                return;
            }

            _disposeActionsOnDestroy = false;
        }

        private string DescribeMissing()
        {
            var s = "";
            if (_moveAction == null) s += _moveActionName + " ";
            if (_dashAction == null) s += _dashActionName + " ";
            if (_interactAction == null) s += _interactActionName + " ";
            if (_attackAction == null) s += _attackActionName + " ";
            if (_throwAction == null) s += _throwActionName + " ";
            return string.IsNullOrEmpty(s) ? "(none)" : s.TrimEnd();
        }

        private void OnEnable()
        {
            _moveAction?.Enable();
            _dashAction?.Enable();
            _interactAction?.Enable();
            _attackAction?.Enable();
            _throwAction?.Enable();
        }

        private void OnDisable()
        {
            _moveAction?.Disable();
            _dashAction?.Disable();
            _interactAction?.Disable();
            _attackAction?.Disable();
            _throwAction?.Disable();
        }

        private void OnDestroy()
        {
            if (_disposeActionsOnDestroy)
                DisposeCodeOwnedActions();
        }

        private void Update()
        {
            Sample();
        }

        /// <summary>
        /// Refreshes cached values. Called from <see cref="Update"/>; exposed for tests or custom drivers.
        /// </summary>
        public void Sample()
        {
            if (_moveAction != null)
                _moveInput = _moveAction.ReadValue<Vector2>();

            _dashPressed = _dashAction != null && _dashAction.WasPressedThisFrame();
            _interactPressed = _interactAction != null && _interactAction.WasPressedThisFrame();
            _attackPressed = _attackAction != null && _attackAction.WasPressedThisFrame();
            _throwPressed = _throwAction != null && _throwAction.WasPressedThisFrame();

            UpdateLastDevice();
        }

        private void UpdateLastDevice()
        {
            if (_throwPressed && _throwAction?.activeControl != null)
            {
                _lastInputDevice = _throwAction.activeControl.device;
                return;
            }

            if (_attackPressed && _attackAction?.activeControl != null)
            {
                _lastInputDevice = _attackAction.activeControl.device;
                return;
            }

            if (_dashPressed && _dashAction?.activeControl != null)
            {
                _lastInputDevice = _dashAction.activeControl.device;
                return;
            }

            if (_interactPressed && _interactAction?.activeControl != null)
            {
                _lastInputDevice = _interactAction.activeControl.device;
                return;
            }

            if (HasMoveInput && _moveAction?.activeControl != null)
            {
                _lastInputDevice = _moveAction.activeControl.device;
                return;
            }
        }

        private void BuildActionsInCode(NinjaResolvedBindings b)
        {
            _moveAction = new InputAction("Move", InputActionType.Value, expectedControlType: "Vector2");

            _moveAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");

            _moveAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/upArrow")
                .With("Down", "<Keyboard>/downArrow")
                .With("Left", "<Keyboard>/leftArrow")
                .With("Right", "<Keyboard>/rightArrow");

            if (!string.IsNullOrEmpty(b.GamepadMoveStick))
                _moveAction.AddBinding(b.GamepadMoveStick);

            if (b.IncludeGamepadDpad)
            {
                _moveAction.AddCompositeBinding("2DVector")
                    .With("Up", "<Gamepad>/dpad/up")
                    .With("Down", "<Gamepad>/dpad/down")
                    .With("Left", "<Gamepad>/dpad/left")
                    .With("Right", "<Gamepad>/dpad/right");
            }

            _dashAction = new InputAction("Dash", InputActionType.Button);
            AddIfPresent(_dashAction, b.DashKeyboard);
            AddIfPresent(_dashAction, b.DashGamepad);

            _interactAction = new InputAction("Interact", InputActionType.Button);
            AddIfPresent(_interactAction, b.InteractKeyboard);
            AddIfPresent(_interactAction, b.InteractGamepad);

            _attackAction = new InputAction("Attack", InputActionType.Button);
            _attackAction.AddBinding("<Mouse>/leftButton");
            _attackAction.AddBinding("<Keyboard>/j");
            _attackAction.AddBinding("<Gamepad>/buttonNorth");

            _throwAction = new InputAction("Throw", InputActionType.Button);
            _throwAction.AddBinding("<Mouse>/rightButton");
            _throwAction.AddBinding("<Keyboard>/k");
            _throwAction.AddBinding("<Gamepad>/buttonEast");
        }

        private static void AddIfPresent(InputAction action, string path)
        {
            if (!string.IsNullOrWhiteSpace(path))
                action.AddBinding(path.Trim());
        }

        private void DisposeCodeOwnedActions()
        {
            _moveAction?.Dispose();
            _moveAction = null;
            _dashAction?.Dispose();
            _dashAction = null;
            _interactAction?.Dispose();
            _interactAction = null;
            _attackAction?.Dispose();
            _attackAction = null;
            _throwAction?.Dispose();
            _throwAction = null;
        }
    }
}
