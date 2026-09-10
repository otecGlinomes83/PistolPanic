using PistolPanic.Core;
using PistolPanic.Meta;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace PistolPanic.Presentation
{
    public sealed class GameLifetimeScope : LifetimeScope
    {
        [SerializeField]
        private MapConfig _mapConfig = null;

        [SerializeField]
        private WeaponConfig _weaponConfig = null;

        [SerializeField]
        private ExplosionConfig _explosionConfig = null;

        [SerializeField]
        private PhysicsConfig _physicsConfig = null;

        [SerializeField]
        private PlayerConfig _playerConfig = null;

        [SerializeField]
        private EnemyConfig _enemyConfig = null;

        protected override void Configure(IContainerBuilder builder)
        {
            if (Parent == null)
            {
                Debug.LogError("GameLifetimeScope: parent project scope is missing. Scenes must be loaded through SceneLoader from Bootstrap.");

                return;
            }

            Debug.Log("Scope: Configure GameLifetimeScope, mapConfig=" + (_mapConfig != null) + " weaponConfig=" + (_weaponConfig != null) + " explosionConfig=" + (_explosionConfig != null) + " physicsConfig=" + (_physicsConfig != null) + " playerConfig=" + (_playerConfig != null) + " enemyConfig=" + (_enemyConfig != null));

            bool hasAllConfigs = _mapConfig != null && _weaponConfig != null && _explosionConfig != null && _physicsConfig != null && _playerConfig != null && _enemyConfig != null;

            if (hasAllConfigs == false)
            {
                Debug.LogError("GameLifetimeScope: configs are not assigned. Create MapConfig/WeaponConfig/ExplosionConfig/PhysicsConfig/PlayerConfig/EnemyConfig (Create → PistolPanic) in Assets/Configs and assign on GameScope in Game scene.");

                return;
            }

            builder.RegisterInstance(_mapConfig);
            builder.RegisterInstance(_weaponConfig);
            builder.RegisterInstance(_explosionConfig);
            builder.RegisterInstance(_physicsConfig);
            builder.RegisterInstance(_playerConfig);
            builder.RegisterInstance(_enemyConfig);
            builder.RegisterInstance(new WeaponParams(_weaponConfig));

            builder.Register<CollisionMath>(Lifetime.Scoped);
            builder.Register<DuelFlow>(Lifetime.Scoped);
            builder.Register<MovementSystem>(Lifetime.Scoped);
            builder.Register<AmmoSystem>(Lifetime.Scoped);
            builder.Register<FireSystem>(Lifetime.Scoped);
            builder.Register<BulletSystem>(Lifetime.Scoped);
            builder.Register<ExplosionSystem>(Lifetime.Scoped);
            builder.Register<DamageSystem>(Lifetime.Scoped);
            builder.Register<AiShooterSystem>(Lifetime.Scoped);
            builder.Register<DuelSim>(Lifetime.Scoped);
            builder.Register<MatchResultApplier>(Lifetime.Scoped);

            builder.RegisterComponentOnNewGameObject<SimulationDriver>(Lifetime.Scoped, "SimulationDriver");
            builder.RegisterComponentOnNewGameObject<InputReader>(Lifetime.Scoped, "InputReader").As<IPlayerInput>();
            builder.RegisterComponentOnNewGameObject<ArenaView>(Lifetime.Scoped, "ArenaView");
            builder.RegisterComponentOnNewGameObject<BulletPool>(Lifetime.Scoped, "BulletPool").As<IBulletPool>();
            builder.RegisterComponentOnNewGameObject<ViewBinder>(Lifetime.Scoped, "ViewBinder");
            builder.RegisterComponentOnNewGameObject<DuelResultOverlayView>(Lifetime.Scoped, "DuelResultOverlay");
            builder.RegisterComponentOnNewGameObject<GameplayLifecyclePresenter>(Lifetime.Scoped, "GameplayLifecyclePresenter");
#if UNITY_EDITOR
            builder.RegisterComponentOnNewGameObject<SimulationDebugOverlay>(Lifetime.Scoped, "SimulationDebugOverlay");
#endif

            builder.RegisterBuildCallback(ActivateAmbientComponents);
        }

        private void ActivateAmbientComponents(IObjectResolver resolver)
        {
            Debug.Log("Scope: activating ambient components");

            resolver.Resolve<SimulationDriver>();
            resolver.Resolve<IPlayerInput>();
            resolver.Resolve<ArenaView>();
            resolver.Resolve<ViewBinder>();
            resolver.Resolve<DuelResultOverlayView>();
            resolver.Resolve<GameplayLifecyclePresenter>();
#if UNITY_EDITOR
            resolver.Resolve<SimulationDebugOverlay>();
#endif
        }
    }
}
