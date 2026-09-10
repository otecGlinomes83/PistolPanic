using UnityEngine;

namespace PistolPanic.Core
{
    public readonly struct BulletMovedSnapshot
    {
        public BulletMovedSnapshot(int bulletId, Vector2 position)
        {
            BulletId = bulletId;
            Position = position;
        }

        public int BulletId { get; }

        public Vector2 Position { get; }
    }
}
