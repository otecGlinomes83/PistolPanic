using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace PistolPanic.Presentation
{
    public sealed class ShopLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            if (Parent == null)
            {
                Debug.LogError("ShopLifetimeScope: parent project scope is missing. Scenes must be loaded through SceneLoader from Bootstrap.");

                return;
            }

            Debug.Log("Scope: Configure ShopLifetimeScope");

            builder.RegisterComponentInHierarchy<SceneSwapDebugView>();
        }
    }
}
