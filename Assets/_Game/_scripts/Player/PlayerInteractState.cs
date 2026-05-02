using Woi.Ninja.Core.StateMachine;
using Woi.Ninja.Player.Services;

namespace Woi.Ninja.Player
{
    public sealed class PlayerInteractState : PlayerState
    {
        public PlayerInteractState(
            IPlayerInputReader input,
            IPlayerMotor motor,
            IPlayerDash dash,
            IPlayerInteraction interaction,
            StateMachine machine,
            PlayerStateRegistry registry)
            : base(input, motor, dash, interaction, machine, registry)
        {
        }

        public override void Enter()
        {
            Interaction.NotifyInteractStateEntered();
        }

        public override void Exit()
        {
            Interaction.NotifyInteractStateExited();
        }

        public override void Tick()
        {
            Motor.Stop();

            if (Interaction.IsInteractionComplete)
            {
                Machine.SetState(Registry.Idle);
            }
        }
    }
}
