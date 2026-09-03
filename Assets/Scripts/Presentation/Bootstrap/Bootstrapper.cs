using System;
using Cysharp.Threading.Tasks;
using PistolPanic.Core;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace PistolPanic.Presentation
{
    public sealed class Bootstrapper : MonoBehaviour
    {
        private const int TargetFrameRate = 60;

        [Inject]
        private readonly ISceneLoader _sceneLoader = null;

        [Inject]
        private readonly IPlatformLifecycle _platformLifecycle = null;

        [Inject]
        private readonly LifetimeScope _projectLifetimeScope = null;

        private void Start()
        {
            Debug.Log("Bootstrapper: Start, sceneLoader=" + (_sceneLoader != null) + " platform=" + (_platformLifecycle != null) + " scope=" + (_projectLifetimeScope != null));

            Application.targetFrameRate = TargetFrameRate;

            DontDestroyOnLoad(_projectLifetimeScope.gameObject);

            RunBootFlowAsync().Forget();
        }

        private async UniTaskVoid RunBootFlowAsync()
        {
            try
            {
                Debug.Log("Bootstrapper: waiting platform data");

                await _platformLifecycle.WaitForDataLoadedAsync();

                Debug.Log("Bootstrapper: platform data loaded, sending ready");

                _platformLifecycle.MarkReady();

                await _sceneLoader.Load(SceneNames.Game);
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }
    }
}
