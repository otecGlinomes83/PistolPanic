using System;
using PistolPanic.Meta;
using UnityEngine;

namespace PistolPanic.Core
{
    public sealed class FireSystem
    {
        private readonly AmmoSystem _ammoSystem;

        private readonly BulletSystem _bulletSystem;

        private readonly MovementSystem _movementSystem;

        private readonly WeaponParams _playerWeaponParams;

        private readonly WeaponParams _enemyWeaponParams;

        private readonly BurstQueueState _playerBurstQueue;

        private readonly BurstQueueState _enemyBurstQueue;

        private readonly AmmoState _playerAmmo;

        private GunState _playerBurstGun;

        private GunState _enemyBurstGun;

        private bool _hasPublishedAmmoState;

        private int _lastPublishedMagazine;

        private float _lastPublishedRegenTimer;

        private float _lastPublishedReloadRemaining;

        private bool _lastPublishedIsReloading;

        public event Action<AmmoSnapshot> PlayerAmmoChanged;

        public event Action<ShotFiredSnapshot> ShotFired;

        public FireSystem(AmmoSystem ammoSystem, BulletSystem bulletSystem, MovementSystem movementSystem, PlayerWeaponProvider playerWeaponProvider, EnemyTypeProvider enemyTypeProvider)
        {
            _ammoSystem = ammoSystem;
            _bulletSystem = bulletSystem;
            _movementSystem = movementSystem;

            _playerWeaponParams = new WeaponParams(playerWeaponProvider.CurrentWeaponConfig);
            _enemyWeaponParams = new WeaponParams(enemyTypeProvider.Current.WeaponConfig);

            _playerBurstQueue = new BurstQueueState();
            _enemyBurstQueue = new BurstQueueState();

            _playerAmmo = new AmmoState();
            _playerAmmo.Magazine = _playerWeaponParams.MagazineSize;
            _playerAmmo.CooldownRemainingSeconds = 0f;
            _playerAmmo.RegenTimerSeconds = 0f;
            _playerAmmo.ReloadRemainingSeconds = 0f;
            _playerAmmo.IsFullReloading = false;

            _lastPublishedMagazine = _playerAmmo.Magazine;
            _lastPublishedRegenTimer = _playerAmmo.RegenTimerSeconds;
            _lastPublishedReloadRemaining = _playerAmmo.ReloadRemainingSeconds;
            _lastPublishedIsReloading = _playerAmmo.IsFullReloading;
        }

        public AmmoState PlayerAmmo => _playerAmmo;

        public void Tick(float fixedDelta)
        {
            _ammoSystem.Tick(fixedDelta, _playerAmmo, _playerWeaponParams);

            TickBurst(fixedDelta, true);
            TickBurst(fixedDelta, false);

            PublishPlayerAmmoChanged();
        }

        public bool TryFire(GunState ownerGun, bool ownerIsPlayer)
        {
            WeaponParams weaponParams;
            BurstQueueState burstQueue;

            if (ownerIsPlayer)
            {
                weaponParams = _playerWeaponParams;
                burstQueue = _playerBurstQueue;
            }
            else
            {
                weaponParams = _enemyWeaponParams;
                burstQueue = _enemyBurstQueue;
            }

            if (burstQueue.IsActive)
            {
                return false;
            }

            if (ownerIsPlayer && _ammoSystem.CanFire(_playerAmmo) == false)
            {
                return false;
            }

            switch (weaponParams.FirePattern)
            {
                case FirePattern.Single:
                    FireSingleShot(ownerGun, weaponParams, ownerIsPlayer);

                    break;

                case FirePattern.Burst:
                    FireSingleShot(ownerGun, weaponParams, ownerIsPlayer);
                    StartBurstQueue(burstQueue, ownerGun, ownerIsPlayer, weaponParams);

                    break;

                case FirePattern.Shotgun:
                    if (ownerIsPlayer)
                    {
                        _ammoSystem.ConsumeShot(_playerAmmo, _playerWeaponParams);
                    }

                    FireShotgunVolley(ownerGun, weaponParams, ownerIsPlayer);

                    break;
            }

            return true;
        }

        private void FireSingleShot(GunState ownerGun, WeaponParams weaponParams, bool ownerIsPlayer)
        {
            if (ownerIsPlayer)
            {
                _ammoSystem.ConsumeShot(_playerAmmo, _playerWeaponParams);
            }

            FireShot(ownerGun, weaponParams, ownerIsPlayer);
        }

        private void StartBurstQueue(BurstQueueState burstQueue, GunState ownerGun, bool ownerIsPlayer, WeaponParams weaponParams)
        {
            burstQueue.ShotsRemaining = weaponParams.BurstCount - 1;

            if (burstQueue.ShotsRemaining <= 0)
            {
                return;
            }

            burstQueue.IsActive = true;
            burstQueue.TimerSeconds = weaponParams.BurstIntervalSeconds;

            if (ownerIsPlayer)
            {
                _playerBurstGun = ownerGun;
            }
            else
            {
                _enemyBurstGun = ownerGun;
            }
        }

        private void FireShot(GunState ownerGun, WeaponParams weaponParams, bool ownerIsPlayer)
        {
            float fireAngleRadians = ownerGun.RotationDegrees * Mathf.Deg2Rad;
            Vector2 fireDirection = new Vector2(Mathf.Cos(fireAngleRadians), Mathf.Sin(fireAngleRadians));
            float muzzleOffset = ownerGun.Radius + weaponParams.BulletRadius + weaponParams.BulletSpawnOffsetUnits;
            Vector2 muzzlePosition = ownerGun.Position + fireDirection * muzzleOffset;

            _bulletSystem.Spawn(muzzlePosition, fireDirection * weaponParams.BulletSpeed, weaponParams.BulletRadius, weaponParams.Damage, ownerIsPlayer);
            _movementSystem.ApplyRecoil(ownerGun, fireDirection, weaponParams);

            ShotFired?.Invoke(new ShotFiredSnapshot(ownerGun.Position, ownerIsPlayer, weaponParams.FirePattern));
        }

        private void FireShotgunVolley(GunState ownerGun, WeaponParams weaponParams, bool ownerIsPlayer)
        {
            if (weaponParams.PelletCount <= 0)
            {
                return;
            }

            float rotationDegrees = ownerGun.RotationDegrees;
            float centerAngleRadians = rotationDegrees * Mathf.Deg2Rad;
            Vector2 centerDirection = new Vector2(Mathf.Cos(centerAngleRadians), Mathf.Sin(centerAngleRadians));
            float muzzleOffset = ownerGun.Radius + weaponParams.BulletRadius + weaponParams.BulletSpawnOffsetUnits;
            float pelletDamage = weaponParams.Damage * weaponParams.PelletDamageMultiplier;

            for (int i = 0; i < weaponParams.PelletCount; i++)
            {
                float pelletAngleDegrees = rotationDegrees;

                if (weaponParams.PelletCount > 1)
                {
                    float angleStep = weaponParams.SpreadAngleDegrees / (weaponParams.PelletCount - 1);
                    float startAngleDegrees = rotationDegrees - weaponParams.SpreadAngleDegrees * 0.5f;

                    pelletAngleDegrees = startAngleDegrees + i * angleStep;
                }

                float pelletAngleRadians = pelletAngleDegrees * Mathf.Deg2Rad;
                Vector2 pelletDirection = new Vector2(Mathf.Cos(pelletAngleRadians), Mathf.Sin(pelletAngleRadians));
                Vector2 muzzlePosition = ownerGun.Position + pelletDirection * muzzleOffset;

                _bulletSystem.Spawn(muzzlePosition, pelletDirection * weaponParams.BulletSpeed, weaponParams.BulletRadius, pelletDamage, ownerIsPlayer);
            }

            _movementSystem.ApplyRecoil(ownerGun, centerDirection, weaponParams);

            ShotFired?.Invoke(new ShotFiredSnapshot(ownerGun.Position, ownerIsPlayer, weaponParams.FirePattern));
        }

        private void TickBurst(float fixedDelta, bool ownerIsPlayer)
        {
            BurstQueueState burstQueue;
            WeaponParams weaponParams;
            GunState burstGun;

            if (ownerIsPlayer)
            {
                burstQueue = _playerBurstQueue;
                weaponParams = _playerWeaponParams;
                burstGun = _playerBurstGun;
            }
            else
            {
                burstQueue = _enemyBurstQueue;
                weaponParams = _enemyWeaponParams;
                burstGun = _enemyBurstGun;
            }

            if (burstQueue.IsActive == false)
            {
                return;
            }

            if (ownerIsPlayer && _playerAmmo.IsFullReloading)
            {
                burstQueue.IsActive = false;

                return;
            }

            burstQueue.TimerSeconds -= fixedDelta;

            if (burstQueue.TimerSeconds > 0f)
            {
                return;
            }

            burstQueue.TimerSeconds = weaponParams.BurstIntervalSeconds;

            if (ownerIsPlayer)
            {
                if (_playerAmmo.Magazine > 0)
                {
                    _ammoSystem.ConsumeShot(_playerAmmo, _playerWeaponParams);
                    _playerAmmo.CooldownRemainingSeconds = weaponParams.BurstIntervalSeconds;
                    FireShot(burstGun, weaponParams, ownerIsPlayer);
                    burstQueue.ShotsRemaining -= 1;
                }
                else
                {
                    burstQueue.IsActive = false;

                    return;
                }
            }
            else
            {
                FireShot(burstGun, weaponParams, ownerIsPlayer);
                burstQueue.ShotsRemaining -= 1;
            }

            if (burstQueue.ShotsRemaining <= 0)
            {
                burstQueue.IsActive = false;
            }
        }

        private void PublishPlayerAmmoChanged()
        {
            bool hasStateChanged = _hasPublishedAmmoState == false
                || _playerAmmo.Magazine != _lastPublishedMagazine
                || _playerAmmo.RegenTimerSeconds != _lastPublishedRegenTimer
                || _playerAmmo.ReloadRemainingSeconds != _lastPublishedReloadRemaining
                || _playerAmmo.IsFullReloading != _lastPublishedIsReloading;

            if (hasStateChanged == false)
            {
                return;
            }

            _hasPublishedAmmoState = true;
            _lastPublishedMagazine = _playerAmmo.Magazine;
            _lastPublishedRegenTimer = _playerAmmo.RegenTimerSeconds;
            _lastPublishedReloadRemaining = _playerAmmo.ReloadRemainingSeconds;
            _lastPublishedIsReloading = _playerAmmo.IsFullReloading;

            float regenProgress01 = Mathf.Clamp01(_playerAmmo.RegenTimerSeconds / Mathf.Max(_playerWeaponParams.RegenDelaySeconds, 0.0001f));
            float reloadProgress01 = Mathf.Clamp01(1f - _playerAmmo.ReloadRemainingSeconds / Mathf.Max(_playerWeaponParams.FullReloadSeconds, 0.0001f));
            AmmoSnapshot ammoSnapshot = new AmmoSnapshot(_playerAmmo.Magazine, _playerWeaponParams.MagazineSize, regenProgress01, _playerAmmo.IsFullReloading, reloadProgress01);

            PlayerAmmoChanged?.Invoke(ammoSnapshot);
        }
    }
}
