using UnityEngine;
using Woi.Ninja.Core.StateMachine;
using Woi.Ninja.Player.Services;

namespace Woi.Ninja.Player
{
    /// <summary>
    /// Owns the player FSM and resolves service components from serialized fields or the same GameObject.
    /// </summary>
    public sealed class PlayerController : MonoBehaviour
    {
        [SerializeField] private PlayerInputReader _inputReader;
        [SerializeField] private PlayerMotor _motor;
        [SerializeField] private PlayerDash _dash;
        [SerializeField] private PlayerInteraction _interaction;
        [SerializeField] private PlayerCombat _combat;
        [SerializeField] private PlayerThrower _thrower;
        [SerializeField] private PlayerAimProvider _aimProvider;
        [SerializeField] private PlayerSwordFeedback _swordFeedback;

        private StateMachine _stateMachine;

        private PlayerIdleState _idle;
        private PlayerMoveState _move;
        private PlayerDashState _dashState;
        private PlayerInteractState _interact;
        private PlayerAttackState _attack;
        private PlayerThrowState _throw;

        public StateMachine StateMachine => _stateMachine;

        private void Awake()
        {
            if (!ResolveServices())
                return;

            CreateStateMachine();
        }

        private void Update()
        {
            _stateMachine?.Tick();
        }

        private void FixedUpdate()
        {
            _stateMachine?.FixedTick();
        }

        private bool ResolveServices()
        {
            if (!TryResolve(ref _inputReader))
            {
                LogMissingService(nameof(PlayerInputReader));
                enabled = false;
                return false;
            }

            if (!TryResolve(ref _motor))
            {
                LogMissingService(nameof(PlayerMotor));
                enabled = false;
                return false;
            }

            if (!TryResolve(ref _dash))
            {
                LogMissingService(nameof(PlayerDash));
                enabled = false;
                return false;
            }

            if (!TryResolve(ref _interaction))
            {
                LogMissingService(nameof(PlayerInteraction));
                enabled = false;
                return false;
            }

            if (!TryResolve(ref _combat))
            {
                LogMissingService(nameof(PlayerCombat));
                enabled = false;
                return false;
            }

            if (!TryResolve(ref _thrower))
            {
                LogMissingService(nameof(PlayerThrower));
                enabled = false;
                return false;
            }

            if (!TryResolve(ref _aimProvider))
            {
                LogMissingService(nameof(PlayerAimProvider));
                enabled = false;
                return false;
            }

            if (_swordFeedback == null)
                _swordFeedback = GetComponentInChildren<PlayerSwordFeedback>(true);

            if (_swordFeedback == null)
            {
                Debug.LogWarning(
                    "[PlayerController] '" + gameObject.name
                    + "': PlayerSwordFeedback not found — sword swing feedback disabled. Add component or assign field.",
                    this);
            }

            return true;
        }

        private void CreateStateMachine()
        {
            _stateMachine = new StateMachine();

            var registry = new PlayerStateRegistry();

            IPlayerSwordFeedback sword = _swordFeedback;

            _idle = new PlayerIdleState(_inputReader, _motor, _dash, _interaction, _combat, _thrower, _stateMachine, registry);
            _move = new PlayerMoveState(_inputReader, _motor, _dash, _interaction, _combat, _thrower, _stateMachine, registry);
            _dashState = new PlayerDashState(_inputReader, _motor, _dash, _interaction, _combat, _thrower, _stateMachine, registry);
            _interact = new PlayerInteractState(_inputReader, _motor, _dash, _interaction, _combat, _thrower, _stateMachine, registry);
            _attack = new PlayerAttackState(
                _inputReader,
                _motor,
                _dash,
                _interaction,
                _combat,
                _thrower,
                _aimProvider,
                sword,
                _stateMachine,
                registry);
            _throw = new PlayerThrowState(_inputReader, _motor, _dash, _interaction, _combat, _thrower, _stateMachine, registry);

            registry.Idle = _idle;
            registry.Move = _move;
            registry.Dash = _dashState;
            registry.Interact = _interact;
            registry.Attack = _attack;
            registry.Throw = _throw;
            registry.PlayerTransform = transform;

            _stateMachine.SetState(_idle);
        }

        private bool TryResolve<T>(ref T component) where T : Component
        {
            if (component != null)
                return true;

            component = GetComponent<T>();
            return component != null;
        }

        private void LogMissingService(string componentName)
        {
            Debug.LogError(
                "[PlayerController] '" + gameObject.name + "': could not resolve " + componentName
                + ". Assign the serialized field on PlayerController or add a " + componentName
                + " component to this GameObject.",
                this);
        }
    }
}
