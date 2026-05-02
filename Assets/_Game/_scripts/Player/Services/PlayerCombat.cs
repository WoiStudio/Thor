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
            },
        };

        private int _comboIndex;

        private float _segmentTimer;

        private bool _segmentFinishedLatch;

        private bool _queuedNext;

        private float _nextAttackAllowedTime;

        private bool _hitboxWindowActive;

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

            if (_attackHitbox == null)
                Debug.LogWarning("[PlayerCombat] Attack Hitbox referansi yok — Hasar/tetik yok. Inspector'dan ata.", this);

            EnsureHitboxClosed();

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

            EnsureHitboxClosed();

            _queuedNext = false;
            _segmentFinishedLatch = false;
            _comboIndex++;
            _segmentTimer = 0f;

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
            _segmentFinishedLatch = false;
            _queuedNext = false;
            _nextAttackAllowedTime = Time.time;
        }

        public void ClearAttackFinishedFlag()
        {
            EnsureHitboxClosed();
            _segmentFinishedLatch = false;
        }

        /// <summary>
        /// Uses swept overlap between [timerBeforeStep, current timer] and [HitboxOpenTime, HitboxCloseTime).
        /// Without this, a large deltaTime can skip the entire window and never open the hitbox.
        /// </summary>
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

        private void EndCombo()
        {
            LogCombat("Combo ended");

            EnsureHitboxClosed();

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
                if (s.ComboInputCloseTime > s.AttackDuration)
                    Debug.LogWarning("[PlayerCombat] Step " + (i + 1) + ": ComboInputCloseTime exceeds AttackDuration.", this);
                if (s.HitboxCloseTime > s.AttackDuration)
                    Debug.LogWarning("[PlayerCombat] Step " + (i + 1) + ": HitboxCloseTime exceeds AttackDuration.", this);
                const float minHitboxSpan = 0.001f;
                if (s.HitboxCloseTime - s.HitboxOpenTime < minHitboxSpan)
                    Debug.LogWarning(
                        "[PlayerCombat] Step " + (i + 1)
                        + ": Hitbox penceresi yok veya cok dar (HitboxCloseTime > HitboxOpenTime olmali, ornek Open=0.05 Close=0.18). Vurus acilmaz.",
                        this);
            }
        }
#endif
    }
}
