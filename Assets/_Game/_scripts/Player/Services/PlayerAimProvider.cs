using UnityEngine;
using UnityEngine.InputSystem;

namespace Woi.Ninja.Player.Services
{
    /// <summary>
    /// Raycasts mouse through a serialized camera onto the XZ plane at the player's Y.
    /// Aim exposed to gameplay is <b>snapshotted each physics step</b> so movement/lunge in <c>FixedUpdate</c>
    /// does not fight per-frame <c>Update</c> mouse jitter.
    /// </summary>
    [DefaultExecutionOrder(-40)]
    public sealed class PlayerAimProvider : MonoBehaviour, IPlayerAimProvider
    {
        private const float MinAimSqr = 1e-6f;

        private const float ParallelRayEpsilon = 1e-5f;

        [SerializeField] private Camera _camera;

        [SerializeField] private Transform _playerRoot;

        private bool _fixedHasAim;

        private Vector3 _fixedAimDirection;

        private Vector3 _fixedAimWorldPoint;

        public bool HasAimDirection => _fixedHasAim;

        public Vector3 AimDirection => _fixedAimDirection;

        public Vector3 AimWorldPoint => _fixedAimWorldPoint;

        public void SampleAimNow()
        {
            ComputeAndWriteFixedSnapshot();
        }

        private void Awake()
        {
            if (_playerRoot == null)
                _playerRoot = transform;

            ComputeAndWriteFixedSnapshot();
        }

        private void FixedUpdate()
        {
            ComputeAndWriteFixedSnapshot();
        }

        private void ComputeAndWriteFixedSnapshot()
        {
            _fixedHasAim = false;
            _fixedAimDirection = Vector3.zero;
            _fixedAimWorldPoint = Vector3.zero;

            if (_camera == null || _playerRoot == null)
                return;

            var mouse = Mouse.current;
            if (mouse == null)
                return;

            Vector2 screen = mouse.position.ReadValue();
            var ray = _camera.ScreenPointToRay(screen);

            float planeY = _playerRoot.position.y;
            float dy = ray.direction.y;
            if (Mathf.Abs(dy) < ParallelRayEpsilon)
                return;

            float t = (planeY - ray.origin.y) / dy;
            if (t < 0f)
                return;

            Vector3 hit = ray.origin + ray.direction * t;
            Vector3 fromPlayer = hit - _playerRoot.position;
            fromPlayer.y = 0f;

            if (fromPlayer.sqrMagnitude < MinAimSqr)
                return;

            _fixedHasAim = true;
            _fixedAimWorldPoint = hit;
            _fixedAimDirection = fromPlayer.normalized;
        }
    }
}
