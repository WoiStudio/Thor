using UnityEngine;

namespace Woi.Ninja.Core.Input
{
    /// <summary>
    /// Frame snapshot for top-down gameplay (move, dash, interact, attack, throw).
    /// Implemented by <see cref="NinjaGameplayInputModule"/> and consumed by <see cref="Player.Services.PlayerInputReader"/>.
    /// </summary>
    public interface INinjaGameplayInput
    {
        Vector2 MoveInput { get; }

        bool HasMoveInput { get; }

        bool DashPressed { get; }

        bool InteractPressed { get; }

        bool AttackPressed { get; }

        bool ThrowPressed { get; }

        /// <summary>
        /// Consumes a recent attack press that may have occurred on an earlier frame (e.g. same frame as combo end).
        /// </summary>
        bool TryConsumeAttackInputBuffer(float maxAgeSeconds);

        /// <summary>
        /// Drops the stored attack-press time so it cannot be consumed as a combo-chain input.
        /// </summary>
        void ClearAttackInputBuffer();
    }
}
