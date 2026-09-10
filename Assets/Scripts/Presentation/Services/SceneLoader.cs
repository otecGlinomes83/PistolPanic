using Cysharp.Threading.Tasks;
using PistolPanic.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using VContainer.Unity;

namespace PistolPanic.Presentation
{
    public sealed class SceneLoader : ISceneLoader
    {
        private const float FadeDurationSeconds = 0.3f;

        private const int FadeCanvasSortingOrder = 10000;

        private readonly LifetimeScope _lifetimeScope;

        private CanvasGroup _fadeCanvasGroup;

        private bool _isLoading;

        public SceneLoader(LifetimeScope lifetimeScope)
        {
            _lifetimeScope = lifetimeScope;
        }

        public async UniTask Load(string sceneName)
        {
            if (_isLoading)
            {
                return;
            }

            _isLoading = true;

            try
            {
                Debug.Log("SceneLoader: fade in, loading '" + sceneName + "'");

                CanvasGroup fadeCanvasGroup = GetOrCreateFadeCanvas();

                fadeCanvasGroup.blocksRaycasts = true;

                await FadeAsync(fadeCanvasGroup, 0f, 1f);

                using (LifetimeScope.EnqueueParent(_lifetimeScope))
                {
                    await SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
                }

                await FadeAsync(fadeCanvasGroup, 1f, 0f);

                fadeCanvasGroup.blocksRaycasts = false;

                Debug.Log("SceneLoader: scene '" + sceneName + "' ready, fade out done");
            }
            finally
            {
                _isLoading = false;
            }
        }

        private async UniTask FadeAsync(CanvasGroup canvasGroup, float fromAlpha, float toAlpha)
        {
            float elapsedTime = 0f;

            while (elapsedTime < FadeDurationSeconds)
            {
                elapsedTime += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Lerp(fromAlpha, toAlpha, elapsedTime / FadeDurationSeconds);

                await UniTask.Yield(PlayerLoopTiming.Update);
            }

            canvasGroup.alpha = toAlpha;
        }

        private CanvasGroup GetOrCreateFadeCanvas()
        {
            if (_fadeCanvasGroup != null)
            {
                return _fadeCanvasGroup;
            }

            GameObject fadeRoot = new GameObject("SceneFadeCanvas");
            fadeRoot.transform.SetParent(_lifetimeScope.transform);

            Canvas canvas = fadeRoot.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = FadeCanvasSortingOrder;

            CanvasGroup canvasGroup = fadeRoot.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            Image fadeImage = fadeRoot.AddComponent<Image>();
            fadeImage.color = Color.black;

            _fadeCanvasGroup = canvasGroup;

            return _fadeCanvasGroup;
        }
    }
}
