using UnityEngine;

namespace PistolPanic.Core
{
    public readonly struct BulletSpawnedSnapshot
    {
        public BulletSpawnedSnapshot(int bulletId, Vector2 position, float diameter)
        {
            BulletId = bulletId;
            Position = position;
            Diameter = diameter;
        }

        public int BulletId { get; }

        public Vector2 Position { get; }

        public float Diameter { get; }
    }
}
