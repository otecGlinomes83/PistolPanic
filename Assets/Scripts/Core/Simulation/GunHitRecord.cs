namespace PistolPanic.Core
{
    public readonly struct GunHitRecord
    {
        public GunHitRecord(bool isTargetPlayer, float damage, float healthAfter)
        {
            IsTargetPlayer = isTargetPlayer;
            Damage = damage;
            HealthAfter = healthAfter;
        }

        public bool IsTargetPlayer { get; }

        public float Damage { get; }

        public float HealthAfter { get; }
    }
}
