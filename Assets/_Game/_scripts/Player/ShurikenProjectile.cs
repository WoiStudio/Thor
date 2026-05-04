using UnityEngine;

namespace Woi.Ninja.Player
{
    /// <summary>
    /// Simple XZ shuriken mover using <see cref="Rigidbody"/> velocity.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public sealed class ShurikenProjectile : MonoBehaviour
    {
        [SerializeField] [Min(0.05f)] private float _lifetime = 4f;

        private Rigidbody _rigidbody;

        private float _lifeLeft;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _rigidbody.useGravity = false;
            _rigidbody.constraints |= RigidbodyConstraints.FreezePositionY;
        }

        /// <summary>
        /// Call immediately after instantiate. Direction should be horizontal.
        /// </summary>
        public void Initialize(Vector3 direction, float speed)
        {
            Vector3 dir = direction;
            dir.y = 0f;
            if (dir.sqrMagnitude < 1e-8f)
                dir = Vector3.forward;
            dir.Normalize();

            Vector3 vel = dir * speed;
            vel.y = 0f;
            _rigidbody.linearVelocity = vel;

            _lifeLeft = _lifetime;
        }

        private void Update()
        {
            _lifeLeft -= Time.deltaTime;
            if (_lifeLeft <= 0f)
                Destroy(gameObject);

            Vector3 v = _rigidbody.linearVelocity;
            v.y = 0f;
            _rigidbody.linearVelocity = v;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            _lifetime = Mathf.Max(0.05f, _lifetime);
        }
#endif
    }
}
