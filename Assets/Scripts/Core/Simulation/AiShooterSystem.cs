using UnityEngine;

namespace PistolPanic.Core
{
    public sealed class AiShooterSystem
    {
        private const float InitialSinceLastShotSeconds = 999f;

        private readonly CollisionMath _collisionMath;

        private float _randomCooldownSeconds;

        private float _invisibleSeconds;

        private float _sinceLastShotSeconds = InitialSinceLastShotSeconds;

        private bool _wasPlayerVisible;

        private int _lastStageIndex = -1;

        public AiShooterSystem(CollisionMath collisionMath)
        {
            _collisionMath = collisionMath;
        }

        public bool Tick(float fixedDelta, ArenaState arena, GunState enemyGun, GunState playerGun, EnemyConfig enemyConfig, int enemyStageIndex)
        {
            if (enemyStageIndex != _lastStageIndex)
            {
                OnStageChanged(enemyConfig, enemyStageIndex);
            }

            bool isPlayerVisible = IsPlayerVisible(arena, enemyGun, playerGun, enemyConfig);

            TickVisibility(fixedDelta, isPlayerVisible, enemyConfig);

            _wasPlayerVisible = isPlayerVisible;
            _sinceLastShotSeconds += fixedDelta;
            _randomCooldownSeconds -= fixedDelta;

            if (_randomCooldownSeconds > 0f)
            {
                return false;
            }

            ResetRandomCooldown(enemyConfig);
            _sinceLastShotSeconds = 0f;

            return true;
        }

        private void OnStageChanged(EnemyConfig enemyConfig, int enemyStageIndex)
        {
            _lastStageIndex = enemyStageIndex;
            ResetRandomCooldown(enemyConfig);
        }

        private void TickVisibility(float fixedDelta, bool isPlayerVisible, EnemyConfig enemyConfig)
        {
            if (isPlayerVisible == false)
            {
                _invisibleSeconds += fixedDelta;

                return;
            }

            bool isNewAppearance = _wasPlayerVisible == false && _invisibleSeconds >= enemyConfig.VisibilityDebounceSeconds;

            _invisibleSeconds = 0f;

            if (isNewAppearance == false)
            {
                return;
            }

            if (_sinceLastShotSeconds < enemyConfig.ReactionSeconds)
            {
                return;
            }

            if (_randomCooldownSeconds > enemyConfig.ReactionSeconds)
            {
                _randomCooldownSeconds = enemyConfig.ReactionSeconds;
            }
        }

        private void ResetRandomCooldown(EnemyConfig enemyConfig)
        {
            _randomCooldownSeconds = Random.Range(enemyConfig.FireCooldownMinSeconds, enemyConfig.FireCooldownMaxSeconds);
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
