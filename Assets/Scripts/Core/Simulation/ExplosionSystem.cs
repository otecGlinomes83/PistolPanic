using System.Collections.Generic;
using UnityEngine;

namespace PistolPanic.Core
{
    public sealed class ExplosionSystem
    {
        private readonly List<ExplosionRecord> _explosionsThisTick = new List<ExplosionRecord>(4);

        private readonly List<DamageRequest> _damageRequests = new List<DamageRequest>(4);

        public IReadOnlyList<ExplosionRecord> ExplosionsThisTick => _explosionsThisTick;

        public IReadOnlyList<DamageRequest> DamageRequests => _damageRequests;

        public void Tick(List<BulletState> bullets, GunState playerGun, GunState enemyGun, ExplosionConfig explosionConfig)
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
                    BulletState secondBullet = bullets[secondIndex];

                    if (secondBullet.IsAlive == false)
                    {
                        continue;
                    }

                    TryExplodePair(firstBullet, secondBullet, playerGun, enemyGun, explosionConfig);
                }
            }
        }

        private void TryExplodePair(BulletState firstBullet, BulletState secondBullet, GunState playerGun, GunState enemyGun, ExplosionConfig explosionConfig)
        {
            float combinedRadius = firstBullet.Radius + secondBullet.Radius;
            Vector2 offset = secondBullet.Position - firstBullet.Position;

            if (offset.sqrMagnitude > combinedRadius * combinedRadius)
            {
                return;
            }

            Vector2 explosionPosition = (firstBullet.Position + secondBullet.Position) * 0.5f;

            firstBullet.IsAlive = false;
            firstBullet.DeathReason = BulletDeathReason.Explosion;
            secondBullet.IsAlive = false;
            secondBullet.DeathReason = BulletDeathReason.Explosion;

            _explosionsThisTick.Add(new ExplosionRecord(explosionPosition, explosionConfig.RadiusUnits));

            AddExplosionDamageRequest(playerGun, explosionPosition, explosionConfig, false);
            AddExplosionDamageRequest(enemyGun, explosionPosition, explosionConfig, true);
        }

        private void AddExplosionDamageRequest(GunState targetGun, Vector2 explosionPosition, ExplosionConfig explosionConfig, bool targetIsPlayer)
        {
            Vector2 offset = targetGun.Position - explosionPosition;

            if (offset.sqrMagnitude > explosionConfig.RadiusUnits * explosionConfig.RadiusUnits)
            {
                return;
            }

            _damageRequests.Add(new DamageRequest(targetIsPlayer, explosionConfig.Damage));
        }
    }
}
