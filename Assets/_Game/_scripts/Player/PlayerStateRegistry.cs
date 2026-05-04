using UnityEngine;

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

        public PlayerAttackState Attack { get; set; }

        public PlayerThrowState Throw { get; set; }

        /// <summary>Used for dash direction fallback when move input is below epsilon.</summary>
        public Transform PlayerTransform { get; set; }
    }
}
