using System;
using UnityEngine;

namespace Woi.Ninja.Player.Services
{
    public enum AttackMovementMode
    {
        None,
        InputOnly,
        AimOnly,
        InputOrAimFallback,
    }

    /// <summary>
    /// Timing and damage for one combo chain step (1-based combo index maps to element index + 1).
    /// </summary>
    [Serializable]
    public struct ComboStepData
    {
        [Min(0)] public int Damage;

        [Min(0.0001f)] public float AttackDuration;

        [Min(0f)] public float ComboInputOpenTime;

        [Min(0f)] public float ComboInputCloseTime;

        [Min(0f)] public float HitboxOpenTime;

        [Min(0f)] public float HitboxCloseTime;

        [Min(0f)] public float CooldownAfterAttack;

        public AttackMovementMode MovementMode;

        [Min(0f)] public float InputMovementMultiplier;

        [Min(0f)] public float AimMovementSpeed;

        [Min(0f)] public float MovementOpenTime;

        [Min(0f)] public float MovementCloseTime;
    }
}
