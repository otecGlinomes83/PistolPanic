using System;

namespace PistolPanic.Meta
{
    public sealed class WeaponLoadoutService
    {
        private int _equippedIndex = -1;

        public event Action<int> EquippedChanged;

        public bool HasSelection => _equippedIndex >= 0;

        public int EquippedIndex => _equippedIndex;

        public void Equip(int weaponIndex)
        {
            _equippedIndex = weaponIndex;

            EquippedChanged?.Invoke(weaponIndex);
        }
    }
}
