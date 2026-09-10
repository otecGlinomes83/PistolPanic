using UnityEngine;

namespace PistolPanic.Core
{
    public interface IBulletPool
    {
        void Spawn(int bulletId, Vector2 position, float diameter, Color color);

        void Move(int bulletId, Vector2 position);

        void Despawn(int bulletId);
    }
}
