using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace PistolPanic.Presentation
{
    public sealed class ShopLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            Debug.Log("Scope: Configure ShopLifetimeScope");

            builder.RegisterComponentInHierarchy<SceneSwapDebugView>();
        }
    }
}
