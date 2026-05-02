using UnityEngine;

namespace Woi.Ninja.Player.Combat
{
    /// <summary>
    /// Test receiver for katana hitbox.
    /// </summary>
    public sealed class DummyHitReceiver : MonoBehaviour, IHitReceiver
    {
        public void ReceiveHit(DamageInfo damageInfo)
        {
            Debug.Log(
                "[DummyHitReceiver] Hit from '" + (damageInfo.Source != null ? damageInfo.Source.name : "null")
                + "' damage=" + damageInfo.Damage + " comboIndex=" + damageInfo.ComboIndex
                + " dir=" + damageInfo.HitDirection,
                this);
        }
    }
}
