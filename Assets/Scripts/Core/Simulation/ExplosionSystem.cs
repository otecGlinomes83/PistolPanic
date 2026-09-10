using System.Collections.Generic;
using UnityEngine;

namespace PistolPanic.Core
{
    public sealed class ExplosionSystem
    {
        private readonly CollisionMath _collisionMath;

        private readonly ExplosionConfig _explosionConfig;

        private readonly List<ExplosionRecord> _explosionsThisTick = new List<ExplosionRecord>(4);

        private readonly List<DamageRequest> _damageRequests = new List<DamageRequest>(4);

        public ExplosionSystem(CollisionMath collisionMath, ExplosionConfig explosionConfig)
        {
            _collisionMath = collisionMath;
            _explosionConfig = explosionConfig;
        }

        public IReadOnlyList<ExplosionRecord> ExplosionsThisTick => _explosionsThisTick;

        public IReadOnlyList<DamageRequest> DamageRequests => _damageRequests;

        public void Tick(IReadOnlyList<BulletState> bullets, GunState playerGun, GunState enemyGun)
        {
            _explosionsThisTick.Clear();
            _damageRequests.Clear();

            for (int firstIndex = 0; firstIndex < bullets.Count; firstIndex++)
            {
                BulletState firstBullet = bullets[firstIndex];

                if (firstBullet.IsAlive == false)
                {
                    continue;
                }

                for (int secondIndex = firstIndex + 1; secondIndex < bullets.Count; secondIndex++)
                {
                    if (firstBullet.IsAlive == false)
                    {
                        break;
                    }

                    BulletState secondBullet = bullets[secondIndex];

                    if (secondBullet.IsAlive == false)
                    {
                        continue;
                    }

                    TryExplodePair(firstBullet, secondBullet, playerGun, enemyGun);
                }
            }
        }

        private void TryExplodePair(BulletState firstBullet, BulletState secondBullet, GunState playerGun, GunState enemyGun)
        {
            float combinedRadius = firstBullet.Radius + secondBullet.Radius;

            bool isCollision = _collisionMath.SegmentIntersectsCircle(firstBullet.PreviousPosition, firstBullet.Position, secondBullet.Position, combinedRadius);

            if (isCollision == false)
            {
                isCollision = _collisionMath.SegmentIntersectsCircle(secondBullet.PreviousPosition, secondBullet.Position, firstBullet.Position, combinedRadius);
            }

            if (isCollision == false)
            {
                return;
            }

            Vector2 explosionPosition = (firstBullet.Position + secondBullet.Position) * 0.5f;

            firstBullet.IsAlive = false;
            firstBullet.DeathReason = BulletDeathReason.Explosion;
            secondBullet.IsAlive = false;
            secondBullet.DeathReason = BulletDeathReason.Explosion;

            _explosionsThisTick.Add(new ExplosionRecord(explosionPosition, _explosionConfig.RadiusUnits));

            AddExplosionDamageRequest(playerGun, explosionPosition, true);
            AddExplosionDamageRequest(enemyGun, explosionPosition, false);
        }

        private void AddExplosionDamageRequest(GunState targetGun, Vector2 explosionPosition, bool targetIsPlayer)
        {
            float blastRadius = _explosionConfig.RadiusUnits + targetGun.Radius;

            Vector2 offset = targetGun.Position - explosionPosition;

            if (offset.sqrMagnitude > blastRadius * blastRadius)
            {
                return;
            }

            _damageRequests.Add(new DamageRequest(targetIsPlayer, _explosionConfig.Damage));
        }
    }
}
