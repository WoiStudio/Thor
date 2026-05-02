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
        [SerializeField] [Min(0f)] private float _moveSpeed = 6f;

        [SerializeField] [Min(0f)] private float _rotationSlerpSpeed = 12f;

        [SerializeField] [Min(0f)] private float _inputEpsilon = 1e-6f;

        private Rigidbody _rigidbody;
        private Vector2 _planarInput;

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
            // Freeze pitch/roll; leave Y free so MoveRotation can yaw toward movement.
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }

        /// <summary>
        /// Caches desired planar input for the next <see cref="ApplyFixedMovement"/> call(s).
        /// </summary>
        public void Move(Vector2 input)
        {
            _planarInput = input;
        }

        /// <summary>
        /// Clears desired motion and zeros horizontal linear velocity.
        /// </summary>
        public void Stop()
        {
            _planarInput = Vector2.zero;
            if (_rigidbody == null)
                return;

            ZeroPlanarVelocity();
        }

        /// <summary>
        /// Performs <see cref="Rigidbody.MovePosition"/> on the XZ plane and rotates to face travel direction.
        /// </summary>
        public void ApplyFixedMovement()
        {
            if (_rigidbody == null)
                return;

            var dir = new Vector3(_planarInput.x, 0f, _planarInput.y);
            var sq = dir.sqrMagnitude;
            if (sq < _inputEpsilon * _inputEpsilon)
                return;

            if (sq > 1f)
                dir.Normalize();

            var delta = dir * (_moveSpeed * Time.fixedDeltaTime);
            _rigidbody.MovePosition(_rigidbody.position + delta);

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
