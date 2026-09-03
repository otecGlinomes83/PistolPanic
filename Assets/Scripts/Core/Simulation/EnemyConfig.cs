using System.Collections.Generic;
using UnityEngine;

namespace PistolPanic.Core
{
    [CreateAssetMenu(menuName = "PistolPanic/EnemyConfig")]
    public sealed class EnemyConfig : ScriptableObject
    {
        [SerializeField]
        private List<float> _stageHealths = new List<float> { 5f, 4f, 3f };

        [SerializeField]
        private List<Color> _stageColors = new List<Color>
        {
            new Color(0.85f, 0.35f, 0.3f, 1f),
            new Color(0.75f, 0.25f, 0.2f, 1f),
            new Color(0.6f, 0.15f, 0.1f, 1f)
        };

        [SerializeField]
        private float _fireCooldownMinSeconds = 1.6f;

        [SerializeField]
        private float _fireCooldownMaxSeconds = 2.6f;

        [SerializeField]
        private float _visionConeDegrees = 100f;

        [SerializeField]
        private float _visionCheckIntervalSeconds = 0.1f;

        public IReadOnlyList<float> StageHealths => _stageHealths;

        public IReadOnlyList<Color> StageColors => _stageColors;

        public float FireCooldownMinSeconds => _fireCooldownMinSeconds;

        public float FireCooldownMaxSeconds => _fireCooldownMaxSeconds;

        public float VisionConeDegrees => _visionConeDegrees;

        public float VisionCheckIntervalSeconds => _visionCheckIntervalSeconds;
    }
}
