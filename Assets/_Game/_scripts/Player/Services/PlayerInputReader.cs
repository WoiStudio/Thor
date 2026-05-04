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
    }
}
