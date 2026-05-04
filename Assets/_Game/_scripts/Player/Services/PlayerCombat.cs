using UnityEngine;
using Woi.Ninja.Player.Combat;

namespace Woi.Ninja.Player.Services
{
    /// <summary>
    /// Combo chain: each step has its own swing length, cancel window, hitbox window, and cooldown.
    /// Timing only advances via <see cref="TickAttack"/>.
    /// </summary>
    public sealed class PlayerCombat : MonoBehaviour, IPlayerCombat
    {
        [Header("Hit detection")]
        [SerializeField] private PlayerAttackHitbox _attackHitbox;

        [SerializeField] private ComboStepData[] _comboSteps =
        {
            new ComboStepData
            {
                Damage = 10,
                AttackDuration = 0.25f,
                ComboInputOpenTime = 0.12f,
                ComboInputCloseTime = 0.23f,
                HitboxOpenTime = 0.05f,
                HitboxCloseTime = 0.18f,
                CooldownAfterAttack = 0.15f,
                MovementMode = AttackMovementMode.InputOnly,
                InputMovementMultiplier = 0.35f,
                AimMovementSpeed = 0f,
                MovementOpenTime = 0f,
                MovementCloseTime = 0.18f,
            },
            new ComboStepData
            {
                Damage = 12,
                AttackDuration = 0.22f,
                ComboInputOpenTime = 0.1f,
                ComboInputCloseTime = 0.2f,
                HitboxOpenTime = 0.04f,
                HitboxCloseTime = 0.16f,
                CooldownAfterAttack = 0.15f,
                MovementMode = AttackMovementMode.InputOnly,
                InputMovementMultiplier = 0.30f,
                AimMovementSpeed = 0f,
                MovementOpenTime = 0f,
                MovementCloseTime = 0.20f,
            },
            new ComboStepData
            {
                Damage = 15,
                AttackDuration = 0.28f,
                ComboInputOpenTime = 0.11f,
                ComboInputCloseTime = 0.24f,
                HitboxOpenTime = 0.06f,
                HitboxCloseTime = 0.22f,
                CooldownAfterAttack = 0.2f,
                MovementMode = AttackMovementMode.InputOrAimFallback,
                InputMovementMultiplier = 0.20f,
                AimMovementSpeed = 4.5f,
                MovementOpenTime = 0.05f,
                MovementCloseTime = 0.24f,
            },
        };

        private int _comboIndex;

        private float _segmentTimer;

        /// <summary>Real time when the current combo step segment started (for combo input after swing ends).</summary>
        private float _segmentWallClockStart;

        private bool _segmentFinishedLatch;

        private bool _queuedNext;

        private float _nextAttackAllowedTime;

        private bool _hitboxWindowActive;

        private int MaxComboCount => _comboSteps != null ? _comboSteps.Length : 0;

        public bool CanAttack => _comboIndex == 0 && Time.time >= _nextAttackAllowedTime && HasValidSteps();

        public bool IsAttacking => _comboIndex > 0;

        public bool HasAttackFinished => _segmentFinishedLatch;

        public int CurrentComboIndex => _comboIndex;

        public ComboStepData? CurrentStep
        {
            get
            {
                if (_comboIndex < 1 || _comboIndex > MaxComboCount || !HasValidSteps())
                    return null;

                return _comboSteps[_comboIndex - 1];
            }
        }

        public float CurrentAttackElapsedTime => _comboIndex > 0 ? _segmentTimer : 0f;

        public bool IsInMovementWindow
        {
            get
            {
                if (!IsAttacking || !TryGetActiveStep(out var step) || HasAttackFinished)
                    return false;

                return _segmentTimer >= step.MovementOpenTime && _segmentTimer <= step.MovementCloseTime;
            }
        }

        public bool IsInComboRecoveryBuffer
        {
            get
            {
                if (!IsAttacking || !HasAttackFinished || HasQueuedNextAttack || !TryGetActiveStep(out var step))
                    return false;

                return ComboInputElapsed <= step.ComboInputCloseTime;
            }
        }

        public bool CanQueueNextAttack
        {
            get
            {
                if (!IsAttacking || !TryGetActiveStep(out var step) || _comboIndex >= MaxComboCount)
                    return false;

                float elapsed = ComboInputElapsed;
                return elapsed >= step.ComboInputOpenTime && elapsed <= step.ComboInputCloseTime;
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

            if (_attackHitbox == null)
                Debug.LogWarning("[PlayerCombat] Attack Hitbox referansi yok — Hasar/tetik yok. Inspector'dan ata.", this);

            EnsureHitboxClosed();

            _comboIndex = 1;
            _segmentTimer = 0f;
            _segmentWallClockStart = Time.time;
            _segmentFinishedLatch = false;
            _queuedNext = false;

            LogCombat("Attack 1 started");
        }

        public void TryQueueNextAttack()
        {
            if (!IsAttacking)
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
                if (TryGetActiveStep(out var step) && ComboInputElapsed <= step.ComboInputCloseTime)
                    return false;

                EndCombo();
                return false;
            }

            if (_comboIndex >= MaxComboCount)
            {
                _queuedNext = false;
                EndCombo();
                return false;
            }

            EnsureHitboxClosed();

            _queuedNext = false;
            _segmentFinishedLatch = false;
            _comboIndex++;
            _segmentTimer = 0f;
            _segmentWallClockStart = Time.time;

            LogCombat("Attack " + _comboIndex + " started");
            return true;
        }

        public void TickAttack()
        {
            if (_comboIndex == 0 || !TryGetActiveStep(out var step))
                return;

            if (_segmentFinishedLatch)
                return;

            float timerBeforeStep = _segmentTimer;
            _segmentTimer += Time.deltaTime;

            bool segmentDone = _segmentTimer >= step.AttackDuration;
            if (segmentDone)
                _segmentTimer = step.AttackDuration;

            UpdateHitboxWindow(step, segmentDone, timerBeforeStep);

            if (segmentDone)
            {
                EnsureHitboxClosed();
                _segmentFinishedLatch = true;
            }
        }

        public void ResetCombo()
        {
            EnsureHitboxClosed();
            _comboIndex = 0;
            _segmentTimer = 0f;
            _segmentWallClockStart = 0f;
            _segmentFinishedLatch = false;
            _queuedNext = false;
            _nextAttackAllowedTime = Time.time;
        }

        public void ClearAttackFinishedFlag()
        {
            EnsureHitboxClosed();
            _segmentFinishedLatch = false;
        }

        private void UpdateHitboxWindow(ComboStepData step, bool segmentDone, float timerBeforeStep)
        {
            if (_attackHitbox == null)
                return;

            if (segmentDone)
            {
                EnsureHitboxClosed();
                return;
            }

            bool overlapsHitWindow = Mathf.Max(timerBeforeStep, step.HitboxOpenTime)
                < Mathf.Min(_segmentTimer, step.HitboxCloseTime);

            if (overlapsHitWindow && !_hitboxWindowActive)
            {
                if (_attackHitbox.BeginHitWindow(BuildDamageInfo(step)))
                    _hitboxWindowActive = true;
            }
            else if (!overlapsHitWindow && _hitboxWindowActive)
            {
                _attackHitbox.EndHitWindow();
                _hitboxWindowActive = false;
            }
        }

        private DamageInfo BuildDamageInfo(ComboStepData step)
        {
            var dir = transform.forward;
            dir.y = 0f;
            if (dir.sqrMagnitude < 1e-6f)
                dir = Vector3.forward;
            dir.Normalize();

            return new DamageInfo
            {
                Source = gameObject,
                Damage = step.Damage,
                HitDirection = dir,
                ComboIndex = _comboIndex,
            };
        }

        private void EnsureHitboxClosed()
        {
            if (_attackHitbox == null || !_hitboxWindowActive)
                return;

            _attackHitbox.EndHitWindow();
            _hitboxWindowActive = false;
        }

        private void LogCombat(string message)
        {
            Debug.Log("[PlayerCombat] " + message + " | t=" + Time.time.ToString("F4") + " frame=" + Time.frameCount, this);
        }

        private bool HasValidSteps() => _comboSteps != null && _comboSteps.Length > 0;

        private bool TryGetActiveStep(out ComboStepData step)
        {
            step = default;
            if (_comboIndex < 1 || _comboIndex > MaxComboCount)
                return false;

            step = _comboSteps[_comboIndex - 1];
            return true;
        }

        private float ComboInputElapsed => Time.time - _segmentWallClockStart;

        private void EndCombo()
        {
            LogCombat("Combo ended");

            EnsureHitboxClosed();

            float delay = 0f;
            if (_comboIndex >= 1 && _comboIndex <= MaxComboCount && HasValidSteps())
                delay = _comboSteps[_comboIndex - 1].CooldownAfterAttack;

            _comboIndex = 0;
            _segmentTimer = 0f;
            _segmentWallClockStart = 0f;
            _queuedNext = false;
            _segmentFinishedLatch = false;

            _nextAttackAllowedTime = Time.time + delay;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_attackHitbox == null)
                Debug.LogWarning("[PlayerCombat] Attack Hitbox atanmadi — vurus detection calismaz.", this);

            if (_comboSteps == null)
                return;

            for (var i = 0; i < _comboSteps.Length; i++)
            {
                ref var s = ref _comboSteps[i];
                s.Damage = Mathf.Max(0, s.Damage);
                s.AttackDuration = Mathf.Max(0.0001f, s.AttackDuration);
                s.ComboInputOpenTime = Mathf.Max(0f, s.ComboInputOpenTime);
                s.ComboInputCloseTime = Mathf.Max(s.ComboInputOpenTime, s.ComboInputCloseTime);
                s.HitboxOpenTime = Mathf.Max(0f, s.HitboxOpenTime);
                s.HitboxCloseTime = Mathf.Max(s.HitboxOpenTime, s.HitboxCloseTime);
                s.CooldownAfterAttack = Mathf.Max(0f, s.CooldownAfterAttack);
                s.InputMovementMultiplier = Mathf.Max(0f, s.InputMovementMultiplier);
                s.AimMovementSpeed = Mathf.Max(0f, s.AimMovementSpeed);
                s.MovementOpenTime = Mathf.Max(0f, s.MovementOpenTime);
                s.MovementCloseTime = Mathf.Max(s.MovementOpenTime, s.MovementCloseTime);
                if (s.HitboxCloseTime > s.AttackDuration)
                    Debug.LogWarning("[PlayerCombat] Step " + (i + 1) + ": HitboxCloseTime exceeds AttackDuration.", this);
                const float minHitboxSpan = 0.001f;
                if (s.HitboxCloseTime - s.HitboxOpenTime < minHitboxSpan)
                    Debug.LogWarning(
                        "[PlayerCombat] Step " + (i + 1)
                        + ": Hitbox penceresi yok veya cok dar (HitboxCloseTime > HitboxOpenTime olmali, ornek Open=0.05 Close=0.18). Vurus acilmaz.",
                        this);
                if (s.MovementCloseTime > s.AttackDuration)
                    Debug.LogWarning("[PlayerCombat] Step " + (i + 1) + ": MovementCloseTime exceeds AttackDuration.", this);
            }
        }
#endif
    }
}
