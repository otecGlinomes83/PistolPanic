using System.Collections.Generic;
using UnityEngine;

namespace PistolPanic.Core
{
    [CreateAssetMenu(menuName = "PistolPanic/WeaponCatalog")]
    public sealed class WeaponCatalog : ScriptableObject
    {
        [SerializeField]
        private List<WeaponConfig> _weapons = new List<WeaponConfig>();

        [SerializeField]
        private int _defaultIndex = 0;

        public IReadOnlyList<WeaponConfig> Weapons => _weapons;

        public int DefaultIndex => _defaultIndex;
    }
}
