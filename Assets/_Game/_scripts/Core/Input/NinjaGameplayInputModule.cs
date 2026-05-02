using UnityEngine;
using UnityEngine.InputSystem;

namespace Woi.Ninja.Core.Input
{
    /// <summary>
    /// Samples gameplay actions via the <b>new Input System</b>.
    /// </summary>
    /// <remarks>
    /// <para><b>Recommended:</b> assign an <see cref="InputActionAsset"/> (e.g. <c>PlayerInputActions</c>) and define
    /// the <b>Gameplay</b> map plus <b>Move</b>, <b>Dash</b>, <b>Interact</b> in the Input Actions window — PC + gamepad bindings and control schemes stay in one place.</para>
    /// <para>If no asset is assigned, actions are built in code; optional <see cref="NinjaInputBindingProfile"/> then supplies paths.</para>
    /// Requires package <c>com.unity.inputsystem</c>. Set <b>Active Input Handling</b> to Input System or Both.
    /// </remarks>
    [DefaultExecutionOrder(-100)]
    public sealed class NinjaGameplayInputModule : MonoBehaviour, INinjaGameplayInput
    {
        [Header("Input Actions asset (recommended)")]
        [Tooltip("When set, Move / Dash / Interact come from this asset (visual editor). Code-built bindings and Binding Profile are ignored.")]
        [SerializeField] private InputActionAsset _inputActionAsset;

        [SerializeField] private string _actionMapName = "Gameplay";

        [SerializeField] private string _moveActionName = "Move";

        [SerializeField] private string _dashActionName = "Dash";

        [SerializeField] private string _interactActionName = "Interact";

        [Header("Code fallback (no asset)")]
        [Tooltip("Only used when Input Action Asset is null. Create: Assets → Create → Woi → Ninja → Input Binding Profile.")]
        [SerializeField] private NinjaInputBindingProfile _bindingProfile;

        [SerializeField] [Min(0f)] private float _moveDeadZone = 0.01f;

        private InputAction _moveAction;
        private InputAction _dashAction;
        private InputAction _interactAction;

        private bool _disposeActionsOnDestroy;

        private Vector2 _moveInput;
        private bool _dashPressed;
        private bool _interactPressed;

        private InputDevice _lastInputDevice;

        public Vector2 MoveInput => _moveInput;

        public bool HasMoveInput => _moveInput.sqrMagnitude > _moveDeadZone * _moveDeadZone;

        public bool DashPressed => _dashPressed;

        public bool InteractPressed => _interactPressed;

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

            if (_moveAction == null || _dashAction == null || _interactAction == null)
            {
                Debug.LogError(
                    "[NinjaGameplayInputModule] Expected actions '" + _moveActionName + "', '" + _dashActionName +
                    "', '" + _interactActionName + "' in map '" + _actionMapName + "'. Missing: " +
                    DescribeMissing() + ". Falling back to code bindings.",
                    this);
                _moveAction = null;
                _dashAction = null;
                _interactAction = null;
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
            return string.IsNullOrEmpty(s) ? "(none)" : s.TrimEnd();
        }

        private void OnEnable()
        {
            _moveAction?.Enable();
            _dashAction?.Enable();
            _interactAction?.Enable();
        }

        private void OnDisable()
        {
            _moveAction?.Disable();
            _dashAction?.Disable();
            _interactAction?.Disable();
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

            UpdateLastDevice();
        }

        private void UpdateLastDevice()
        {
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
        }
    }
}
