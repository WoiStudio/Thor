using UnityEngine;
using Woi.Ninja.Player.Config;

namespace Woi.Ninja.Player.Services
{
    /// <summary>
    /// XZ dash using <see cref="Rigidbody.MovePosition"/>. Next dash allowed after <c>dashDuration + cooldown</c> from <see cref="BeginDash"/>.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public sealed class PlayerDash : MonoBehaviour, IPlayerDash
    {
        [Header("Config")]
        [Tooltip("Drag a Dash Config asset per character. Falls back to values below if empty.")]
        [SerializeField] private PlayerDashConfig _dashConfig;

        [Header("Fallback (no config asset)")]
        [SerializeField] [Min(0f)] private float _dashDistance = 4f;

        [SerializeField] [Min(0.0001f)] private float _dashDuration = 0.18f;

        [SerializeField] [Min(0f)] private float _cooldown = 0.35f;

        [SerializeField] [Min(0f)] private float _inputEpsilon = 0.12f;

        private Rigidbody _rigidbody;

        private float _runtimeDashDistance;
        private float _runtimeDashDuration;
        private float _runtimeCooldown;
        private float _runtimeInputEpsilon;

        private bool _isDashing;
        private Vector3 _dashDirection;
        private float _distanceMoved;

        private float _nextDashAllowedFixedTime;

        private bool _dashFinishedLatch;

        public bool CanDash => !_isDashing && Time.fixedTime >= _nextDashAllowedFixedTime;

        public bool IsDashing => _isDashing;

        public bool HasDashFinished => _dashFinishedLatch;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            if (_rigidbody == null)
            {
                Debug.LogError("[PlayerDash] Missing Rigidbody.", this);
                enabled = false;
                return;
            }

            RefreshRuntimeValues();
        }

        /// <summary>
        /// Re-reads config / fallback (e.g. after swapping <see cref="_dashConfig"/> at runtime).
        /// </summary>
        public void RefreshRuntimeValues()
        {
            if (_dashConfig != null)
            {
                _runtimeDashDistance = _dashConfig.DashDistance;
                _runtimeDashDuration = _dashConfig.DashDuration;
                _runtimeCooldown = _dashConfig.Cooldown;
                _runtimeInputEpsilon = _dashConfig.InputEpsilon;
            }
            else
            {
                _runtimeDashDistance = _dashDistance;
                _runtimeDashDuration = _dashDuration;
                _runtimeCooldown = _cooldown;
                _runtimeInputEpsilon = _inputEpsilon;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Application.isPlaying)
                RefreshRuntimeValues();
        }
#endif

        public void BeginDash(Vector2 inputDirection, Vector3 fallbackForward)
        {
            if (!CanDash || _rigidbody == null)
                return;

            _dashFinishedLatch = false;
            _distanceMoved = 0f;

            _dashDirection = ResolveDashDirection(inputDirection, fallbackForward);

            _isDashing = true;
            _nextDashAllowedFixedTime = Time.fixedTime + _runtimeDashDuration + _runtimeCooldown;
        }

        public void ApplyFixedDash()
        {
            if (!_isDashing || _rigidbody == null)
                return;

            float step = _runtimeDashDistance / _runtimeDashDuration * Time.fixedDeltaTime;
            float remaining = _runtimeDashDistance - _distanceMoved;
            float moveDist = Mathf.Min(step, Mathf.Max(0f, remaining));

            var delta = _dashDirection * moveDist;
            _rigidbody.MovePosition(_rigidbody.position + delta);
            _distanceMoved += moveDist;

            FaceDashDirection(_dashDirection);

            if (_distanceMoved >= _runtimeDashDistance - Mathf.Epsilon)
            {
                _isDashing = false;
                _dashFinishedLatch = true;
            }
        }

        public void ClearDashFinishedFlag()
        {
            _dashFinishedLatch = false;
        }

        private Vector3 ResolveDashDirection(Vector2 inputDirection, Vector3 fallbackForward)
        {
            if (inputDirection.sqrMagnitude > _runtimeInputEpsilon * _runtimeInputEpsilon)
            {
                var xz = new Vector3(inputDirection.x, 0f, inputDirection.y);
                xz.Normalize();
                return xz;
            }

            var f = new Vector3(fallbackForward.x, 0f, fallbackForward.z);
            if (f.sqrMagnitude < 1e-8f)
                f = Vector3.forward;
            f.Normalize();
            return f;
        }

        private void FaceDashDirection(Vector3 directionXZ)
        {
            directionXZ.y = 0f;
            if (directionXZ.sqrMagnitude < 1e-8f)
                return;

            var target = Quaternion.LookRotation(directionXZ.normalized);
            _rigidbody.MoveRotation(target);
        }
    }
}
