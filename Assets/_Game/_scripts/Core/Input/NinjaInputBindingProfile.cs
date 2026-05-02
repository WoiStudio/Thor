using UnityEngine;

namespace Woi.Ninja.Core.Input
{
    /// <summary>
    /// PC + gamepad binding paths for <see cref="NinjaGameplayInputModule"/>.
    /// Create an asset (Create → Woi → Ninja → Input Binding Profile) or leave the module field empty to use built-in defaults.
    /// </summary>
    [CreateAssetMenu(fileName = "NinjaInputBindingProfile", menuName = "Woi/Ninja/Input Binding Profile")]
    public sealed class NinjaInputBindingProfile : ScriptableObject
    {
        [Header("Move — Keyboard (fixed WASD + arrows in code)")]
        [Tooltip("Gamepad analog stick for movement.")]
        [SerializeField] private string _gamepadMoveStick = "<Gamepad>/leftStick";

        [Tooltip("Adds D-pad as a second 2D source (recommended for menus / grid feel).")]
        [SerializeField] private bool _includeGamepadDpad = true;

        [Header("Dash")]
        [SerializeField] private string _dashKeyboard = "<Keyboard>/space";

        [SerializeField] private string _dashGamepad = "<Gamepad>/buttonSouth";

        [Header("Interact")]
        [SerializeField] private string _interactKeyboard = "<Keyboard>/e";

        [SerializeField] private string _interactGamepad = "<Gamepad>/buttonWest";

        public string GamepadMoveStick => _gamepadMoveStick;

        public bool IncludeGamepadDpad => _includeGamepadDpad;

        public string DashKeyboard => _dashKeyboard;

        public string DashGamepad => _dashGamepad;

        public string InteractKeyboard => _interactKeyboard;

        public string InteractGamepad => _interactGamepad;
    }
}
