namespace Woi.Ninja.Core.StateMachine
{
    /// <summary>
    /// A directed transition between states. For "any" transitions, <see cref="From"/> is <c>null</c>.
    /// </summary>
    public interface ITransition
    {
        /// <summary>Source state, or <c>null</c> when registered via <see cref="StateMachine.AddAnyTransition"/>.</summary>
        IState From { get; }

        IState To { get; }

        bool ShouldTransition();
    }
}
