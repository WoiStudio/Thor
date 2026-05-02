using Woi.Ninja.Core.StateMachine;
using Woi.Ninja.Player.Services;

namespace Woi.Ninja.Player
{
    public sealed class PlayerIdleState : PlayerState
    {
        public PlayerIdleState(
            IPlayerInputReader input,
            IPlayerMotor motor,
            IPlayerDash dash,
            IPlayerInteraction interaction,
            StateMachine machine,
            PlayerStateRegistry registry)
            : base(input, motor, dash, interaction, machine, registry)
        {
        }

        public override void Tick()
        {
            Motor.Stop();

            if (Input.HasMoveInput)
            {
                Machine.SetState(Registry.Move);
                return;
            }

            if (Input.DashPressed && Dash.CanDash)
            {
                Machine.SetState(Registry.Dash);
                return;
            }

            if (Input.InteractPressed)
            {
                Machine.SetState(Registry.Interact);
            }
        }
    }
}
