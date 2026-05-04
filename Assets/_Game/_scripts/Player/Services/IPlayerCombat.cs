namespace Woi.Ninja.Player.Services
{
    /// <summary>
    /// Katana combo timing. Driven by <see cref="PlayerAttackState"/> calling <see cref="TickAttack"/>.
    /// </summary>
    public interface IPlayerCombat
    {
        bool CanAttack { get; }

        bool IsAttacking { get; }

        bool HasAttackFinished { get; }

        int CurrentComboIndex { get; }

        /// <summary>Active step while attacking; null when not in an attack segment.</summary>
        ComboStepData? CurrentStep { get; }

        /// <summary>Elapsed time in the current attack segment (0 after combo reset).</summary>
        float CurrentAttackElapsedTime { get; }

        /// <summary>True when attack movement (WASD / aim lunge) is allowed for the current step.</summary>
        bool IsInMovementWindow { get; }

        /// <summary>True when the swing segment ended but combo input window is still open (no next hit queued yet).</summary>
        bool IsInComboRecoveryBuffer { get; }

        bool CanQueueNextAttack { get; }

        bool HasQueuedNextAttack { get; }

        void BeginAttack();

        void TickAttack();

        void TryQueueNextAttack();

        bool TryBeginQueuedAttack();

        void ResetCombo();

        void ClearAttackFinishedFlag();
    }
}
