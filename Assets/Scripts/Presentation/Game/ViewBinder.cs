using PistolPanic.Core;
using UnityEngine;
using VContainer;

namespace PistolPanic.Presentation
{
    public sealed class ViewBinder : MonoBehaviour
    {
        [Inject]
        private readonly DuelSim _duelSim = null;

        [Inject]
        private readonly SpriteFactory _spriteFactory = null;

        [Inject]
        private readonly IBulletPool _bulletPool = null;

        [Inject]
        private readonly EnemyConfig _enemyConfig = null;

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
            _duelSim.EnemyStageChanged += OnEnemyStageChanged;

            Debug.Log("ViewBinder: gun views created and subscribed");
        }

        private void OnDestroy()
        {
            _duelSim.PlayerGunUpdated -= OnPlayerGunUpdated;
            _duelSim.EnemyGunUpdated -= OnEnemyGunUpdated;
            _duelSim.BulletSpawned -= OnBulletSpawned;
            _duelSim.BulletUpdated -= OnBulletUpdated;
            _duelSim.BulletRemoved -= OnBulletRemoved;
            _duelSim.EnemyStageChanged -= OnEnemyStageChanged;
        }

        private void CreateGunViews()
        {
            Sprite gunSprite = _spriteFactory.GetSquareSprite();

            _playerGunView = CreateGunView("PlayerGunView", gunSprite, new Color(0.3f, 0.85f, 0.4f, 1f));
            _enemyGunView = CreateGunView("EnemyGunView", gunSprite, new Color(0.85f, 0.35f, 0.3f, 1f));
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
            if (stageIndex < 0 || stageIndex >= _enemyConfig.StageColors.Count)
            {
                return;
            }

            _enemyGunView.SetColor(_enemyConfig.StageColors[stageIndex]);
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
            _bulletPool.Spawn(snapshot.BulletId, snapshot.Position, snapshot.Diameter);
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
