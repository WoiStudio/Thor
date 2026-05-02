using UnityEngine;

namespace Woi.Ninja.Player.Services
{
    /// <summary>
    /// Placeholder interaction handler. Defaults to complete so the interact state can unwind in tests.
    /// </summary>
    public sealed class PlayerInteraction : MonoBehaviour, IPlayerInteraction
    {
        public void NotifyInteractStateEntered() { }

        public void NotifyInteractStateExited() { }

        public bool IsInteractionComplete => true;
    }
}
