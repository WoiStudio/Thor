using UnityEngine;
using UnityEngine.InputSystem;

namespace Woi.Ninja.Player.Services
{
    /// <summary>
    /// Raycasts mouse through a serialized camera onto the XZ plane at the player's Y.
    /// </summary>
    [DefaultExecutionOrder(-40)]
    public sealed class PlayerAimProvider : MonoBehaviour, IPlayerAimProvider
    {
        private const float MinAimSqr = 1e-6f;

        private const float ParallelRayEpsilon = 1e-5f;

        [SerializeField] private Camera _camera;

        [SerializeField] private Transform _playerRoot;

        private bool _hasAim;

        private Vector3 _aimDirection;

        private Vector3 _aimWorldPoint;

        public bool HasAimDirection => _hasAim;

        public Vector3 AimDirection => _aimDirection;

        public Vector3 AimWorldPoint => _aimWorldPoint;

        private void Awake()
        {
            if (_playerRoot == null)
                _playerRoot = transform;
        }

        private void Update()
        {
            RefreshAim();
        }

        private void RefreshAim()
        {
            _hasAim = false;
            _aimDirection = Vector3.zero;
            _aimWorldPoint = Vector3.zero;

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

            _hasAim = true;
            _aimWorldPoint = hit;
            _aimDirection = fromPlayer.normalized;
        }
    }
}
