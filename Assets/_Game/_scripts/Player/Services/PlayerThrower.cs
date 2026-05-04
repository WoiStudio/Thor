using UnityEngine;
using Woi.Ninja.Player;

namespace Woi.Ninja.Player.Services
{
    /// <summary>
    /// Spawns a projectile and holds throw/cooldown timing. Updated only via <see cref="TickThrow"/> from the FSM.
    /// </summary>
    public sealed class PlayerThrower : MonoBehaviour, IPlayerThrower
    {
        [SerializeField] [Min(0.0001f)] private float _throwDuration = 0.35f;

        [SerializeField] [Min(0f)] private float _cooldown = 0.5f;

        [SerializeField] private GameObject _projectilePrefab;

        [SerializeField] private Transform _spawnPoint;

        [SerializeField] [Min(0f)] private float _projectileSpeed = 18f;

        private float _segmentTimer;

        private bool _isThrowing;

        private bool _throwFinishedLatch;

        private float _nextThrowAllowedTime;

        public bool CanThrow =>
            !_isThrowing && Time.time >= _nextThrowAllowedTime && _projectilePrefab != null;

        public bool IsThrowing => _isThrowing;

        public bool HasThrowFinished => _throwFinishedLatch;

        public bool BeginThrow(Vector3 direction)
        {
            if (!CanThrow)
                return false;

            if (_projectilePrefab == null)
            {
                Debug.LogWarning("[PlayerThrower] Projectile prefab atanmadi.", this);
                return false;
            }

            Vector3 dir = ProjectAndNormalizeXZ(direction);
            if (dir.sqrMagnitude < 1e-6f)
                dir = ProjectAndNormalizeXZ(transform.forward);

            Transform origin = _spawnPoint != null ? _spawnPoint : transform;
            Quaternion rot = Quaternion.LookRotation(dir, Vector3.up);
            GameObject instance = Instantiate(_projectilePrefab, origin.position, rot);

            var projectile = instance.GetComponent<ShurikenProjectile>();
            if (projectile != null)
                projectile.Initialize(dir, _projectileSpeed);
            else
                Debug.LogWarning("[PlayerThrower] Projectile prefab'da ShurikenProjectile yok.", this);

            _segmentTimer = 0f;
            _isThrowing = true;
            _throwFinishedLatch = false;
            return true;
        }

        public void TickThrow()
        {
            if (!_isThrowing)
                return;

            _segmentTimer += Time.deltaTime;

            if (_segmentTimer < _throwDuration)
                return;

            _isThrowing = false;
            _throwFinishedLatch = true;
            _nextThrowAllowedTime = Time.time + _cooldown;
        }

        public void ClearThrowFinishedFlag()
        {
            _throwFinishedLatch = false;
        }

        private static Vector3 ProjectAndNormalizeXZ(Vector3 v)
        {
            v.y = 0f;
            if (v.sqrMagnitude < 1e-8f)
                return Vector3.zero;

            v.Normalize();
            return v;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            _throwDuration = Mathf.Max(0.0001f, _throwDuration);
            _cooldown = Mathf.Max(0f, _cooldown);
            _projectileSpeed = Mathf.Max(0f, _projectileSpeed);
        }
#endif
    }
}
