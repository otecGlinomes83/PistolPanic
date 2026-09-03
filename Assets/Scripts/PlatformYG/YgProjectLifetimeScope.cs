using PistolPanic.Core;
using UnityEngine;
using VContainer;

namespace PistolPanic.Presentation
{
    public sealed class YgProjectLifetimeScope : ProjectLifetimeScope
    {
        protected override void ConfigurePlatformServices(IContainerBuilder builder)
        {
            Debug.Log("Scope: platform services (YG)");

            builder.Register<YgLifecycleAdapter>(Lifetime.Singleton).As<IPlatformLifecycle>();
        }
    }
}
