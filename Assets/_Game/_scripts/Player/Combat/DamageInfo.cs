using UnityEngine;

namespace Woi.Ninja.Player.Combat
{
    /// <summary>
    /// Payload sent to <see cref="IHitReceiver"/> when a weapon hit connects.
    /// </summary>
    public struct DamageInfo
    {
        public GameObject Source;

        public int Damage;

        public Vector3 HitDirection;

        public int ComboIndex;
    }
}
