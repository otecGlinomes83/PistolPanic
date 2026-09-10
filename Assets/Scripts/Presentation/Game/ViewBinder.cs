using System.Collections.Generic;
using PistolPanic.Core;
using PistolPanic.Meta;
using UnityEngine;
using VContainer;

namespace PistolPanic.Presentation
{
    public sealed class ViewBinder : MonoBehaviour
    {
        private readonly Color _playerBulletColor = new Color(1f, 0.85f, 0.3f, 1f);

        private readonly Color _enemyBulletColor = new Color(0.9f, 0.3f, 0.25f, 1f);

        [Inject]
        private readonly DuelSim _duelSim = null;

        [Inject]
        private readonly SpriteFactory _spriteFactory = null;

        [Inject]
        private readonly IBulletPool _bulletPool = null;

        [Inject]
        private readonly FireSystem _fireSystem = null;

        [Inject]
        private readonly PlayerWeaponProvider _playerWeaponProvider = null;

        [Inject]
        private readonly EnemyTypeProvider _enemyTypeProvider = null;

        [Inject]
        private readonly FxPool _fxPool = null;

        [Inject]
        private readonly AudioService _audioService = null;

        private GunView _playerGunView;

        private GunView _enemyGunView;

        private void Start()
        {
            CreateGunViews();

            _duelSim.PlayerGunUpdated += OnPlayerGunUpdated;
            _duelSim.EnemyGunUpdated += OnEnemyGunUpdated;
            _duelSim.BulletSpawned += OnBulletSpawned;
            _duelSim.BulletUpdated += OnBulletUpdated;
            _duelSim.BulletRemoved += OnBulletRemoved;
            _duelSim.GunHit += OnGunHit;
            _duelSim.ExplosionHappened += OnExplosionHappened;
            _duelSim.EnemyStageChanged += OnEnemyStageChanged;
            _fireSystem.ShotFired += OnShotFired;

            Debug.Log("ViewBinder: gun views created and subscribed");
        }

        private void OnDestroy()
        {
            _duelSim.PlayerGunUpdated -= OnPlayerGunUpdated;
            _duelSim.EnemyGunUpdated -= OnEnemyGunUpdated;
            _duelSim.BulletSpawned -= OnBulletSpawned;
            _duelSim.BulletUpdated -= OnBulletUpdated;
            _duelSim.BulletRemoved -= OnBulletRemoved;
            _duelSim.GunHit -= OnGunHit;
            _duelSim.ExplosionHappened -= OnExplosionHappened;
            _duelSim.EnemyStageChanged -= OnEnemyStageChanged;
            _fireSystem.ShotFired -= OnShotFired;
        }

        private void CreateGunViews()
        {
            Sprite playerGunSprite = _spriteFactory.GetGunSprite(_playerWeaponProvider.CurrentWeaponConfig.FirePattern);
            Sprite enemyGunSprite = _spriteFactory.GetGunSprite(_enemyTypeProvider.Current.WeaponConfig.FirePattern);
            Color playerGunColor = new Color(0.3f, 0.85f, 0.4f, 1f);

            _playerGunView = CreateGunView("PlayerGunView", playerGunSprite, playerGunColor);
            _enemyGunView = CreateGunView("EnemyGunView", enemyGunSprite, _enemyTypeProvider.Current.GunColor);
        }

        private GunView CreateGunView(string viewName, Sprite gunSprite, Color gunColor)
        {
            GameObject viewObject = new GameObject(viewName);
            viewObject.transform.SetParent(transform, false);

            GunView gunView = viewObject.AddComponent<GunView>();
            gunView.Initialize(gunSprite, gunColor);

            return gunView;
        }

        private void OnEnemyStageChanged(int stageIndex)
        {
            _audioService.PlaySfx(SfxType.StageBreak);

            IReadOnlyList<Color> stageColors = _enemyTypeProvider.Current.StageColors;

            if (stageIndex < 0 || stageIndex >= stageColors.Count)
            {
                return;
            }

            _enemyGunView.SetColor(stageColors[stageIndex]);
        }

        private void OnGunHit(GunHitRecord record)
        {
            _audioService.PlaySfx(SfxType.GunHit);

            Vector3 flashPosition;

            if (record.IsTargetPlayer)
            {
                flashPosition = _playerGunView.transform.position;
            }
            else
            {
                flashPosition = _enemyGunView.transform.position;
            }

            _fxPool.PlayFlash(flashPosition, new Color(1f, 0.4f, 0.3f), 0.9f, 0.2f, 0.15f);
        }

        private void OnExplosionHappened(ExplosionRecord record)
        {
            _audioService.PlaySfx(SfxType.Explosion);

            Vector3 flashPosition = new Vector3(record.Position.x, record.Position.y, 0f);

            _fxPool.PlayFlash(flashPosition, new Color(1f, 0.75f, 0.25f), 0.3f, record.Radius * 2f, 0.3f);
        }

        private void OnShotFired(ShotFiredSnapshot snapshot)
        {
            _audioService.PlaySfx(MapPatternToSfx(snapshot.Pattern));

            Vector3 flashPosition;

            if (snapshot.IsPlayerOwned)
            {
                flashPosition = _playerGunView.transform.position;
            }
            else
            {
                flashPosition = _enemyGunView.transform.position;
            }

            _fxPool.PlayFlash(flashPosition, new Color(1f, 0.9f, 0.5f), 0.5f, 0.1f, 0.08f);
        }

        private SfxType MapPatternToSfx(FirePattern pattern)
        {
            int patternValue = (int)pattern;

            if (patternValue == (int)FirePattern.Burst)
            {
                return SfxType.BurstShot;
            }

            if (patternValue == (int)FirePattern.Shotgun)
            {
                return SfxType.ShotgunBlast;
            }

            return SfxType.PistolShot;
        }

        private void OnPlayerGunUpdated(GunSnapshot snapshot)
        {
            _playerGunView.ApplySnapshot(snapshot);
        }

        private void OnEnemyGunUpdated(GunSnapshot snapshot)
        {
            _enemyGunView.ApplySnapshot(snapshot);
        }

        private void OnBulletSpawned(BulletSpawnedSnapshot snapshot)
        {
            Color bulletColor = _playerBulletColor;

            if (snapshot.IsPlayerOwned == false)
            {
                bulletColor = _enemyBulletColor;
            }

            _bulletPool.Spawn(snapshot.BulletId, snapshot.Position, snapshot.Diameter, bulletColor);
        }

        private void OnBulletUpdated(BulletMovedSnapshot snapshot)
        {
            _bulletPool.Move(snapshot.BulletId, snapshot.Position);
        }

        private void OnBulletRemoved(int bulletId)
        {
            _bulletPool.Despawn(bulletId);
        }
    }
}
