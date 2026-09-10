using System.Collections.Generic;

namespace PistolPanic.Core
{
    public sealed class DamageSystem
    {
        private readonly PlayerConfig _playerConfig;

        private readonly float[] _stageHealthLookup;

        public DamageSystem(PlayerConfig playerConfig, EnemyConfig enemyConfig)
        {
            _playerConfig = playerConfig;
            _stageHealthLookup = new float[enemyConfig.StageHealths.Count];

            for (int i = 0; i < enemyConfig.StageHealths.Count; i++)
            {
                _stageHealthLookup[i] = enemyConfig.StageHealths[i];
            }
        }

        public EnemyHealthState CreateEnemyHealth()
        {
            EnemyHealthState enemyHealth = new EnemyHealthState();
            enemyHealth.TotalStages = _stageHealthLookup.Length;
            enemyHealth.StageIndex = 0;
            enemyHealth.StageHealthRemaining = _stageHealthLookup[0];

            return enemyHealth;
        }

        public void ApplyDamage(
            List<DamageRequest> requests,
            GunState playerGun,
            EnemyHealthState enemyHealth,
            float currentFightSeconds,
            List<GunHitRecord> appliedHits)
        {
            for (int i = 0; i < requests.Count; i++)
            {
                if (requests[i].IsTargetPlayer)
                {
                    ApplyToPlayer(requests[i], playerGun, currentFightSeconds, appliedHits);
                }
                else
                {
                    ApplyToEnemy(requests[i], enemyHealth, appliedHits);
                }
            }
        }

        private void ApplyToPlayer(DamageRequest request, GunState playerGun, float currentFightSeconds, List<GunHitRecord> appliedHits)
        {
            if (currentFightSeconds < playerGun.InvulnerableUntilSeconds)
            {
                return;
            }

            playerGun.Health -= request.Damage;
            playerGun.InvulnerableUntilSeconds = currentFightSeconds + _playerConfig.InvulnerabilitySeconds;
            appliedHits.Add(new GunHitRecord(true, request.Damage, playerGun.Health));
        }

        private void ApplyToEnemy(DamageRequest request, EnemyHealthState enemyHealth, List<GunHitRecord> appliedHits)
        {
            enemyHealth.StageHealthRemaining -= request.Damage;

            if (enemyHealth.StageHealthRemaining < 0f)
            {
                enemyHealth.StageHealthRemaining = 0f;
            }

            if (enemyHealth.StageHealthRemaining <= 0f && enemyHealth.StageIndex < enemyHealth.TotalStages - 1)
            {
                enemyHealth.StageIndex += 1;
                enemyHealth.StageHealthRemaining += _stageHealthLookup[enemyHealth.StageIndex];
            }

            appliedHits.Add(new GunHitRecord(false, request.Damage, enemyHealth.StageHealthRemaining));
        }
    }
}
