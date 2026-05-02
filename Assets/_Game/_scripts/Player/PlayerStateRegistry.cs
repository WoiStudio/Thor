namespace Woi.Ninja.Player
{
    /// <summary>
    /// Holds references to all player states for explicit transitions without service location.
    /// Populated by <see cref="PlayerController"/> after construction.
    /// </summary>
    public sealed class PlayerStateRegistry
    {
        public PlayerIdleState Idle { get; set; }

        public PlayerMoveState Move { get; set; }

        public PlayerDashState Dash { get; set; }

        public PlayerInteractState Interact { get; set; }
    }
}
