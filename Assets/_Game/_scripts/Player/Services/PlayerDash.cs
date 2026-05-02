using UnityEngine;

namespace Woi.Ninja.Player.Services
{
    /// <summary>
    /// XZ dash using <see cref="Rigidbody.MovePosition"/>. Next dash allowed after <c>dashDuration + cooldown</c> from <see cref="BeginDash"/>.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public sealed class PlayerDash : MonoBehaviour, IPlayerDash
    {
        [SerializeField] [Min(0f)] private float _dashDistance = 4f;

        [SerializeField] [Min(0.0001f)] private float _dashDuration = 0.18f;

        [SerializeField] [Min(0f)] private float _cooldown = 0.35f;

        [SerializeField] [Min(0f)] private float _inputEpsilon = 0.12f;

        private Rigidbody _rigidbody;

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
            }
        }

        public void BeginDash(Vector2 inputDirection, Vector3 fallbackForward)
        {
            if (!CanDash || _rigidbody == null)
                return;

            _dashFinishedLatch = false;
            _distanceMoved = 0f;

            _dashDirection = ResolveDashDirection(inputDirection, fallbackForward);

            _isDashing = true;
            _nextDashAllowedFixedTime = Time.fixedTime + _dashDuration + _cooldown;
        }

        public void ApplyFixedDash()
        {
            if (!_isDashing || _rigidbody == null)
                return;

            float step = _dashDistance / _dashDuration * Time.fixedDeltaTime;
            float remaining = _dashDistance - _distanceMoved;
            float moveDist = Mathf.Min(step, Mathf.Max(0f, remaining));

            var delta = _dashDirection * moveDist;
            _rigidbody.MovePosition(_rigidbody.position + delta);
            _distanceMoved += moveDist;

            FaceDashDirection(_dashDirection);

            if (_distanceMoved >= _dashDistance - Mathf.Epsilon)
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
            if (inputDirection.sqrMagnitude > _inputEpsilon * _inputEpsilon)
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
