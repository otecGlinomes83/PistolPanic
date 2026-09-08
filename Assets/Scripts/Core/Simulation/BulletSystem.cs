using System.Collections.Generic;
using UnityEngine;

namespace PistolPanic.Core
{
    public sealed class BulletSystem
    {
        private readonly CollisionMath _collisionMath;

        private readonly List<DamageRequest> _damageRequests = new List<DamageRequest>(16);

        public BulletSystem(CollisionMath collisionMath)
        {
            _collisionMath = collisionMath;
        }

        public IReadOnlyList<DamageRequest> DamageRequests => _damageRequests;

        public void Tick(float fixedDelta, ArenaState arena, GunState playerGun, GunState enemyGun, List<BulletState> bullets)
        {
            _damageRequests.Clear();

            for (int i = 0; i < bullets.Count; i++)
            {
                BulletState bullet = bullets[i];

                if (bullet.IsAlive == false)
                {
                    continue;
                }

                MoveAndCollide(fixedDelta, arena, playerGun, enemyGun, bullet);
            }
        }

        private void MoveAndCollide(float fixedDelta, ArenaState arena, GunState playerGun, GunState enemyGun, BulletState bullet)
        {
            bullet.PreviousPosition = bullet.Position;
            bullet.Position += bullet.Velocity * fixedDelta;

            if (IsOutsideArena(arena, bullet))
            {
                bullet.IsAlive = false;
                bullet.DeathReason = BulletDeathReason.Wall;

                return;
            }

            if (HitsObstacle(arena, bullet.PreviousPosition, bullet.Position, bullet.Radius))
            {
                bullet.IsAlive = false;
                bullet.DeathReason = BulletDeathReason.Obstacle;

                return;
            }

            TryHitGun(bullet.PreviousPosition, bullet, playerGun, true);
            TryHitGun(bullet.PreviousPosition, bullet, enemyGun, false);
        }

        private bool IsOutsideArena(ArenaState arena, BulletState bullet)
        {
            if (bullet.Position.x < arena.Min.x - bullet.Radius)
            {
                return true;
            }

            if (bullet.Position.x > arena.Max.x + bullet.Radius)
            {
                return true;
            }

            if (bullet.Position.y < arena.Min.y - bullet.Radius)
            {
                return true;
            }

            if (bullet.Position.y > arena.Max.y + bullet.Radius)
            {
                return true;
            }

            return false;
        }

        private bool HitsObstacle(ArenaState arena, Vector2 segmentStart, Vector2 segmentEnd, float bulletRadius)
        {
            ObstacleData[] obstacles = arena.Obstacles;

            for (int i = 0; i < obstacles.Length; i++)
            {
                float combinedRadius = obstacles[i].Radius + bulletRadius;

                if (_collisionMath.SegmentIntersectsCircle(segmentStart, segmentEnd, obstacles[i].Center, combinedRadius))
                {
                    return true;
                }
            }

            return false;
        }

        private void TryHitGun(Vector2 segmentStart, BulletState bullet, GunState targetGun, bool targetIsPlayer)
        {
            if (bullet.IsAlive == false)
            {
                return;
            }

            if (bullet.OwnerIsPlayer == targetIsPlayer)
            {
                return;
            }

            float combinedRadius = targetGun.Radius + bullet.Radius;

            if (_collisionMath.SegmentIntersectsCircle(segmentStart, bullet.Position, targetGun.Position, combinedRadius) == false)
            {
                return;
            }

            bullet.IsAlive = false;
            bullet.DeathReason = BulletDeathReason.GunHit;
            _damageRequests.Add(new DamageRequest(targetIsPlayer, bullet.Damage));
        }
    }
}
