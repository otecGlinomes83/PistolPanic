using UnityEngine;

namespace PistolPanic.Core
{
    [CreateAssetMenu(menuName = "PistolPanic/WeaponConfig")]
    public sealed class WeaponConfig : ScriptableObject
    {
        [SerializeField]
        private float _damage = 1f;

        [SerializeField]
        private float _cooldownSeconds = 0.8f;

        [SerializeField]
        private int _magazineSize = 6;

        [SerializeField]
        private float _regenDelaySeconds = 2.5f;

        [SerializeField]
        private float _fullReloadSeconds = 3f;

        [SerializeField]
        private float _bulletSpeed = 13f;

        [SerializeField]
        private float _bulletRadius = 0.15f;

        [SerializeField]
        private float _recoilImpulse = 2f;

        [SerializeField]
        private float _recoilTorqueMinDegrees = 30f;

        [SerializeField]
        private float _recoilTorqueMaxDegrees = 90f;

        [SerializeField]
        private float _bulletSpawnOffsetUnits = 0.05f;

        [SerializeField]
        private string _displayName = "Pistol";

        [SerializeField]
        private FirePattern _firePattern = FirePattern.Single;

        [SerializeField]
        private int _burstCount = 3;

        [SerializeField]
        private float _burstIntervalSeconds = 0.12f;

        [SerializeField]
        private int _pelletCount = 6;

        [SerializeField]
        private float _spreadAngleDegrees = 30f;

        [SerializeField]
        private float _pelletDamageMultiplier = 0.34f;

        public float Damage => _damage;

        public float CooldownSeconds => _cooldownSeconds;

        public int MagazineSize => _magazineSize;

        public float RegenDelaySeconds => _regenDelaySeconds;

        public float FullReloadSeconds => _fullReloadSeconds;

        public float BulletSpeed => _bulletSpeed;

        public float BulletRadius => _bulletRadius;

        public float RecoilImpulse => _recoilImpulse;

        public float RecoilTorqueMinDegrees => _recoilTorqueMinDegrees;

        public float RecoilTorqueMaxDegrees => _recoilTorqueMaxDegrees;

        public float BulletSpawnOffsetUnits => _bulletSpawnOffsetUnits;

        public string DisplayName => _displayName;

        public FirePattern FirePattern => _firePattern;

        public int BurstCount => _burstCount;

        public float BurstIntervalSeconds => _burstIntervalSeconds;

        public int PelletCount => _pelletCount;

        public float SpreadAngleDegrees => _spreadAngleDegrees;

        public float PelletDamageMultiplier => _pelletDamageMultiplier;
    }
}
