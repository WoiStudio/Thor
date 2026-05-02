using System;
using System.Collections.Generic;

namespace Woi.Ninja.Core.StateMachine
{
    /// <summary>
    /// Lightweight finite state machine. Not a <c>MonoBehaviour</c>; drive it from your own update loop.
    /// Condition-based transitions are evaluated in <see cref="Tick"/> after the current state's <see cref="IState.Tick"/>.
    /// "Any" transitions are evaluated first, in registration order; then outgoing transitions from the current state.
    /// </summary>
    public sealed class StateMachine
    {
        private readonly List<ITransition> _anyTransitions = new List<ITransition>();
        private readonly Dictionary<IState, List<ITransition>> _transitions = new Dictionary<IState, List<ITransition>>();

        public IState CurrentState { get; private set; }

        public void SetState(IState state)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));

            if (ReferenceEquals(CurrentState, state))
                return;

            CurrentState?.Exit();
            CurrentState = state;
            CurrentState.Enter();
        }

        public void Tick()
        {
            CurrentState?.Tick();
            EvaluateTransitions();
        }

        public void FixedTick()
        {
            CurrentState?.FixedTick();
        }

        public void AddTransition(IState from, IState to, Func<bool> condition)
        {
            if (from == null)
                throw new ArgumentNullException(nameof(from));

            var transition = new FuncTransition(from, to, condition);
            if (!_transitions.TryGetValue(from, out var list))
            {
                list = new List<ITransition>();
                _transitions[from] = list;
            }

            list.Add(transition);
        }

        public void AddAnyTransition(IState to, Func<bool> condition)
        {
            _anyTransitions.Add(new FuncTransition(from: null, to, condition));
        }

        private void EvaluateTransitions()
        {
            if (CurrentState == null)
                return;

            foreach (var transition in _anyTransitions)
            {
                if (transition.ShouldTransition())
                {
                    SetState(transition.To);
                    return;
                }
            }

            if (!_transitions.TryGetValue(CurrentState, out var outgoing))
                return;

            foreach (var transition in outgoing)
            {
                if (transition.ShouldTransition())
                {
                    SetState(transition.To);
                    return;
                }
            }
        }
    }
}
