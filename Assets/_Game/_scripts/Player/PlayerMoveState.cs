using Woi.Ninja.Core.StateMachine;
using Woi.Ninja.Player.Services;

namespace Woi.Ninja.Player
{
    public sealed class PlayerMoveState : PlayerState
    {
        public PlayerMoveState(
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
            if (Input.DashPressed)
            {
                Machine.SetState(Registry.Dash);
                return;
            }

            if (Input.InteractPressed)
            {
                Machine.SetState(Registry.Interact);
                return;
            }

            if (!Input.HasMoveInput)
            {
                Machine.SetState(Registry.Idle);
            }
        }

        public override void FixedTick()
        {
            Motor.Move(Input.MoveInput);
            Motor.ApplyFixedMovement();
        }
    }
}
