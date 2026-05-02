using UnityEngine;

namespace Woi.Ninja.Core.Input
{
    /// <summary>
    /// Frame snapshot for top-down gameplay (move, dash, interact, attack).
    /// Implemented by <see cref="NinjaGameplayInputModule"/> and consumed by <see cref="Player.Services.PlayerInputReader"/>.
    /// </summary>
    public interface INinjaGameplayInput
    {
        Vector2 MoveInput { get; }

        bool HasMoveInput { get; }

        bool DashPressed { get; }

        bool InteractPressed { get; }

        bool AttackPressed { get; }
    }
}
