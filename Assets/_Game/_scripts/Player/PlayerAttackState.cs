using Woi.Ninja.Core.StateMachine;
using Woi.Ninja.Player.Services;

namespace Woi.Ninja.Player
{
    /// <summary>
    /// Katana combo state: chains up to 3 hits without exiting between swings when queued.
    /// </summary>
    public sealed class PlayerAttackState : PlayerState
    {
        public PlayerAttackState(
            IPlayerInputReader input,
            IPlayerMotor motor,
            IPlayerDash dash,
            IPlayerInteraction interaction,
            IPlayerCombat combat,
            StateMachine machine,
            PlayerStateRegistry registry)
            : base(input, motor, dash, interaction, combat, machine, registry)
        {
        }

        public override void Enter()
        {
            Motor.Stop();
            Combat.BeginAttack();
        }

        public override void Exit()
        {
            Combat.ClearAttackFinishedFlag();
        }

        public override void Tick()
        {
            Combat.TickAttack();

            if (Input.AttackPressed)
                Combat.TryQueueNextAttack();

            if (!Combat.HasAttackFinished)
                return;

            if (Combat.TryBeginQueuedAttack())
                return;

            if (Input.HasMoveInput)
                Machine.SetState(Registry.Move);
            else
                Machine.SetState(Registry.Idle);
        }
    }
}
