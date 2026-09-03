using UnityEngine;

namespace PistolPanic.Core
{
    [CreateAssetMenu(menuName = "PistolPanic/PlayerConfig")]
    public sealed class PlayerConfig : ScriptableObject
    {
        [SerializeField]
        private float _maxHealth = 3f;

        [SerializeField]
        private float _invulnerabilitySeconds = 0.6f;

        public float MaxHealth => _maxHealth;

        public float InvulnerabilitySeconds => _invulnerabilitySeconds;
    }
}
