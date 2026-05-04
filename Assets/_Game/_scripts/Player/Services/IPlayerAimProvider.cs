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
    }
}
