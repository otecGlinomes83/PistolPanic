using UnityEngine;

namespace PistolPanic.Core
{
    public sealed class GunState
    {
        public Vector2 Position { get; set; }

        public Vector2 Velocity { get; set; }

        public float RotationDegrees { get; set; }

        public float AngularVelocityDegrees { get; set; }

        public float Radius { get; set; }

        public float Health { get; set; }

        public float InvulnerableUntilSeconds { get; set; }
    }
}
