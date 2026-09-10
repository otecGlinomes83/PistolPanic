namespace PistolPanic.Core
{
    public readonly struct DamageRequest
    {
        public DamageRequest(bool isTargetPlayer, float damage)
        {
            IsTargetPlayer = isTargetPlayer;
            Damage = damage;
        }

        public bool IsTargetPlayer { get; }

        public float Damage { get; }
    }
}
