using System.Collections.Generic;
using UnityEngine;

namespace PistolPanic.Core
{
    public sealed class BulletSystem
    {
        private const int BulletListCapacity = 64;

        private readonly CollisionMath _collisionMath;

        private readonly List<BulletState> _bullets = new List<BulletState>(BulletListCapacity);

        private readonly List<bool> _wasAliveThisTick = new List<bool>(BulletListCapacity);

        private readonly List<BulletSpawnedSnapshot> _spawnedThisTick = new List<BulletSpawnedSnapshot>(BulletListCapacity);

        private readonly List<BulletMovedSnapshot> _movedThisTick = new List<BulletMovedSnapshot>(BulletListCapacity);

        private readonly List<int> _removedThisTick = new List<int>(BulletListCapacity);

        private readonly List<DamageRequest> _damageRequests = new List<DamageRequest>(16);

        private int _nextBulletId = 1;

        public BulletSystem(CollisionMath collisionMath)
        {
            _collisionMath = collisionMath;

            for (int i = 0; i < BulletListCapacity; i++)
            {
                _bullets.Add(new BulletState());
                _wasAliveThisTick.Add(false);
            }
        }

        public IReadOnlyList<BulletState> Bullets => _bullets;

        public IReadOnlyList<BulletSpawnedSnapshot> SpawnedThisTick => _spawnedThisTick;

        public IReadOnlyList<BulletMovedSnapshot> MovedThisTick => _movedThisTick;

        public IReadOnlyList<int> RemovedThisTick => _removedThisTick;

        public IReadOnlyList<DamageRequest> DamageRequests => _damageRequests;

        public void Spawn(Vector2 position, Vector2 velocity, float radius, float damage, bool isPlayerOwned)
        {
            BulletState bullet = FindFreeBullet();

            bullet.Id = _nextBulletId;
            _nextBulletId += 1;
            bullet.Position = position;
            bullet.PreviousPosition = position;
            bullet.Velocity = velocity;
            bullet.Radius = radius;
            bullet.Damage = damage;
            bullet.IsPlayerOwned = isPlayerOwned;
            bullet.IsAlive = true;
            bullet.DeathReason = BulletDeathReason.None;
        }

        public void Tick(float fixedDelta, ArenaState arena, GunState playerGun, GunState enemyGun)
        {
            _damageRequests.Clear();

            for (int i = 0; i < _bullets.Count; i++)
            {
                BulletState bullet = _bullets[i];

                if (bullet.IsAlive == false)
                {
                    continue;
                }

                MoveAndCollide(fixedDelta, arena, playerGun, enemyGun, bullet);
            }
        }

        public void CaptureTickDiffs()
        {
            _spawnedThisTick.Clear();
            _movedThisTick.Clear();
            _removedThisTick.Clear();

            for (int i = 0; i < _bullets.Count; i++)
            {
                BulletState bullet = _bullets[i];
                bool wasAlive = _wasAliveThisTick[i];

                if (bullet.IsAlive && wasAlive == false)
                {
                    _spawnedThisTick.Add(new BulletSpawnedSnapshot(bullet.Id, bullet.Position, bullet.Radius * 2f, bullet.IsPlayerOwned));
                }
                else if (bullet.IsAlive && wasAlive)
                {
                    _movedThisTick.Add(new BulletMovedSnapshot(bullet.Id, bullet.Position));
                }
                else if (bullet.IsAlive == false && wasAlive)
                {
                    _removedThisTick.Add(bullet.Id);
                }

                _wasAliveThisTick[i] = bullet.IsAlive;
            }
        }

        public void ClearTickDiffs()
        {
            _spawnedThisTick.Clear();
            _movedThisTick.Clear();
            _removedThisTick.Clear();
        }

        private BulletState FindFreeBullet()
        {
            for (int i = 0; i < _bullets.Count; i++)
            {
                if (_bullets[i].IsAlive == false)
                {
                    return _bullets[i];
                }
            }

            BulletState newBullet = new BulletState();
            _bullets.Add(newBullet);
            _wasAliveThisTick.Add(false);

            return newBullet;
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

            if (bullet.IsPlayerOwned == targetIsPlayer)
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
