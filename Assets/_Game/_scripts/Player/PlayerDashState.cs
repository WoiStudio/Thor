using UnityEngine;
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
            IPlayerCombat combat,
            IPlayerThrower thrower,
            StateMachine machine,
            PlayerStateRegistry registry)
            : base(input, motor, dash, interaction, combat, thrower, machine, registry)
        {
        }

        public override void Enter()
        {
            Motor.Stop();

            var fallback = Registry.PlayerTransform != null
                ? Registry.PlayerTransform.forward
                : Vector3.forward;

            Dash.BeginDash(Input.MoveInput, fallback);
        }

        public override void Exit()
        {
            Dash.ClearDashFinishedFlag();
        }

        public override void Tick()
        {
            if (!Dash.HasDashFinished)
                return;

            if (Input.HasMoveInput)
                Machine.SetState(Registry.Move);
            else
                Machine.SetState(Registry.Idle);
        }

        public override void FixedTick()
        {
            Dash.ApplyFixedDash();
        }
    }
}
