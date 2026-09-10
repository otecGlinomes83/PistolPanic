using System;
using Cysharp.Threading.Tasks;
using PistolPanic.Core;
using PistolPanic.Meta;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace PistolPanic.Presentation
{
    public sealed class DuelResultOverlayView : MonoBehaviour
    {
        private const int OverlaySortingOrder = 500;

        [Inject]
        private readonly ISceneLoader _sceneLoader = null;

        [Inject]
        private readonly MatchResultApplier _matchResultApplier = null;

        [Inject]
        private readonly AudioService _audioService = null;

        private GameObject _overlayRoot;

        private TMP_Text _titleText;

        private TMP_Text _rewardText;

        private TMP_Text _buttonText;

        private string _nextSceneName = SceneNames.Game;

        private void Start()
        {
            CreateOverlay();
            _overlayRoot.SetActive(false);

            _matchResultApplier.MatchFinished += OnMatchFinished;
        }

        private void OnDestroy()
        {
            _matchResultApplier.MatchFinished -= OnMatchFinished;
        }

        private void OnMatchFinished(MatchResult result)
        {
            if (result.Phase == DuelPhase.Victory)
            {
                _rewardText.gameObject.SetActive(true);
                _rewardText.text = "+" + result.Reward;
                _audioService.PlaySfx(SfxType.Victory);

                ShowOverlay("VICTORY", "SHOP", SceneNames.Shop);
            }
            else if (result.Phase == DuelPhase.Defeat)
            {
                _rewardText.gameObject.SetActive(false);
                _audioService.PlaySfx(SfxType.Defeat);

                ShowOverlay("DEFEAT", "SHOP", SceneNames.Shop);
            }
        }

        private void ShowOverlay(string title, string buttonText, string nextSceneName)
        {
            _titleText.text = title;
            _buttonText.text = buttonText;
            _nextSceneName = nextSceneName;
            _overlayRoot.SetActive(true);
        }

        private void CreateOverlay()
        {
            _overlayRoot = new GameObject("DuelResultOverlay");

            Canvas canvas = _overlayRoot.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = OverlaySortingOrder;

            _overlayRoot.AddComponent<GraphicRaycaster>();

            GameObject panelObject = new GameObject("Panel", typeof(RectTransform));
            panelObject.transform.SetParent(_overlayRoot.transform, false);

            RectTransform panelRect = panelObject.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            Image panelImage = panelObject.AddComponent<Image>();
            panelImage.color = new Color(0.05f, 0.07f, 0.12f, 0.85f);

            _titleText = CreateText(panelObject.transform, "TitleText");
            SetAnchors(_titleText.rectTransform, new Vector2(0.5f, 0.68f), new Vector2(0.5f, 0.68f));
            _titleText.rectTransform.sizeDelta = new Vector2(900f, 160f);
            _titleText.fontSize = 110f;
            _titleText.alignment = TextAlignmentOptions.Center;

            _rewardText = CreateText(panelObject.transform, "RewardText");
            SetAnchors(_rewardText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            _rewardText.rectTransform.sizeDelta = new Vector2(600f, 120f);
            _rewardText.fontSize = 90f;
            _rewardText.alignment = TextAlignmentOptions.Center;
            _rewardText.color = new Color(1f, 0.85f, 0.3f, 1f);

            GameObject buttonObject = new GameObject("ContinueButton", typeof(RectTransform));
            buttonObject.transform.SetParent(panelObject.transform, false);

            RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.5f, 0.3f);
            buttonRect.anchorMax = new Vector2(0.5f, 0.3f);
            buttonRect.pivot = new Vector2(0.5f, 0.5f);
            buttonRect.sizeDelta = new Vector2(420f, 140f);

            Image buttonImage = buttonObject.AddComponent<Image>();
            buttonImage.color = new Color(0.95f, 0.65f, 0.15f, 1f);

            Button continueButton = buttonObject.AddComponent<Button>();
            continueButton.onClick.AddListener(OnContinueButtonClicked);

            _buttonText = CreateText(buttonObject.transform, "ButtonText");
            SetAnchors(_buttonText.rectTransform, Vector2.zero, Vector2.one);
            _buttonText.rectTransform.offsetMin = Vector2.zero;
            _buttonText.rectTransform.offsetMax = Vector2.zero;
            _buttonText.fontSize = 60f;
            _buttonText.alignment = TextAlignmentOptions.Center;
            _buttonText.color = Color.black;
            _buttonText.fontStyle = FontStyles.Bold;
        }

        private TMP_Text CreateText(Transform parentTransform, string textObjectName)
        {
            GameObject textObject = new GameObject(textObjectName, typeof(RectTransform));
            textObject.transform.SetParent(parentTransform, false);

            TMP_Text text = textObject.AddComponent<TextMeshProUGUI>();
            text.color = Color.white;

            return text;
        }

        private void SetAnchors(RectTransform rectTransform, Vector2 anchorMin, Vector2 anchorMax)
        {
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
        }

        private void OnContinueButtonClicked()
        {
            _audioService.PlaySfx(SfxType.UiClick);
            _overlayRoot.SetActive(false);
            LoadNextSceneAsync().Forget();
        }

        private async UniTaskVoid LoadNextSceneAsync()
        {
            try
            {
                await _sceneLoader.Load(_nextSceneName);
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }
    }
}
