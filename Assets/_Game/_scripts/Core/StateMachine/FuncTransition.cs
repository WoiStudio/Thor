using System;

namespace Woi.Ninja.Core.StateMachine
{
    /// <summary>
    /// Transition gated by a <see cref="Func{TResult}"/> predicate.
    /// </summary>
    public sealed class FuncTransition : ITransition
    {
        private readonly Func<bool> _condition;

        public FuncTransition(IState from, IState to, Func<bool> condition)
        {
            To = to ?? throw new ArgumentNullException(nameof(to));
            _condition = condition ?? throw new ArgumentNullException(nameof(condition));
            From = from;
        }

        public IState From { get; }

        public IState To { get; }

        public bool ShouldTransition() => _condition();
    }
}
