namespace PistolPanic.Core
{
    public sealed class AmmoSystem
    {
        public void Tick(float fixedDelta, AmmoState ammo, WeaponParams weaponParams)
        {
            TickCooldown(fixedDelta, ammo);

            if (ammo.IsFullReloading)
            {
                TickFullReload(fixedDelta, ammo, weaponParams);

                return;
            }

            TickRegen(fixedDelta, ammo, weaponParams);
        }

        private void TickCooldown(float fixedDelta, AmmoState ammo)
        {
            if (ammo.CooldownRemainingSeconds > 0f)
            {
                ammo.CooldownRemainingSeconds -= fixedDelta;
            }
        }

        private void TickFullReload(float fixedDelta, AmmoState ammo, WeaponParams weaponParams)
        {
            ammo.ReloadRemainingSeconds -= fixedDelta;

            if (ammo.ReloadRemainingSeconds > 0f)
            {
                return;
            }

            ammo.IsFullReloading = false;
            ammo.Magazine = weaponParams.MagazineSize;
            ammo.RegenTimerSeconds = 0f;
        }

        private void TickRegen(float fixedDelta, AmmoState ammo, WeaponParams weaponParams)
        {
            if (ammo.Magazine >= weaponParams.MagazineSize)
            {
                ammo.RegenTimerSeconds = 0f;

                return;
            }

            ammo.RegenTimerSeconds += fixedDelta;

            if (ammo.RegenTimerSeconds < weaponParams.RegenDelaySeconds)
            {
                return;
            }

            ammo.RegenTimerSeconds -= weaponParams.RegenDelaySeconds;
            ammo.Magazine += 1;
        }

        public bool CanFire(AmmoState ammo)
        {
            if (ammo.IsFullReloading)
            {
                return false;
            }

            if (ammo.Magazine <= 0)
            {
                return false;
            }

            if (ammo.CooldownRemainingSeconds > 0f)
            {
                return false;
            }

            return true;
        }

        public void ConsumeShot(AmmoState ammo, WeaponParams weaponParams)
        {
            ammo.Magazine -= 1;
            ammo.CooldownRemainingSeconds = weaponParams.CooldownSeconds;

            if (ammo.Magazine <= 0)
            {
                ammo.IsFullReloading = true;
                ammo.ReloadRemainingSeconds = weaponParams.FullReloadSeconds;
            }
        }
    }
}
