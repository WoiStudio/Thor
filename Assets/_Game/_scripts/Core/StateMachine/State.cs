namespace Woi.Ninja.Core.StateMachine
{
    /// <summary>
    /// Optional base class for states; override only what you need.
    /// </summary>
    public abstract class State : IState
    {
        public virtual void Enter() { }

        public virtual void Exit() { }

        public virtual void Tick() { }

        public virtual void FixedTick() { }
    }
}
