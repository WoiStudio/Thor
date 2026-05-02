using UnityEngine;

namespace Woi.Ninja.Player.Services
{
    /// <summary>
    /// Physics-driven dash on the XZ plane. Driven from <see cref="ApplyFixedDash"/> (no Update/FixedUpdate here).
    /// </summary>
    public interface IPlayerDash
    {
        bool CanDash { get; }

        bool IsDashing { get; }

        /// <summary>
        /// True after the dash segment completes until <see cref="ClearDashFinishedFlag"/>.
        /// </summary>
        bool HasDashFinished { get; }

        void BeginDash(Vector2 inputDirection, Vector3 fallbackForward);

        void ApplyFixedDash();

        void ClearDashFinishedFlag();
    }
}
