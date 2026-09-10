using System.Collections.Generic;
using UnityEngine;

namespace PistolPanic.Core
{
    [CreateAssetMenu(menuName = "PistolPanic/EnemyCatalog")]
    public sealed class EnemyCatalog : ScriptableObject
    {
        [SerializeField]
        private List<EnemyTypeConfig> _types = new List<EnemyTypeConfig> { null, null, null };

        public IReadOnlyList<EnemyTypeConfig> Types => _types;
    }
}
