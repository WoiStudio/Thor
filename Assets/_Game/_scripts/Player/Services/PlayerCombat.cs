using UnityEngine;

namespace Woi.Ninja.Player.Services
{
    /// <summary>
    /// Combo chain: each step has its own swing length, cancel window, and post-hit cooldown.
    /// Timing only advances via <see cref="TickAttack"/>.
    /// </summary>
    public sealed class PlayerCombat : MonoBehaviour, IPlayerCombat
    {
        [SerializeField] private ComboStepData[] _comboSteps =
        {
            new ComboStepData
            {
                AttackDuration = 0.25f,
                ComboInputOpenTime = 0.12f,
                ComboInputCloseTime = 0.23f,
                CooldownAfterAttack = 0.15f,
            },
            new ComboStepData
            {
                AttackDuration = 0.22f,
                ComboInputOpenTime = 0.1f,
                ComboInputCloseTime = 0.2f,
                CooldownAfterAttack = 0.15f,
            },
            new ComboStepData
            {
                AttackDuration = 0.28f,
                ComboInputOpenTime = 0.11f,
                ComboInputCloseTime = 0.24f,
                CooldownAfterAttack = 0.2f,
            },
        };

        private int _comboIndex;

        private float _segmentTimer;

        private bool _segmentFinishedLatch;

        private bool _queuedNext;

        private float _nextAttackAllowedTime;

        private int MaxComboCount => _comboSteps != null ? _comboSteps.Length : 0;

        public bool CanAttack => _comboIndex == 0 && Time.time >= _nextAttackAllowedTime && HasValidSteps();

        public bool IsAttacking => _comboIndex > 0;

        public bool HasAttackFinished => _segmentFinishedLatch;

        public int CurrentComboIndex => _comboIndex;

        public bool CanQueueNextAttack
        {
            get
            {
                if (!IsAttacking || _segmentFinishedLatch || !TryGetActiveStep(out var step))
                    return false;

                return _segmentTimer >= step.ComboInputOpenTime
                    && _segmentTimer <= step.ComboInputCloseTime
                    && _comboIndex < MaxComboCount;
            }
        }

        public bool HasQueuedNextAttack => _queuedNext;

        public void BeginAttack()
        {
            if (!HasValidSteps())
            {
                Debug.LogError("[PlayerCombat] comboSteps is empty; cannot attack.", this);
                return;
            }

            if (!CanAttack)
                return;

            _comboIndex = 1;
            _segmentTimer = 0f;
            _segmentFinishedLatch = false;
            _queuedNext = false;

            LogCombat("Attack 1 started");
        }

        public void TryQueueNextAttack()
        {
            if (!IsAttacking)
                return;

            if (HasAttackFinished)
                return;

            if (_comboIndex >= MaxComboCount)
                return;

            if (_queuedNext)
                return;

            if (!CanQueueNextAttack)
                return;

            _queuedNext = true;
            LogCombat("Next attack queued");
        }

        public bool TryBeginQueuedAttack()
        {
            if (!_segmentFinishedLatch)
                return false;

            if (!_queuedNext)
            {
                EndCombo();
                return false;
            }

            if (_comboIndex >= MaxComboCount)
            {
                _queuedNext = false;
                EndCombo();
                return false;
            }

            _queuedNext = false;
            _segmentFinishedLatch = false;
            _comboIndex++;
            _segmentTimer = 0f;

            LogCombat("Attack " + _comboIndex + " started");
            return true;
        }

        public void TickAttack()
        {
            if (_comboIndex == 0 || _segmentFinishedLatch || !TryGetActiveStep(out var step))
                return;

            _segmentTimer += Time.deltaTime;
            if (_segmentTimer >= step.AttackDuration)
            {
                _segmentTimer = step.AttackDuration;
                _segmentFinishedLatch = true;
            }
        }

        public void ResetCombo()
        {
            _comboIndex = 0;
            _segmentTimer = 0f;
            _segmentFinishedLatch = false;
            _queuedNext = false;
            _nextAttackAllowedTime = Time.time;
        }

        public void ClearAttackFinishedFlag()
        {
            _segmentFinishedLatch = false;
        }

        private void LogCombat(string message)
        {
            Debug.Log("[PlayerCombat] " + message + " | t=" + Time.time.ToString("F4") + " frame=" + Time.frameCount, this);
        }

        private bool HasValidSteps() => _comboSteps != null && _comboSteps.Length > 0;

        /// <summary>Active step for current combo hit (1-based index → 0-based array).</summary>
        private bool TryGetActiveStep(out ComboStepData step)
        {
            step = default;
            if (_comboIndex < 1 || _comboIndex > MaxComboCount)
                return false;

            step = _comboSteps[_comboIndex - 1];
            return true;
        }

        private void EndCombo()
        {
            LogCombat("Combo ended");

            float delay = 0f;
            if (_comboIndex >= 1 && _comboIndex <= MaxComboCount && HasValidSteps())
                delay = _comboSteps[_comboIndex - 1].CooldownAfterAttack;

            _comboIndex = 0;
            _segmentTimer = 0f;
            _queuedNext = false;
            _segmentFinishedLatch = false;

            _nextAttackAllowedTime = Time.time + delay;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_comboSteps == null)
                return;

            for (var i = 0; i < _comboSteps.Length; i++)
            {
                ref var s = ref _comboSteps[i];
                s.AttackDuration = Mathf.Max(0.0001f, s.AttackDuration);
                s.ComboInputOpenTime = Mathf.Max(0f, s.ComboInputOpenTime);
                s.ComboInputCloseTime = Mathf.Max(s.ComboInputOpenTime, s.ComboInputCloseTime);
                s.CooldownAfterAttack = Mathf.Max(0f, s.CooldownAfterAttack);
                if (s.ComboInputCloseTime > s.AttackDuration)
                    Debug.LogWarning("[PlayerCombat] Step " + (i + 1) + ": ComboInputCloseTime exceeds AttackDuration.", this);
            }
        }
#endif
    }
}
