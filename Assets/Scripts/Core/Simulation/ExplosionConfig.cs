using UnityEngine;

namespace PistolPanic.Core
{
    [CreateAssetMenu(menuName = "PistolPanic/ExplosionConfig")]
    public sealed class ExplosionConfig : ScriptableObject
    {
        [SerializeField]
        private float _radiusUnits = 0.85f;

        [SerializeField]
        private float _damage = 1f;

        public float RadiusUnits => _radiusUnits;

        public float Damage => _damage;
    }
}
