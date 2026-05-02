namespace Woi.Ninja.Core.Input
{
    /// <summary>
    /// Resolved PC + gamepad paths used when building <see cref="UnityEngine.InputSystem.InputAction"/>s.
    /// </summary>
    internal readonly struct NinjaResolvedBindings
    {
        public NinjaResolvedBindings(
            string gamepadMoveStick,
            bool includeGamepadDpad,
            string dashKeyboard,
            string dashGamepad,
            string interactKeyboard,
            string interactGamepad)
        {
            GamepadMoveStick = gamepadMoveStick;
            IncludeGamepadDpad = includeGamepadDpad;
            DashKeyboard = dashKeyboard;
            DashGamepad = dashGamepad;
            InteractKeyboard = interactKeyboard;
            InteractGamepad = interactGamepad;
        }

        public string GamepadMoveStick { get; }

        public bool IncludeGamepadDpad { get; }

        public string DashKeyboard { get; }

        public string DashGamepad { get; }

        public string InteractKeyboard { get; }

        public string InteractGamepad { get; }

        public static NinjaResolvedBindings BuiltIn => new NinjaResolvedBindings(
            gamepadMoveStick: "<Gamepad>/leftStick",
            includeGamepadDpad: true,
            dashKeyboard: "<Keyboard>/space",
            dashGamepad: "<Gamepad>/buttonSouth",
            interactKeyboard: "<Keyboard>/e",
            interactGamepad: "<Gamepad>/buttonWest");

        public static NinjaResolvedBindings FromProfile(NinjaInputBindingProfile profile)
        {
            return new NinjaResolvedBindings(
                string.IsNullOrWhiteSpace(profile.GamepadMoveStick)
                    ? BuiltIn.GamepadMoveStick
                    : profile.GamepadMoveStick.Trim(),
                profile.IncludeGamepadDpad,
                Coalesce(profile.DashKeyboard, BuiltIn.DashKeyboard),
                Coalesce(profile.DashGamepad, BuiltIn.DashGamepad),
                Coalesce(profile.InteractKeyboard, BuiltIn.InteractKeyboard),
                Coalesce(profile.InteractGamepad, BuiltIn.InteractGamepad));
        }

        private static string Coalesce(string value, string fallback) =>
            string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
    }
}
