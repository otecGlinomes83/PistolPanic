using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using UnityEngine;

namespace PistolPanic.Core
{
    public sealed class DuelSim
    {
        private readonly IPlayerInput _playerInput;

        private readonly MovementSystem _movementSystem;

        private readonly FireSystem _fireSystem;

        private readonly BulletSystem _bulletSystem;

        private readonly ExplosionSystem _explosionSystem;

        private readonly DamageSystem _damageSystem;

        private readonly AiShooterSystem _aiShooterSystem;

        private readonly DuelFlow _duelFlow;

        private readonly ArenaState _arena;

        private readonly GunState _playerGun;

        private readonly GunState _enemyGun;

        private readonly EnemyConfig _enemyConfig;

        private readonly EnemyHealthState _enemyHealth;

        private readonly float _gracePeriodSeconds;

        private readonly float _playerMaxHealth;

        private readonly List<GunHitRecord> _appliedHits = new List<GunHitRecord>(16);

        private readonly List<DamageRequest> _collectedDamageRequests = new List<DamageRequest>(16);

        private int _lastEnemyStageIndex;

        public DuelSim(
            IPlayerInput playerInput,
            MovementSystem movementSystem,
            FireSystem fireSystem,
            BulletSystem bulletSystem,
            ExplosionSystem explosionSystem,
            DamageSystem damageSystem,
            AiShooterSystem aiShooterSystem,
            DuelFlow duelFlow,
            MapConfig mapConfig,
            PlayerConfig playerConfig,
            EnemyConfig enemyConfig)
        {
            _playerInput = playerInput;
            _movementSystem = movementSystem;
            _fireSystem = fireSystem;
            _bulletSystem = bulletSystem;
            _explosionSystem = explosionSystem;
            _damageSystem = damageSystem;
            _aiShooterSystem = aiShooterSystem;
            _duelFlow = duelFlow;
            _arena = new ArenaState(mapConfig);
            _enemyConfig = enemyConfig;
            _gracePeriodSeconds = mapConfig.GracePeriodSeconds;
            _playerMaxHealth = playerConfig.MaxHealth;

            _playerGun = CreateGun(mapConfig, mapConfig.PlayerSpawnPoint, mapConfig.PlayerSpawnRotationDegrees, _playerMaxHealth);
            _enemyGun = CreateGun(mapConfig, mapConfig.EnemySpawnPoint, mapConfig.EnemySpawnRotationDegrees, float.MaxValue);

            _enemyHealth = _damageSystem.CreateEnemyHealth();
            _lastEnemyStageIndex = _enemyHealth.StageIndex;
        }

        public event Action<GunSnapshot> PlayerGunUpdated;

        public event Action<GunSnapshot> EnemyGunUpdated;

        public event Action<BulletSpawnedSnapshot> BulletSpawned;

        public event Action<BulletMovedSnapshot> BulletUpdated;

        public event Action<int> BulletRemoved;

        public event Action<ExplosionRecord> ExplosionHappened;

        public event Action<GunHitRecord> GunHit;

        public event Action<int> EnemyStageChanged;

        public void Tick(float fixedDelta)
        {
            bool isPressed = _playerInput.ConsumePress();

            if (isPressed && _duelFlow.Phase == DuelPhase.Armed)
            {
                _duelFlow.StartGrace(_gracePeriodSeconds);
            }

            _duelFlow.Tick(fixedDelta);

            if (_duelFlow.IsSimulationRunning == false)
            {
                PublishEvents();

                return;
            }

            TickFire(fixedDelta, isPressed);
            TickEnemyAi(fixedDelta);
            TickBullets(fixedDelta);
            TickMovement(fixedDelta);
            TickExplosions();
            _bulletSystem.CaptureTickDiffs();
            ApplyDamage();
            CheckDeaths();
            PublishEvents();
        }

        private void TickFire(float fixedDelta, bool isPressed)
        {
            _fireSystem.Tick(fixedDelta);

            if (isPressed && _duelFlow.IsFireAllowed)
            {
                _fireSystem.TryFire(_playerGun, true);
            }
        }

        private void TickEnemyAi(float fixedDelta)
        {
            if (_duelFlow.IsFireAllowed == false)
            {
                return;
            }

            bool isFired = _aiShooterSystem.Tick(fixedDelta, _arena, _enemyGun, _playerGun, _enemyConfig, _enemyHealth.StageIndex);

            if (isFired == false)
            {
                return;
            }

            _fireSystem.TryFire(_enemyGun, false);
        }

        private void TickBullets(float fixedDelta)
        {
            _bulletSystem.Tick(fixedDelta, _arena, _playerGun, _enemyGun);
        }

        private void TickMovement(float fixedDelta)
        {
            _movementSystem.Tick(fixedDelta, _arena, _playerGun);
            _movementSystem.Tick(fixedDelta, _arena, _enemyGun);
            _movementSystem.ResolveGunPairCollision(_playerGun, _enemyGun);
        }

        private void TickExplosions()
        {
            _explosionSystem.Tick(_bulletSystem.Bullets, _playerGun, _enemyGun);
        }

        private void ApplyDamage()
        {
            IReadOnlyList<DamageRequest> bulletRequests = _bulletSystem.DamageRequests;
            IReadOnlyList<DamageRequest> explosionRequests = _explosionSystem.DamageRequests;

            bool hasRequests = bulletRequests.Count > 0 || explosionRequests.Count > 0;

            if (hasRequests == false)
            {
                return;
            }

            CollectRequests(bulletRequests);
            CollectRequests(explosionRequests);

            _damageSystem.ApplyDamage(_collectedDamageRequests, _playerGun, _enemyHealth, _duelFlow.FightElapsedSeconds, _appliedHits);
        }

        private void CollectRequests(IReadOnlyList<DamageRequest> requests)
        {
            for (int i = 0; i < requests.Count; i++)
            {
                _collectedDamageRequests.Add(requests[i]);
            }
        }

        private void CheckDeaths()
        {
            if (_enemyHealth.IsDefeated)
            {
                _duelFlow.CompleteVictory();

                return;
            }

            if (_playerGun.Health <= 0f)
            {
                _duelFlow.CompleteDefeat();
            }
        }

        private void PublishEvents()
        {
            GunSnapshot playerSnapshot = new GunSnapshot(_playerGun.Position, _playerGun.RotationDegrees);
            GunSnapshot enemySnapshot = new GunSnapshot(_enemyGun.Position, _enemyGun.RotationDegrees);
            PlayerGunUpdated?.Invoke(playerSnapshot);
            EnemyGunUpdated?.Invoke(enemySnapshot);

            IReadOnlyList<BulletSpawnedSnapshot> spawnedBullets = _bulletSystem.SpawnedThisTick;

            for (int i = 0; i < spawnedBullets.Count; i++)
            {
                BulletSpawned?.Invoke(spawnedBullets[i]);
            }

            IReadOnlyList<BulletMovedSnapshot> movedBullets = _bulletSystem.MovedThisTick;

            for (int i = 0; i < movedBullets.Count; i++)
            {
                BulletUpdated?.Invoke(movedBullets[i]);
            }

            IReadOnlyList<int> removedBullets = _bulletSystem.RemovedThisTick;

            for (int i = 0; i < removedBullets.Count; i++)
            {
                BulletRemoved?.Invoke(removedBullets[i]);
            }

            for (int i = 0; i < _appliedHits.Count; i++)
            {
                GunHit?.Invoke(_appliedHits[i]);
            }

            IReadOnlyList<ExplosionRecord> explosions = _explosionSystem.ExplosionsThisTick;

            for (int i = 0; i < explosions.Count; i++)
            {
                ExplosionHappened?.Invoke(explosions[i]);
            }

            if (_enemyHealth.StageIndex != _lastEnemyStageIndex)
            {
                _lastEnemyStageIndex = _enemyHealth.StageIndex;
                EnemyStageChanged?.Invoke(_enemyHealth.StageIndex);
            }

            _bulletSystem.ClearTickDiffs();
            _appliedHits.Clear();
            _collectedDamageRequests.Clear();
        }

        private GunState CreateGun(MapConfig mapConfig, Vector2 spawnPoint, float spawnRotationDegrees, float health)
        {
            float impulseAngleRadians = Random.Range(0f, Mathf.PI * 2f);
            Vector2 impulseDirection = new Vector2(Mathf.Cos(impulseAngleRadians), Mathf.Sin(impulseAngleRadians));
            float impulseMagnitude = Random.Range(mapConfig.SpawnImpulseMin, mapConfig.SpawnImpulseMax);
            float angularVelocity = Random.Range(mapConfig.SpawnAngularVelocityMin, mapConfig.SpawnAngularVelocityMax);

            GunState gun = new GunState();
            gun.Position = spawnPoint;
            gun.Velocity = impulseDirection * impulseMagnitude;
            gun.RotationDegrees = spawnRotationDegrees;
            gun.AngularVelocityDegrees = angularVelocity;
            gun.Radius = mapConfig.GunCollisionRadius;
            gun.Health = health;
            gun.InvulnerableUntilSeconds = 0f;

            return gun;
        }
    }
}
