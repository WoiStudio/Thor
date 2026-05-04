using UnityEngine;
using Woi.Ninja.Core.Input;

namespace Woi.Ninja.Player.Services
{
    /// <summary>
    /// Bridges <see cref="IPlayerInputReader"/> to the core ninja gameplay input module.
    /// Does not call <see cref="UnityEngine.Input"/> directly; all sampling lives in <see cref="NinjaGameplayInputModule"/>.
    /// </summary>
    [RequireComponent(typeof(NinjaGameplayInputModule))]
    public sealed class PlayerInputReader : MonoBehaviour, IPlayerInputReader
    {
        [Tooltip("Attack presses within this many seconds can still start or continue a combo after timing edges.")]
        [SerializeField] [Min(0f)] private float _attackInputBufferSeconds = 0.18f;

        private INinjaGameplayInput _gameplayInput;

        private void Awake()
        {
            _gameplayInput = GetComponent<NinjaGameplayInputModule>();
            if (_gameplayInput == null)
            {
                Debug.LogError(
                    "[PlayerInputReader] '" + gameObject.name + "' requires NinjaGameplayInputModule.",
                    this);
                enabled = false;
            }
        }

        public Vector2 MoveInput => _gameplayInput != null ? _gameplayInput.MoveInput : default;

        public bool HasMoveInput => _gameplayInput != null && _gameplayInput.HasMoveInput;

        public bool DashPressed => _gameplayInput != null && _gameplayInput.DashPressed;

        public bool InteractPressed => _gameplayInput != null && _gameplayInput.InteractPressed;

        public bool AttackPressed => _gameplayInput != null && _gameplayInput.AttackPressed;

        public bool ThrowPressed => _gameplayInput != null && _gameplayInput.ThrowPressed;

        public bool TryConsumeAttackInputBuffer()
        {
            if (_gameplayInput == null)
                return false;

            return _gameplayInput.TryConsumeAttackInputBuffer(_attackInputBufferSeconds);
        }

        public void ClearAttackInputBuffer()
        {
            _gameplayInput?.ClearAttackInputBuffer();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            _attackInputBufferSeconds = Mathf.Max(0f, _attackInputBufferSeconds);
        }
#endif
    }
}
