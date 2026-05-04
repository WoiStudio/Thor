using UnityEngine;

namespace Woi.Ninja.Player.Services
{
    /// <summary>
    /// Top-down mouse aim on the XZ plane at the player's height.
    /// Read <see cref="AimDirection"/> / <see cref="HasAimDirection"/> use a physics-step snapshot (see <see cref="PlayerAimProvider"/>).
    /// </summary>
    public interface IPlayerAimProvider
    {
        bool HasAimDirection { get; }

        Vector3 AimDirection { get; }

        Vector3 AimWorldPoint { get; }

        /// <summary>
        /// Recomputes the fixed-step aim snapshot immediately (e.g. attack <c>Enter</c> before physics runs).
        /// </summary>
        void SampleAimNow();
    }
}
