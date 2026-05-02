namespace Woi.Ninja.Core.StateMachine
{
    /// <summary>
    /// Contract for a state executed by <see cref="StateMachine"/>.
    /// </summary>
    public interface IState
    {
        void Enter();
        void Exit();
        void Tick();
        void FixedTick();
    }
}
