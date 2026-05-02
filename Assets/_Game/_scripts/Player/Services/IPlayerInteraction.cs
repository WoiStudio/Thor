namespace Woi.Ninja.Player.Services
{
    /// <summary>
    /// Puzzle interactions (switches, doors, carry, etc.). No real interaction logic yet.
    /// </summary>
    public interface IPlayerInteraction
    {
        void NotifyInteractStateEntered();

        void NotifyInteractStateExited();

        bool IsInteractionComplete { get; }
    }
}
