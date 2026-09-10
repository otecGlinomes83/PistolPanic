using UnityEngine;

namespace PistolPanic.Core
{
    [CreateAssetMenu(menuName = "PistolPanic/SimulationConfig")]
    public sealed class SimulationConfig : ScriptableObject
    {
        [SerializeField]
        private int _ticksPerSecond = 60;

        [SerializeField]
        private int _maxStepsPerFrame = 3;

        [SerializeField]
        private int _targetFrameRate = 60;

        public int TicksPerSecond => _ticksPerSecond;

        public int MaxStepsPerFrame => _maxStepsPerFrame;

        public int TargetFrameRate => _targetFrameRate;
    }
}
