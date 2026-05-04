using UnityEngine;

namespace Woi.Ninja.Player.Services
{
    /// <summary>
    /// Top-down XZ movement using <see cref="Rigidbody.MovePosition"/> and <see cref="Rigidbody.MoveRotation"/>.
    /// Physics step is driven by <see cref="ApplyFixedMovement"/> (from player states), not from this component's <c>FixedUpdate</c>.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public sealed class PlayerMotor : MonoBehaviour, IPlayerMotor
    {
        private const float FaceDirectionEpsilonSqr = 1e-8f;

        [SerializeField] [Min(0f)] private float _moveSpeed = 6f;

        [SerializeField] [Min(0f)] private float _rotationSlerpSpeed = 12f;

        [SerializeField] [Min(0f)] private float _inputEpsilon = 1e-6f;

        private Rigidbody _rigidbody;

        private Vector2 _planarInput;

        private float _speedMultiplier = 1f;

        private bool _worldMoveMode;

        private Vector3 _worldMoveDir;

        private float _worldMoveSpeed;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            if (_rigidbody == null)
            {
                Debug.LogError(
                    "[PlayerMotor] '" + gameObject.name + "' requires a Rigidbody. Disabling PlayerMotor.",
                    this);
                enabled = false;
                return;
            }

            ConfigureRigidbodyForTopDown(_rigidbody);
        }

        private static void ConfigureRigidbodyForTopDown(Rigidbody rb)
        {
            rb.useGravity = false;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }

        public Vector3 Forward
        {
            get
            {
                var f = transform.forward;
                f.y = 0f;
                return f.sqrMagnitude > _inputEpsilon * _inputEpsilon ? f.normalized : Vector3.forward;
            }
        }

        public void Move(Vector2 input)
        {
            Move(input, 1f);
        }

        public void Move(Vector2 input, float speedMultiplier)
        {
            _worldMoveMode = false;
            _planarInput = input;
            _speedMultiplier = Mathf.Max(0f, speedMultiplier);
        }

        public void MoveWorldDirection(Vector3 worldDirection, float speed)
        {
            var dir = worldDirection;
            dir.y = 0f;
            if (dir.sqrMagnitude < _inputEpsilon * _inputEpsilon)
            {
                _worldMoveMode = false;
                _planarInput = Vector2.zero;
                return;
            }

            dir.Normalize();
            _worldMoveMode = true;
            _worldMoveDir = dir;
            _worldMoveSpeed = Mathf.Max(0f, speed);
            _planarInput = Vector2.zero;
            _speedMultiplier = 1f;
        }

        public void FaceWorldDirection(Vector3 worldDirection)
        {
            if (_rigidbody == null)
                return;

            var d = worldDirection;
            d.y = 0f;
            if (d.sqrMagnitude < FaceDirectionEpsilonSqr)
                return;

            d.Normalize();
            var target = Quaternion.LookRotation(d);
            float t = 1f - Mathf.Exp(-_rotationSlerpSpeed * Time.deltaTime);
            _rigidbody.MoveRotation(Quaternion.Slerp(_rigidbody.rotation, target, t));
        }

        public void Stop()
        {
            _worldMoveMode = false;
            _planarInput = Vector2.zero;
            _speedMultiplier = 1f;
            if (_rigidbody == null)
                return;

            ZeroPlanarVelocity();
        }

        public void ApplyFixedMovement()
        {
            if (_rigidbody == null)
                return;

            if (_worldMoveMode)
            {
                var delta = _worldMoveDir * (_worldMoveSpeed * Time.fixedDeltaTime);
                _rigidbody.MovePosition(_rigidbody.position + delta);
                return;
            }

            var dir = new Vector3(_planarInput.x, 0f, _planarInput.y);
            var sq = dir.sqrMagnitude;
            if (sq < _inputEpsilon * _inputEpsilon)
                return;

            if (sq > 1f)
                dir.Normalize();

            float speed = _moveSpeed * _speedMultiplier;
            var deltaInput = dir * (speed * Time.fixedDeltaTime);
            _rigidbody.MovePosition(_rigidbody.position + deltaInput);

            FaceDirection(dir);
        }

        private void FaceDirection(Vector3 directionXZ)
        {
            var target = Quaternion.LookRotation(directionXZ.normalized);
            var t = 1f - Mathf.Exp(-_rotationSlerpSpeed * Time.fixedDeltaTime);
            _rigidbody.MoveRotation(Quaternion.Slerp(_rigidbody.rotation, target, t));
        }

        private void ZeroPlanarVelocity()
        {
#if UNITY_6000_0_OR_NEWER
            var v = _rigidbody.linearVelocity;
            v.x = 0f;
            v.z = 0f;
            _rigidbody.linearVelocity = v;
#else
            var v = _rigidbody.velocity;
            v.x = 0f;
            v.z = 0f;
            _rigidbody.velocity = v;
#endif
        }
    }
}
