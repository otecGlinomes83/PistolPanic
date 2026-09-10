using PistolPanic.Core;
using PistolPanic.Meta;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace PistolPanic.Presentation
{
    public class ProjectLifetimeScope : LifetimeScope
    {
        [SerializeField]
        private SimulationConfig _simulationConfig = null;

        [SerializeField]
        private WeaponCatalog _weaponCatalog = null;

        protected override void Awake()
        {
            base.Awake();

            DontDestroyOnLoad(gameObject);
        }

        protected sealed override void Configure(IContainerBuilder builder)
        {
            Debug.Log("Scope: Configure " + GetType().Name);

            RegisterSimulationConfig(builder);
            RegisterWeaponCatalog(builder);

            builder.RegisterComponentInHierarchy<Bootstrapper>();

            builder.Register<SceneLoader>(Lifetime.Singleton).As<ISceneLoader>();

            builder.Register<WeaponLoadoutService>(Lifetime.Singleton);

            ConfigurePlatformServices(builder);
        }

        protected virtual void ConfigurePlatformServices(IContainerBuilder builder)
        {
        }

        private void RegisterSimulationConfig(IContainerBuilder builder)
        {
            if (_simulationConfig != null)
            {
                builder.RegisterInstance(_simulationConfig);

                return;
            }

            Debug.LogError("ProjectLifetimeScope: SimulationConfig is not assigned. Create asset (Create → PistolPanic → SimulationConfig) in Assets/Configs and assign it on the project scope in Bootstrap scene.");

            builder.RegisterInstance(ScriptableObject.CreateInstance<SimulationConfig>());
        }

        private void RegisterWeaponCatalog(IContainerBuilder builder)
        {
            if (_weaponCatalog != null)
            {
                builder.RegisterInstance(_weaponCatalog);

                return;
            }

            Debug.LogError("ProjectLifetimeScope: WeaponCatalog is not assigned. Create asset (Create → PistolPanic → WeaponCatalog) in Assets/Configs and assign it on the project scope in Bootstrap scene.");

            builder.RegisterInstance(ScriptableObject.CreateInstance<WeaponCatalog>());
        }
    }
}
