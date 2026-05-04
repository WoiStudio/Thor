using UnityEngine;

namespace Woi.Ninja.Player.Services
{
    /// <summary>
    /// Abstracts player intent. Default implementation is <see cref="PlayerInputReader"/> over <see cref="Woi.Ninja.Core.Input.NinjaGameplayInputModule"/>.
    /// </summary>
    public interface IPlayerInputReader
    {
        Vector2 MoveInput { get; }

        bool HasMoveInput { get; }

        bool DashPressed { get; }

        bool InteractPressed { get; }

        bool AttackPressed { get; }

        bool ThrowPressed { get; }

        /// <summary>
        /// True once if attack was pressed within the buffer window and not already consumed this way.
        /// Use with <see cref="AttackPressed"/> for reliable chain / post-cooldown starts.
        /// </summary>
        bool TryConsumeAttackInputBuffer();

        /// <summary>
        /// Clears the attack press timestamp so a press that started this swing cannot queue the next hit on the following frame.
        /// </summary>
        void ClearAttackInputBuffer();
    }
}
