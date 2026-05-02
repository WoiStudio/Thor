using UnityEngine;

namespace Woi.Ninja.Player.Services
{
    /// <summary>
    /// Placeholder dash service. Defaults to "finished" so the dash state can exit immediately in tests.
    /// </summary>
    public sealed class PlayerDash : MonoBehaviour, IPlayerDash
    {
        public void NotifyDashStateEntered() { }

        public void NotifyDashStateExited() { }

        public bool IsDashFinished => true;
    }
}
