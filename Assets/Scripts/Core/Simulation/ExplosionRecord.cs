using UnityEngine;

namespace PistolPanic.Core
{
    public readonly struct ExplosionRecord
    {
        public ExplosionRecord(Vector2 position, float radius)
        {
            Position = position;
            Radius = radius;
        }

        public Vector2 Position { get; }

        public float Radius { get; }
    }
}
