using Woi.Ninja.Core.StateMachine;
using Woi.Ninja.Player.Services;

namespace Woi.Ninja.Player
{
    /// <summary>
    /// Shared dependencies for all player states (constructor-injected).
    /// </summary>
    public abstract class PlayerState : State
    {
        protected PlayerState(
            IPlayerInputReader input,
            IPlayerMotor motor,
            IPlayerDash dash,
            IPlayerInteraction interaction,
            StateMachine machine,
            PlayerStateRegistry registry)
        {
            Input = input;
            Motor = motor;
            Dash = dash;
            Interaction = interaction;
            Machine = machine;
            Registry = registry;
        }

        protected IPlayerInputReader Input { get; }

        protected IPlayerMotor Motor { get; }

        protected IPlayerDash Dash { get; }

        protected IPlayerInteraction Interaction { get; }

        protected StateMachine Machine { get; }

        protected PlayerStateRegistry Registry { get; }
    }
}
