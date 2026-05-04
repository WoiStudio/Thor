using UnityEngine;

namespace Woi.Ninja.Player.Services
{
    /// <summary>
    /// Shuriken throw timing; driven by <see cref="PlayerThrowState"/> via <see cref="TickThrow"/>.
    /// </summary>
    public interface IPlayerThrower
    {
        bool CanThrow { get; }

        bool IsThrowing { get; }

        bool HasThrowFinished { get; }

        /// <returns>False if throw did not start (cannot throw right now).</returns>
        bool BeginThrow(Vector3 direction);

        void TickThrow();

        void ClearThrowFinishedFlag();
    }
}
