using System;
using UnityEngine;

namespace Woi.Ninja.Player.Services
{
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
    }
}
