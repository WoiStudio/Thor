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
            IPlayerCombat combat,
            IPlayerThrower thrower,
            StateMachine machine,
            PlayerStateRegistry registry)
            : base(input, motor, dash, interaction, combat, thrower, machine, registry)
        {
        }

        public override void Tick()
        {
            Motor.Stop();

            if (Input.AttackPressed && Combat.CanAttack)
            {
                Machine.SetState(Registry.Attack);
                return;
            }

            if (Combat.CanAttack && Input.TryConsumeAttackInputBuffer())
            {
                Machine.SetState(Registry.Attack);
                return;
            }

            if (Input.ThrowPressed && Thrower.CanThrow)
            {
                Machine.SetState(Registry.Throw);
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
                return;
            }

            if (Input.HasMoveInput)
            {
                Machine.SetState(Registry.Move);
            }
        }
    }
}
