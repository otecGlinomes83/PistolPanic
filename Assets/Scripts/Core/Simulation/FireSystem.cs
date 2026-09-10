using UnityEngine;

namespace PistolPanic.Core
{
    public sealed class FireSystem
    {
        private readonly AmmoSystem _ammoSystem;

        private readonly WeaponParams _weaponParams;

        private readonly BulletSystem _bulletSystem;

        private readonly MovementSystem _movementSystem;

        private readonly AmmoState _playerAmmo;

        public FireSystem(AmmoSystem ammoSystem, WeaponParams weaponParams, BulletSystem bulletSystem, MovementSystem movementSystem)
        {
            _ammoSystem = ammoSystem;
            _weaponParams = weaponParams;
            _bulletSystem = bulletSystem;
            _movementSystem = movementSystem;

            _playerAmmo = new AmmoState();
            _playerAmmo.Magazine = _weaponParams.MagazineSize;
            _playerAmmo.CooldownRemainingSeconds = 0f;
            _playerAmmo.RegenTimerSeconds = 0f;
            _playerAmmo.ReloadRemainingSeconds = 0f;
            _playerAmmo.IsFullReloading = false;
        }

        public AmmoState PlayerAmmo => _playerAmmo;

        public void Tick(float fixedDelta)
        {
            _ammoSystem.Tick(fixedDelta, _playerAmmo, _weaponParams);
        }

        public bool TryFire(GunState ownerGun, bool ownerIsPlayer)
        {
            if (ownerIsPlayer)
            {
                if (_ammoSystem.CanFire(_playerAmmo) == false)
                {
                    return false;
                }

                _ammoSystem.ConsumeShot(_playerAmmo, _weaponParams);
            }

            float fireAngleRadians = ownerGun.RotationDegrees * Mathf.Deg2Rad;
            Vector2 fireDirection = new Vector2(Mathf.Cos(fireAngleRadians), Mathf.Sin(fireAngleRadians));

            _bulletSystem.Spawn(ownerGun, fireDirection, ownerIsPlayer);
            _movementSystem.ApplyRecoil(ownerGun, fireDirection);

            return true;
        }
    }
}
