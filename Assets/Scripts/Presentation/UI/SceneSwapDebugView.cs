using System;
using Cysharp.Threading.Tasks;
using PistolPanic.Core;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace PistolPanic.Presentation
{
    public sealed class SceneSwapDebugView : MonoBehaviour
    {
        [Inject]
        private readonly ISceneLoader _sceneLoader = null;

        [Inject]
        private readonly AudioService _audioService = null;

        [SerializeField]
        private Button _swapButton = null;

        [SerializeField]
        private string _targetSceneName = null;

        private void Start()
        {
            if (_swapButton == null)
            {
                return;
            }

            _swapButton.onClick.AddListener(OnSwapButtonClicked);
        }

        private void OnDestroy()
        {
            if (_swapButton == null)
            {
                return;
            }

            _swapButton.onClick.RemoveListener(OnSwapButtonClicked);
        }

        private void OnSwapButtonClicked()
        {
            _audioService.PlaySfx(SfxType.UiClick);
            LoadTargetSceneAsync().Forget();
        }

        private async UniTaskVoid LoadTargetSceneAsync()
        {
            try
            {
                await _sceneLoader.Load(_targetSceneName);
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }
    }
}
