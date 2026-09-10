using UnityEngine;

namespace PistolPanic.Core
{
    public readonly struct BulletSpawnedSnapshot
    {
        public BulletSpawnedSnapshot(int bulletId, Vector2 position, float diameter, bool isPlayerOwned)
        {
            BulletId = bulletId;
            Position = position;
            Diameter = diameter;
            IsPlayerOwned = isPlayerOwned;
        }

        public int BulletId { get; }

        public Vector2 Position { get; }

        public float Diameter { get; }

        public bool IsPlayerOwned { get; }
    }
}
