using PistolPanic.Core;
using PistolPanic.Meta;
using UnityEngine;
using VContainer;

namespace PistolPanic.Presentation
{
    public sealed class YgProjectLifetimeScope : ProjectLifetimeScope
    {
        [SerializeField]
        private EconomyConfig _economyConfig = null;

        protected override void ConfigurePlatformServices(IContainerBuilder builder)
        {
            Debug.Log("Scope: platform services (YG)");

            builder.Register<YgLifecycleAdapter>(Lifetime.Singleton).As<IPlatformLifecycle>();

            if (_economyConfig == null)
            {
                Debug.LogError("YgProjectLifetimeScope: EconomyConfig is not assigned. Create asset (Create → PistolPanic → EconomyConfig) and assign on YgProjectLifetimeScope in Bootstrap scene.");

                return;
            }

            builder.RegisterInstance(_economyConfig);
            builder.Register<ProgressService>(Lifetime.Singleton);
            builder.Register<EconomyService>(Lifetime.Singleton);
        }
    }
}
