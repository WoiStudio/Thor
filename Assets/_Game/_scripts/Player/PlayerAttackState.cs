using UnityEngine;
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

        private bool _lockedLungeAimValid;

        private Vector3 _lockedLungeAimDir;

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
            ClearLungeAimLock();

            SnapFacingToCurrentAim();

            Motor.Stop();
            Combat.BeginAttack();
            Input.ClearAttackInputBuffer();
            _swordFeedback?.PlaySwing(Combat.CurrentComboIndex);
        }

        public override void Exit()
        {
            ClearLungeAimLock();
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
                ClearLungeAimLock();
                SnapFacingToCurrentAim();
                Motor.Stop();
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

        private void SnapFacingToCurrentAim()
        {
            _aimProvider.SampleAimNow();

            if (_aimProvider.HasAimDirection)
                Motor.FaceWorldDirectionImmediate(_aimProvider.AimDirection);
        }

        private void ClearLungeAimLock()
        {
            _lockedLungeAimValid = false;
            _lockedLungeAimDir = Vector3.zero;
        }

        private bool TryGetLockedLungeAimDirection(out Vector3 dir)
        {
            if (_lockedLungeAimValid)
            {
                dir = _lockedLungeAimDir;
                return true;
            }

            _aimProvider.SampleAimNow();
            if (!_aimProvider.HasAimDirection)
            {
                dir = Vector3.zero;
                return false;
            }

            _lockedLungeAimDir = _aimProvider.AimDirection;
            _lockedLungeAimValid = true;
            dir = _lockedLungeAimDir;
            return true;
        }

        public override void FixedTick()
        {
            if (Combat.IsInComboRecoveryBuffer)
            {
                ClearLungeAimLock();
                Motor.Move(Input.MoveInput);
                Motor.ApplyFixedMovement(false);
                return;
            }

            if (!Combat.IsAttacking)
            {
                ClearLungeAimLock();
                return;
            }

            ComboStepData? stepNullable = Combat.CurrentStep;
            if (stepNullable == null)
            {
                ClearLungeAimLock();
                Motor.Stop();
                return;
            }

            if (!Combat.IsInMovementWindow)
            {
                ClearLungeAimLock();
                Motor.Stop();
                return;
            }

            var step = stepNullable.Value;

            switch (step.MovementMode)
            {
                case AttackMovementMode.None:
                    ClearLungeAimLock();
                    Motor.Stop();
                    break;

                case AttackMovementMode.InputOnly:
                    ClearLungeAimLock();
                    if (Input.HasMoveInput)
                    {
                        Motor.Move(Input.MoveInput, step.InputMovementMultiplier);
                        Motor.ApplyFixedMovement(false);
                    }
                    else
                    {
                        Motor.Stop();
                    }

                    break;

                case AttackMovementMode.AimOnly:
                    if (TryGetLockedLungeAimDirection(out var aimDirOnly))
                    {
                        Motor.MoveWorldDirection(aimDirOnly, step.AimMovementSpeed);
                        Motor.ApplyFixedMovement(false);
                    }
                    else
                    {
                        ClearLungeAimLock();
                        Motor.Stop();
                    }

                    break;

                case AttackMovementMode.InputOrAimFallback:
                    if (Input.HasMoveInput)
                    {
                        ClearLungeAimLock();
                        Motor.Move(Input.MoveInput, step.InputMovementMultiplier);
                        Motor.ApplyFixedMovement(false);
                    }
                    else if (TryGetLockedLungeAimDirection(out var aimDirFb))
                    {
                        Motor.MoveWorldDirection(aimDirFb, step.AimMovementSpeed);
                        Motor.ApplyFixedMovement(false);
                    }
                    else
                    {
                        ClearLungeAimLock();
                        Motor.Stop();
                    }

                    break;
            }
        }
    }
}
