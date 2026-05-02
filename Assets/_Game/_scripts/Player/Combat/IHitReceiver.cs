namespace Woi.Ninja.Player.Combat
{
    /// <summary>
    /// Implemented by damageable enemies / props.
    /// </summary>
    public interface IHitReceiver
    {
        void ReceiveHit(DamageInfo damageInfo);
    }
}
