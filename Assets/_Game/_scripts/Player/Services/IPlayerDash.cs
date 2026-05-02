namespace Woi.Ninja.Player.Services
{
    /// <summary>
    /// Dash execution and lifecycle. No real dash logic yet.
    /// </summary>
    public interface IPlayerDash
    {
        void NotifyDashStateEntered();

        void NotifyDashStateExited();

        bool IsDashFinished { get; }
    }
}
