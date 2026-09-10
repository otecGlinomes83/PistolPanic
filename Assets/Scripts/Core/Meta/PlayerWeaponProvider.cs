using PistolPanic.Core;

namespace PistolPanic.Meta
{
    public sealed class PlayerWeaponProvider
    {
        private readonly WeaponConfig _currentWeaponConfig;

        public PlayerWeaponProvider(WeaponCatalog weaponCatalog, WeaponLoadoutService weaponLoadoutService, WeaponConfig fallbackWeaponConfig)
        {
            _currentWeaponConfig = ResolveWeaponConfig(weaponCatalog, weaponLoadoutService, fallbackWeaponConfig);
        }

        public WeaponConfig CurrentWeaponConfig => _currentWeaponConfig;

        private WeaponConfig ResolveWeaponConfig(WeaponCatalog weaponCatalog, WeaponLoadoutService weaponLoadoutService, WeaponConfig fallbackWeaponConfig)
        {
            if (weaponLoadoutService != null && weaponLoadoutService.HasSelection)
            {
                return ResolveByIndex(weaponCatalog, weaponLoadoutService.EquippedIndex, fallbackWeaponConfig);
            }

            if (weaponCatalog != null && weaponCatalog.Weapons.Count > 0)
            {
                return ResolveByIndex(weaponCatalog, weaponCatalog.DefaultIndex, fallbackWeaponConfig);
            }

            return fallbackWeaponConfig;
        }

        private WeaponConfig ResolveByIndex(WeaponCatalog weaponCatalog, int weaponIndex, WeaponConfig fallbackWeaponConfig)
        {
            if (weaponCatalog == null)
            {
                return fallbackWeaponConfig;
            }

            if (weaponIndex < 0 || weaponIndex >= weaponCatalog.Weapons.Count)
            {
                return fallbackWeaponConfig;
            }

            WeaponConfig resolvedWeapon = weaponCatalog.Weapons[weaponIndex];

            if (resolvedWeapon == null)
            {
                return fallbackWeaponConfig;
            }

            return resolvedWeapon;
        }
    }
}
