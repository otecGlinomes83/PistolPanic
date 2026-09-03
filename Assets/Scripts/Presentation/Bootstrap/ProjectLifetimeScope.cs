using PistolPanic.Core;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace PistolPanic.Presentation
{
    public class ProjectLifetimeScope : LifetimeScope
    {
        protected sealed override void Configure(IContainerBuilder builder)
        {
            Debug.Log("Scope: Configure " + GetType().Name);

            builder.RegisterComponentInHierarchy<Bootstrapper>();

            builder.Register<SceneLoader>(Lifetime.Singleton).As<ISceneLoader>();

            ConfigurePlatformServices(builder);
        }

        protected virtual void ConfigurePlatformServices(IContainerBuilder builder)
        {
        }
    }
}
