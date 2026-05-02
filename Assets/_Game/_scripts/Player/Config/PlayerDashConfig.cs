using UnityEngine;

namespace Woi.Ninja.Player.Config
{
    /// <summary>
    /// Dash tuning per character / prefab. Assign on <see cref="Services.PlayerDash"/>.
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerDashConfig", menuName = "Woi/Ninja/Player/Dash Config")]
    public sealed class PlayerDashConfig : ScriptableObject
    {
        [SerializeField] [Min(0f)] private float _dashDistance = 4f;

        [SerializeField] [Min(0.0001f)] private float _dashDuration = 0.18f;

        [SerializeField] [Min(0f)] private float _cooldown = 0.35f;

        [SerializeField] [Min(0f)] private float _inputEpsilon = 0.12f;

        public float DashDistance => _dashDistance;

        public float DashDuration => _dashDuration;

        public float Cooldown => _cooldown;

        public float InputEpsilon => _inputEpsilon;

        private void OnValidate()
        {
            _dashDistance = Mathf.Max(0f, _dashDistance);
            _dashDuration = Mathf.Max(0.0001f, _dashDuration);
            _cooldown = Mathf.Max(0f, _cooldown);
            _inputEpsilon = Mathf.Max(0f, _inputEpsilon);
        }
    }
}
