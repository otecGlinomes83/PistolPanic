using System;
using Cysharp.Threading.Tasks;
using PistolPanic.Core;
using UnityEngine;
using VContainer;

namespace PistolPanic.Presentation
{
    public sealed class Bootstrapper : MonoBehaviour
    {
        [Inject]
        private readonly SimulationConfig _simulationConfig = null;

        [Inject]
        private readonly ISceneLoader _sceneLoader = null;

        [Inject]
        private readonly IPlatformLifecycle _platformLifecycle = null;

        private void Start()
        {
            Debug.Log("Bootstrapper: Start, sceneLoader=" + (_sceneLoader != null) + " platform=" + (_platformLifecycle != null));

            Application.targetFrameRate = _simulationConfig.TargetFrameRate;

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
            catch (TimeoutException exception)
            {
                Debug.LogError("Bootstrap: YG SDK was not enabled within the timeout " + exception.Message);

                return;
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }
    }
}
