using UnityEngine;

namespace PistolPanic.Core
{
    [CreateAssetMenu(menuName = "PistolPanic/PhysicsConfig")]
    public sealed class PhysicsConfig : ScriptableObject
    {
        [SerializeField]
        private float _linearDampingPerSecond = 0.4f;

        [SerializeField]
        private float _angularDampingPerSecond = 1.2f;

        [SerializeField]
        private float _minLinearSpeedUnits = 0.05f;

        [SerializeField]
        private float _minAngularSpeedDegrees = 5f;

        public float LinearDampingPerSecond => _linearDampingPerSecond;

        public float AngularDampingPerSecond => _angularDampingPerSecond;

        public float MinLinearSpeedUnits => _minLinearSpeedUnits;

        public float MinAngularSpeedDegrees => _minAngularSpeedDegrees;
    }
}
