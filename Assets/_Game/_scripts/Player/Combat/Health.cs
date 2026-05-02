using UnityEngine;

namespace Woi.Ninja.Player.Combat
{
    /// <summary>
    /// Minimal HP + death. Implements <see cref="IHitReceiver"/> for weapon hits.
    /// </summary>
    public sealed class Health : MonoBehaviour, IHitReceiver
    {
        [SerializeField] private int _maxHealth = 100;

        [SerializeField] private bool _destroyOnDeath = true;

        private int _current;

        public int Current => _current;

        public int Max => _maxHealth;

        public bool IsDead { get; private set; }

        private void Awake()
        {
            _current = Mathf.Max(1, _maxHealth);
        }

        public void ReceiveHit(DamageInfo damageInfo)
        {
            if (IsDead)
                return;

            int amount = Mathf.Max(0, damageInfo.Damage);
            _current -= amount;

            if (_current <= 0)
                Die();
        }

        private void Die()
        {
            if (IsDead)
                return;

            IsDead = true;

            if (_destroyOnDeath)
                Destroy(gameObject);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            _maxHealth = Mathf.Max(1, _maxHealth);
        }
#endif
    }
}
