namespace PistolPanic.Core
{
    public readonly struct WeaponParams
    {
        public WeaponParams(WeaponConfig weaponConfig)
        {
            Damage = weaponConfig.Damage;
            CooldownSeconds = weaponConfig.CooldownSeconds;
            MagazineSize = weaponConfig.MagazineSize;
            RegenDelaySeconds = weaponConfig.RegenDelaySeconds;
            FullReloadSeconds = weaponConfig.FullReloadSeconds;
            BulletSpeed = weaponConfig.BulletSpeed;
            BulletRadius = weaponConfig.BulletRadius;
            RecoilImpulse = weaponConfig.RecoilImpulse;
            RecoilTorqueMinDegrees = weaponConfig.RecoilTorqueMinDegrees;
            RecoilTorqueMaxDegrees = weaponConfig.RecoilTorqueMaxDegrees;
            BulletSpawnOffsetUnits = weaponConfig.BulletSpawnOffsetUnits;
        }

        public float Damage { get; }

        public float CooldownSeconds { get; }

        public int MagazineSize { get; }

        public float RegenDelaySeconds { get; }

        public float FullReloadSeconds { get; }

        public float BulletSpeed { get; }

        public float BulletRadius { get; }

        public float RecoilImpulse { get; }

        public float RecoilTorqueMinDegrees { get; }

        public float RecoilTorqueMaxDegrees { get; }

        public float BulletSpawnOffsetUnits { get; }
    }
}
