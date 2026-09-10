using PistolPanic.Core;
using PistolPanic.Meta;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace PistolPanic.Presentation
{
    public sealed class YgProjectLifetimeScope : ProjectLifetimeScope
    {
        [SerializeField]
        private EconomyConfig _economyConfig = null;

        [SerializeField]
        private AudioConfig _audioConfig = null;

        protected override void ConfigurePlatformServices(IContainerBuilder builder)
        {
            Debug.Log("Scope: platform services (YG)");

            builder.Register<YgLifecycleAdapter>(Lifetime.Singleton).As<IPlatformLifecycle>();
            builder.Register<SpriteFactory>(Lifetime.Singleton);

            if (_economyConfig == null)
            {
                throw new VContainerException(typeof(EconomyConfig), "YgProjectLifetimeScope: EconomyConfig is not assigned. Create asset (Create → PistolPanic → EconomyConfig) and assign on YgProjectLifetimeScope in Bootstrap scene.");
            }

            builder.RegisterInstance(_economyConfig);
            builder.Register<ProgressService>(Lifetime.Singleton);
            builder.Register<EconomyService>(Lifetime.Singleton);

            RegisterAudioConfig(builder);
            builder.RegisterComponentOnNewGameObject<AudioService>(Lifetime.Singleton, "AudioService").DontDestroyOnLoad();
            builder.RegisterBuildCallback(ActivateAudioService);
        }

        private void RegisterAudioConfig(IContainerBuilder builder)
        {
            if (_audioConfig != null)
            {
                builder.RegisterInstance(_audioConfig);

                return;
            }

            Debug.LogError("YgProjectLifetimeScope: AudioConfig is not assigned. Create asset (Create → PistolPanic → AudioConfig) in Assets/Configs and assign on YgProjectLifetimeScope in Bootstrap scene.");

            builder.RegisterInstance(ScriptableObject.CreateInstance<AudioConfig>());
        }

        private void ActivateAudioService(IObjectResolver resolver)
        {
            resolver.Resolve<AudioService>();
        }
    }
}
