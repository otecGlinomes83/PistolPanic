using UnityEngine;

namespace PistolPanic.Core
{
    public readonly struct GunSnapshot
    {
        public GunSnapshot(Vector2 position, float rotationDegrees)
        {
            Position = position;
            RotationDegrees = rotationDegrees;
        }

        public Vector2 Position { get; }

        public float RotationDegrees { get; }
    }
}
