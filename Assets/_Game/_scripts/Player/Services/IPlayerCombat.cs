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
