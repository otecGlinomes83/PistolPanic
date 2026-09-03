using Random = UnityEngine.Random;
using UnityEngine;

namespace PistolPanic.Core
{
    public sealed class AiShooterSystem
    {
        private readonly CollisionMath _collisionMath;

        private float _cooldownRemainingSeconds = -1f;

        private int _lastStageIndex = -1;

        public AiShooterSystem(CollisionMath collisionMath)
        {
            _collisionMath = collisionMath;
        }

        public bool Tick(float fixedDelta, ArenaState arena, GunState enemyGun, GunState playerGun, EnemyConfig enemyConfig, int enemyStageIndex)
        {
            if (enemyStageIndex != _lastStageIndex)
            {
                _lastStageIndex = enemyStageIndex;
                _cooldownRemainingSeconds = Random.Range(enemyConfig.FireCooldownMinSeconds, enemyConfig.FireCooldownMaxSeconds);
            }

            _cooldownRemainingSeconds -= fixedDelta;

            if (_cooldownRemainingSeconds > 0f)
            {
                return false;
            }

            bool isPlayerVisible = IsPlayerVisible(arena, enemyGun, playerGun, enemyConfig);

            if (isPlayerVisible == false)
            {
                _cooldownRemainingSeconds = enemyConfig.VisionCheckIntervalSeconds;

                return false;
            }

            _cooldownRemainingSeconds = Random.Range(enemyConfig.FireCooldownMinSeconds, enemyConfig.FireCooldownMaxSeconds);

            return true;
        }

        private bool IsPlayerVisible(ArenaState arena, GunState enemyGun, GunState playerGun, EnemyConfig enemyConfig)
        {
            bool isInsideCone = _collisionMath.IsInsideCone(enemyGun.Position, enemyGun.RotationDegrees, playerGun.Position, enemyConfig.VisionConeDegrees);

            if (isInsideCone == false)
            {
                return false;
            }

            return HasLineOfSight(arena, enemyGun.Position, playerGun.Position);
        }

        private bool HasLineOfSight(ArenaState arena, Vector2 fromPosition, Vector2 toPosition)
        {
            ObstacleData[] obstacles = arena.Obstacles;

            for (int i = 0; i < obstacles.Length; i++)
            {
                if (_collisionMath.SegmentIntersectsCircle(fromPosition, toPosition, obstacles[i].Center, obstacles[i].Radius))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
