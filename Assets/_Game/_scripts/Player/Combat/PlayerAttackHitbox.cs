using System.Collections.Generic;
using UnityEngine;

namespace Woi.Ninja.Player.Combat
{
    /// <summary>
    /// Trigger-based melee hitbox; toggled per swing via <see cref="BeginHitWindow"/>.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public sealed class PlayerAttackHitbox : MonoBehaviour
    {
        [Header("Debug")]
        [SerializeField] private bool _logHitWindow;

        [SerializeField] private bool _logOverlapScan;

        private Collider _collider;

        private DamageInfo _activeDamage;

        /// <summary>Receiver <see cref="MonoBehaviour"/> instance IDs hit this swing (no double-damage same component).</summary>
        private readonly HashSet<int> _hitReceiverComponentIdsThisSwing = new HashSet<int>();

        private void Awake()
        {
            EnsureColliderCreated();
        }

        /// <summary>
        /// Hitbox child may start inactive; Awake might not have run yet. Call before any hit logic.
        /// </summary>
        private void EnsureColliderCreated()
        {
            if (_collider != null)
                return;

            _collider = GetComponent<Collider>();
            if (_collider == null)
            {
                Debug.LogError("[PlayerAttackHitbox] Collider bulunamadi.", this);
                return;
            }

            _collider.isTrigger = true;
            _collider.enabled = false;
        }

        /// <summary>
        /// Opens the hitbox for this swing and clears per-swing hit tracking.
        /// </summary>
        /// <returns>False if the hitbox could not activate (e.g. inactive GameObject).</returns>
        public bool BeginHitWindow(DamageInfo damageInfo)
        {
            EnsureColliderCreated();
            if (_collider == null)
                return false;

            if (!gameObject.activeInHierarchy)
            {
                Debug.LogWarning(
                    "[PlayerAttackHitbox] GameObject aktif degil — hitbox overlap/trigger calismaz. Child objeyi aktif tut.",
                    this);
                return false;
            }

            _activeDamage = damageInfo;
            _hitReceiverComponentIdsThisSwing.Clear();
            _collider.enabled = true;

            if (_logHitWindow)
                Debug.Log("[PlayerAttackHitbox] Hit window OPEN", this);

            Physics.SyncTransforms();
            // Enabling a trigger that already overlaps another collider often skips OnTriggerEnter;
            // resolve overlaps once when the window opens.
            ProcessOverlapsAtOpen();
            return true;
        }

        /// <summary>
        /// Disables the hitbox until the next window.
        /// </summary>
        public void EndHitWindow()
        {
            if (_collider != null)
                _collider.enabled = false;

            if (_logHitWindow)
                Debug.Log("[PlayerAttackHitbox] Hit window CLOSED", this);
        }

        private void OnTriggerEnter(Collider other)
        {
            TryHit(other);
        }

        private void ProcessOverlapsAtOpen()
        {
            if (_collider == null)
                return;

            Collider[] cols;
            if (_collider is BoxCollider box)
            {
                var t = box.transform;
                var center = t.TransformPoint(box.center);
                var halfExtents = Vector3.Scale(box.size * 0.5f, AbsVec(t.lossyScale));
                cols = Physics.OverlapBox(center, halfExtents, t.rotation, ~0, QueryTriggerInteraction.Collide);
            }
            else
            {
                var b = _collider.bounds;
                cols = Physics.OverlapBox(b.center, b.extents, Quaternion.identity, ~0, QueryTriggerInteraction.Collide);
            }

            if (_logOverlapScan)
                Debug.Log("[PlayerAttackHitbox] Overlap scan hit count=" + cols.Length, this);

            for (var i = 0; i < cols.Length; i++)
                TryHit(cols[i]);
        }

        private static Vector3 AbsVec(Vector3 v) =>
            new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));

        private void TryHit(Collider other)
        {
            EnsureColliderCreated();
            if (_collider == null || !_collider.enabled || other == null || other == _collider)
                return;

            // Deliver hit to every IHitReceiver on this branch (e.g. Dummy + Health on same GameObject).
            for (Transform tr = other.transform; tr != null; tr = tr.parent)
            {
                var behaviours = tr.GetComponents<MonoBehaviour>();
                for (var i = 0; i < behaviours.Length; i++)
                {
                    var mb = behaviours[i];
                    if (mb == null || !(mb is IHitReceiver receiver))
                        continue;

                    int componentId = mb.GetInstanceID();
                    if (_hitReceiverComponentIdsThisSwing.Contains(componentId))
                        continue;

                    _hitReceiverComponentIdsThisSwing.Add(componentId);
                    receiver.ReceiveHit(_activeDamage);
                }
            }
        }
    }
}
