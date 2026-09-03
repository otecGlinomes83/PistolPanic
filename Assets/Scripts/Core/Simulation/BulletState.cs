using UnityEngine;

namespace PistolPanic.Core
{
    public sealed class BulletState
    {
        public int Id { get; set; }

        public Vector2 Position { get; set; }

        public Vector2 Velocity { get; set; }

        public float Radius { get; set; }

        public float Damage { get; set; }

        public bool OwnerIsPlayer { get; set; }

        public bool IsAlive { get; set; }

        public bool WasAlive { get; set; }

        public BulletDeathReason DeathReason { get; set; }
    }
}
