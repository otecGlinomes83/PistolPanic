using System.Collections.Generic;
using UnityEngine;

namespace PistolPanic.Core
{
    [CreateAssetMenu(menuName = "PistolPanic/EnemyTypeConfig")]
    public sealed class EnemyTypeConfig : ScriptableObject
    {
        [SerializeField]
        private string _displayName = "Стрелок";

        [SerializeField]
        private WeaponConfig _weaponConfig = null;

        [SerializeField]
        private Color _gunColor = new Color(0.85f, 0.35f, 0.3f, 1f);

        [SerializeField]
        private List<Color> _stageColors = new List<Color>
        {
            new Color(0.85f, 0.35f, 0.3f, 1f),
            new Color(0.75f, 0.25f, 0.2f, 1f),
            new Color(0.6f, 0.15f, 0.1f, 1f)
        };

        public string DisplayName => _displayName;

        public WeaponConfig WeaponConfig => _weaponConfig;

        public Color GunColor => _gunColor;

        public IReadOnlyList<Color> StageColors => _stageColors;
    }
}
