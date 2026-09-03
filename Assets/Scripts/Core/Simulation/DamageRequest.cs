using UnityEngine;

namespace PistolPanic.Core
{
    public readonly struct DamageRequest
    {
        public DamageRequest(bool targetIsPlayer, float damage)
        {
            TargetIsPlayer = targetIsPlayer;
            Damage = damage;
        }

        public bool TargetIsPlayer { get; }

        public float Damage { get; }
    }
}
