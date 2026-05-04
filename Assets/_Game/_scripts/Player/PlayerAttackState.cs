using Woi.Ninja.Core.StateMachine;
using Woi.Ninja.Player.Services;

namespace Woi.Ninja.Player
{
    /// <summary>
    /// Katana combo state: chains up to 3 hits without exiting between swings when queued.
    /// </summary>
    public sealed class PlayerAttackState : PlayerState
    {
        private readonly IPlayerAimProvider _aimProvider;

        private readonly IPlayerSwordFeedback _swordFeedback;

        public PlayerAttackState(
            IPlayerInputReader input,
            IPlayerMotor motor,
            IPlayerDash dash,
            IPlayerInteraction interaction,
            IPlayerCombat combat,
            IPlayerThrower thrower,
            IPlayerAimProvider aimProvider,
            IPlayerSwordFeedback swordFeedback,
            StateMachine machine,
            PlayerStateRegistry registry)
            : base(input, motor, dash, interaction, combat, thrower, machine, registry)
        {
            _aimProvider = aimProvider;
            _swordFeedback = swordFeedback;
        }

        public override void Enter()
        {
            if (_aimProvider.HasAimDirection)
                Motor.FaceWorldDirection(_aimProvider.AimDirection);

            Motor.Stop();
            Combat.BeginAttack();
            Input.ClearAttackInputBuffer();
            _swordFeedback?.PlaySwing(Combat.CurrentComboIndex);
        }

        public override void Exit()
        {
            Combat.ClearAttackFinishedFlag();
        }

        public override void Tick()
        {
            if (Input.AttackPressed)
                Combat.TryQueueNextAttack();
            else if (Combat.CanQueueNextAttack && Input.TryConsumeAttackInputBuffer())
                Combat.TryQueueNextAttack();

            Combat.TickAttack();

            if (!Combat.HasAttackFinished)
                return;

            if (Combat.TryBeginQueuedAttack())
            {
                Input.ClearAttackInputBuffer();
                _swordFeedback?.PlaySwing(Combat.CurrentComboIndex);
                return;
            }

            if (Combat.IsAttacking)
                return;

            if (Input.HasMoveInput)
                Machine.SetState(Registry.Move);
            else
                Machine.SetState(Registry.Idle);
        }

        public override void FixedTick()
        {
            if (Combat.IsInComboRecoveryBuffer)
            {
                Motor.Move(Input.MoveInput);
                Motor.ApplyFixedMovement();
                return;
            }

            if (!Combat.IsAttacking)
                return;

            ComboStepData? stepNullable = Combat.CurrentStep;
            if (stepNullable == null)
            {
                Motor.Stop();
                return;
            }

            if (!Combat.IsInMovementWindow)
            {
                Motor.Stop();
                return;
            }

            var step = stepNullable.Value;

            switch (step.MovementMode)
            {
                case AttackMovementMode.None:
                    Motor.Stop();
                    break;

                case AttackMovementMode.InputOnly:
                    if (Input.HasMoveInput)
                    {
                        Motor.Move(Input.MoveInput, step.InputMovementMultiplier);
                        Motor.ApplyFixedMovement();
                    }
                    else
                    {
                        Motor.Stop();
                    }

                    break;

                case AttackMovementMode.AimOnly:
                    if (_aimProvider.HasAimDirection)
                    {
                        Motor.MoveWorldDirection(_aimProvider.AimDirection, step.AimMovementSpeed);
                        Motor.ApplyFixedMovement();
                    }
                    else
                    {
                        Motor.Stop();
                    }

                    break;

                case AttackMovementMode.InputOrAimFallback:
                    if (Input.HasMoveInput)
                    {
                        Motor.Move(Input.MoveInput, step.InputMovementMultiplier);
                        Motor.ApplyFixedMovement();
                    }
                    else if (_aimProvider.HasAimDirection)
                    {
                        Motor.MoveWorldDirection(_aimProvider.AimDirection, step.AimMovementSpeed);
                        Motor.ApplyFixedMovement();
                    }
                    else
                    {
                        Motor.Stop();
                    }

                    break;
            }
        }
    }
}
