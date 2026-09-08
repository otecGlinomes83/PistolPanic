using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using UnityEngine;

namespace PistolPanic.Core
{
    public sealed class DuelSim
    {
        private const int BulletListCapacity = 64;

        private readonly IPlayerInput _playerInput;

        private readonly MovementSystem _movementSystem;

        private readonly AmmoSystem _ammoSystem;

        private readonly FireSystem _fireSystem;

        private readonly BulletSystem _bulletSystem;

        private readonly ExplosionSystem _explosionSystem;

        private readonly DamageSystem _damageSystem;

        private readonly AiShooterSystem _aiShooterSystem;

        private readonly DuelFlow _duelFlow;

        private readonly ArenaState _arena;

        private readonly GunState _playerGun;

        private readonly GunState _enemyGun;

        private readonly WeaponParams _weaponParams;

        private readonly ExplosionConfig _explosionConfig;

        private readonly EnemyConfig _enemyConfig;

        private readonly EnemyHealthState _enemyHealth;

        private readonly float _gracePeriodSeconds;

        private readonly float _playerMaxHealth;

        private readonly AmmoState _playerAmmo;

        private readonly List<BulletState> _bullets = new List<BulletState>(BulletListCapacity);

        private readonly List<GunHitRecord> _appliedHits = new List<GunHitRecord>(16);

        private readonly List<DamageRequest> _collectedDamageRequests = new List<DamageRequest>(16);

        private float _fightElapsedSeconds;

        private int _lastEnemyStageIndex;

        private int _nextBulletId = 1;

        public DuelSim(
            IPlayerInput playerInput,
            MovementSystem movementSystem,
            AmmoSystem ammoSystem,
            FireSystem fireSystem,
            BulletSystem bulletSystem,
            ExplosionSystem explosionSystem,
            DamageSystem damageSystem,
            AiShooterSystem aiShooterSystem,
            DuelFlow duelFlow,
            MapConfig mapConfig,
            WeaponConfig weaponConfig,
            ExplosionConfig explosionConfig,
            PlayerConfig playerConfig,
            EnemyConfig enemyConfig)
        {
            _playerInput = playerInput;
            _movementSystem = movementSystem;
            _ammoSystem = ammoSystem;
            _fireSystem = fireSystem;
            _bulletSystem = bulletSystem;
            _explosionSystem = explosionSystem;
            _damageSystem = damageSystem;
            _aiShooterSystem = aiShooterSystem;
            _duelFlow = duelFlow;
            _arena = new ArenaState(mapConfig);
            _weaponParams = new WeaponParams(weaponConfig);
            _explosionConfig = explosionConfig;
            _enemyConfig = enemyConfig;
            _gracePeriodSeconds = mapConfig.GracePeriodSeconds;
            _playerMaxHealth = playerConfig.MaxHealth;
            _duelFlow.PhaseChanged += OnDuelPhaseChanged;

            float enemySpawnRotationDegrees = mapConfig.PlayerSpawnRotationDegrees + 180f;
            _playerGun = CreateGun(mapConfig, mapConfig.PlayerSpawnPoint, mapConfig.PlayerSpawnRotationDegrees, _playerMaxHealth);
            _enemyGun = CreateGun(mapConfig, mapConfig.EnemySpawnPoint, enemySpawnRotationDegrees, float.MaxValue);

            _enemyHealth = _damageSystem.CreateEnemyHealth();
            _lastEnemyStageIndex = _enemyHealth.StageIndex;

            _playerAmmo = new AmmoState();
            _playerAmmo.Magazine = _weaponParams.MagazineSize;
            _playerAmmo.CooldownRemainingSeconds = 0f;
            _playerAmmo.RegenTimerSeconds = 0f;
            _playerAmmo.ReloadRemainingSeconds = 0f;
            _playerAmmo.IsFullReloading = false;

            Debug.Log("DuelSim: init, arena=" + mapConfig.ArenaSize + " obstacles=" + mapConfig.Obstacles.Count + " enemyStages=" + _enemyHealth.TotalStages);
        }

        public event Action<GunSnapshot> PlayerGunUpdated;

        public event Action<GunSnapshot> EnemyGunUpdated;

        public event Action<BulletSpawnedSnapshot> BulletSpawned;

        public event Action<BulletSpawnedSnapshot> BulletUpdated;

        public event Action<int> BulletRemoved;

        public event Action<ExplosionRecord> ExplosionHappened;

        public event Action<GunHitRecord> GunHit;

        public event Action<DuelPhase> DuelPhaseChanged;

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

            _fightElapsedSeconds += fixedDelta;

            TickFire(fixedDelta, isPressed);
            TickEnemyAi(fixedDelta);
            TickBullets(fixedDelta);
            TickMovement(fixedDelta);
            TickExplosions();
            ApplyDamage();
            CheckDeaths();
            PublishEvents();
        }

        private void TickFire(float fixedDelta, bool isPressed)
        {
            _fireSystem.Tick(fixedDelta, _ammoSystem, _playerAmmo, _weaponParams);

            if (isPressed && _duelFlow.IsFireAllowed)
            {
                TryFirePlayerGun();
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

            float fireAngleRadians = _enemyGun.RotationDegrees * Mathf.Deg2Rad;
            Vector2 fireDirection = new Vector2(Mathf.Cos(fireAngleRadians), Mathf.Sin(fireAngleRadians));

            SpawnBullet(_enemyGun, fireDirection, false);
            _movementSystem.ApplyRecoil(_enemyGun, fireDirection, _weaponParams);
        }

        private void TickBullets(float fixedDelta)
        {
            _bulletSystem.Tick(fixedDelta, _arena, _playerGun, _enemyGun, _bullets);
        }

        private void TickMovement(float fixedDelta)
        {
            _movementSystem.Tick(fixedDelta, _arena, _playerGun);
            _movementSystem.Tick(fixedDelta, _arena, _enemyGun);
            _movementSystem.ResolveGunPairCollision(_playerGun, _enemyGun);
        }

        private void TickExplosions()
        {
            _explosionSystem.Tick(_bullets, _playerGun, _enemyGun, _explosionConfig);
        }

        private void ApplyDamage()
        {
            _appliedHits.Clear();
            _collectedDamageRequests.Clear();

            IReadOnlyList<DamageRequest> bulletRequests = _bulletSystem.DamageRequests;
            IReadOnlyList<DamageRequest> explosionRequests = _explosionSystem.DamageRequests;

            bool hasRequests = bulletRequests.Count > 0 || explosionRequests.Count > 0;

            if (hasRequests == false)
            {
                return;
            }

            CollectRequests(bulletRequests);
            CollectRequests(explosionRequests);

            List<DamageRequest> collectedRequests = _collectedDamageRequests;

            _damageSystem.ApplyDamage(collectedRequests, _playerGun, _enemyHealth, _fightElapsedSeconds, _appliedHits);
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

        private void TryFirePlayerGun()
        {
            if (_ammoSystem.CanFire(_playerAmmo) == false)
            {
                return;
            }

            _ammoSystem.ConsumeShot(_playerAmmo, _weaponParams);

            float fireAngleRadians = _playerGun.RotationDegrees * Mathf.Deg2Rad;
            Vector2 fireDirection = new Vector2(Mathf.Cos(fireAngleRadians), Mathf.Sin(fireAngleRadians));

            SpawnBullet(_playerGun, fireDirection, true);
            _movementSystem.ApplyRecoil(_playerGun, fireDirection, _weaponParams);
        }

        private void SpawnBullet(GunState ownerGun, Vector2 fireDirection, bool ownerIsPlayer)
        {
            BulletState bullet = FindFreeBullet();
            float spawnOffset = ownerGun.Radius + _weaponParams.BulletRadius + 0.05f;

            bullet.Id = _nextBulletId;
            _nextBulletId += 1;
            bullet.Position = ownerGun.Position + fireDirection * spawnOffset;
            bullet.PreviousPosition = bullet.Position;
            bullet.Velocity = fireDirection * _weaponParams.BulletSpeed;
            bullet.Radius = _weaponParams.BulletRadius;
            bullet.Damage = _weaponParams.Damage;
            bullet.OwnerIsPlayer = ownerIsPlayer;
            bullet.IsAlive = true;
            bullet.WasAlive = false;
            bullet.DeathReason = BulletDeathReason.None;

            Debug.Log("DuelSim: bullet " + bullet.Id + " spawned at " + bullet.Position + " speed " + bullet.Velocity.magnitude);
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

            return newBullet;
        }

        private void PublishEvents()
        {
            GunSnapshot playerSnapshot = new GunSnapshot(_playerGun.Position, _playerGun.RotationDegrees);
            GunSnapshot enemySnapshot = new GunSnapshot(_enemyGun.Position, _enemyGun.RotationDegrees);
            PlayerGunUpdated?.Invoke(playerSnapshot);
            EnemyGunUpdated?.Invoke(enemySnapshot);

            for (int i = 0; i < _bullets.Count; i++)
            {
                BulletState bullet = _bullets[i];

                if (bullet.IsAlive && bullet.WasAlive == false)
                {
                    BulletSpawnedSnapshot spawnSnapshot = new BulletSpawnedSnapshot(bullet.Id, bullet.Position, bullet.Radius * 2f);
                    BulletSpawned?.Invoke(spawnSnapshot);
                }
                else if (bullet.IsAlive && bullet.WasAlive)
                {
                    BulletSpawnedSnapshot moveSnapshot = new BulletSpawnedSnapshot(bullet.Id, bullet.Position, bullet.Radius * 2f);
                    BulletUpdated?.Invoke(moveSnapshot);
                }
                else if (bullet.IsAlive == false && bullet.WasAlive)
                {
                    Debug.Log("DuelSim: bullet " + bullet.Id + " died, reason " + bullet.DeathReason + " at " + bullet.Position);
                    BulletRemoved?.Invoke(bullet.Id);
                }

                bullet.WasAlive = bullet.IsAlive;
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
                Debug.Log("DuelSim: enemy stage -> " + _enemyHealth.StageIndex);
                EnemyStageChanged?.Invoke(_enemyHealth.StageIndex);
            }
        }

        private void OnDuelPhaseChanged(DuelPhase phase)
        {
            DuelPhaseChanged?.Invoke(phase);
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
