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
    }
}
