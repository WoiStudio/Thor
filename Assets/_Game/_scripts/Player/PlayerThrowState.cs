using UnityEngine;
using Woi.Ninja.Core.StateMachine;
using Woi.Ninja.Player.Services;

namespace Woi.Ninja.Player
{
    /// <summary>
    /// Ranged throw; optional movement lock for the throw window (see <see cref="IPlayerThrower.StopMovementDuringThrow"/>).
    /// </summary>
    public sealed class PlayerThrowState : PlayerState
    {
        public PlayerThrowState(
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

        public override void Enter()
        {
            if (Thrower.StopMovementDuringThrow)
                Motor.Stop();

            Vector3 forward = Registry.PlayerTransform != null
                ? Registry.PlayerTransform.forward
                : Vector3.forward;

            if (!Thrower.BeginThrow(forward))
            {
                if (Input.HasMoveInput)
                    Machine.SetState(Registry.Move);
                else
                    Machine.SetState(Registry.Idle);
            }
        }

        public override void Exit()
        {
            Thrower.ClearThrowFinishedFlag();
        }

        public override void Tick()
        {
            Thrower.TickThrow();

            if (!Thrower.HasThrowFinished)
                return;

            if (Input.HasMoveInput)
                Machine.SetState(Registry.Move);
            else
                Machine.SetState(Registry.Idle);
        }

        public override void FixedTick()
        {
            if (Thrower.StopMovementDuringThrow)
                return;

            Motor.Move(Input.MoveInput);
            Motor.ApplyFixedMovement();
        }
    }
}
