using PistolPanic.Core;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace PistolPanic.Presentation
{
    public class ProjectLifetimeScope : LifetimeScope
    {
        [SerializeField]
        private SimulationConfig _simulationConfig = null;

        protected override void Awake()
        {
            base.Awake();

            DontDestroyOnLoad(gameObject);
        }

        protected sealed override void Configure(IContainerBuilder builder)
        {
            Debug.Log("Scope: Configure " + GetType().Name);

            RegisterSimulationConfig(builder);

            builder.RegisterComponentInHierarchy<Bootstrapper>();

            builder.Register<SceneLoader>(Lifetime.Singleton).As<ISceneLoader>();

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
    }
}
