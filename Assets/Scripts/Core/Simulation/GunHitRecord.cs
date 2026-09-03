namespace PistolPanic.Core
{
    public readonly struct GunHitRecord
    {
        public GunHitRecord(bool targetIsPlayer, float damage, float healthAfter)
        {
            TargetIsPlayer = targetIsPlayer;
            Damage = damage;
            HealthAfter = healthAfter;
        }

        public bool TargetIsPlayer { get; }

        public float Damage { get; }

        public float HealthAfter { get; }
    }
}
