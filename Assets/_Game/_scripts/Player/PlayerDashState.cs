using Woi.Ninja.Core.StateMachine;
using Woi.Ninja.Player.Services;

namespace Woi.Ninja.Player
{
    public sealed class PlayerDashState : PlayerState
    {
        public PlayerDashState(
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
            Dash.NotifyDashStateEntered();
        }

        public override void Exit()
        {
            Dash.NotifyDashStateExited();
        }

        public override void Tick()
        {
            Motor.Stop();

            if (Dash.IsDashFinished)
            {
                Machine.SetState(Registry.Idle);
            }
        }
    }
}
