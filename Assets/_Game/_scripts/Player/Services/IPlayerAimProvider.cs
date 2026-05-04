using UnityEngine;

namespace Woi.Ninja.Player.Services
{
    /// <summary>
    /// Top-down mouse aim on the XZ plane at the player's height.
    /// </summary>
    public interface IPlayerAimProvider
    {
        bool HasAimDirection { get; }

        Vector3 AimDirection { get; }

        Vector3 AimWorldPoint { get; }

        /// <summary>
        /// Recomputes aim immediately (e.g. before attack <c>Enter</c> so the first frame has valid data).
        /// </summary>
        void SampleAimNow();
    }
}
