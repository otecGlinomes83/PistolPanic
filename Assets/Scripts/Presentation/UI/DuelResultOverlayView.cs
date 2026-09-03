using System;
using Cysharp.Threading.Tasks;
using PistolPanic.Core;
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
        private readonly DuelSim _duelSim = null;

        [Inject]
        private readonly ISceneLoader _sceneLoader = null;

        private GameObject _overlayRoot;

        private TMP_Text _titleText;

        private TMP_Text _buttonText;

        private void Start()
        {
            CreateOverlay();
            _overlayRoot.SetActive(false);

            _duelSim.DuelPhaseChanged += OnDuelPhaseChanged;

            Debug.Log("DuelResultOverlay: created");
        }

        private void OnDestroy()
        {
            _duelSim.DuelPhaseChanged -= OnDuelPhaseChanged;
        }

        private void OnDuelPhaseChanged(DuelPhase phase)
        {
            if (phase == DuelPhase.Victory)
            {
                ShowOverlay("VICTORY", "NEXT");
            }
            else if (phase == DuelPhase.Defeat)
            {
                ShowOverlay("DEFEAT", "RETRY");
            }
        }

        private void ShowOverlay(string title, string buttonText)
        {
            _titleText.text = title;
            _buttonText.text = buttonText;
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
            panelImage.color = new Color(0f, 0f, 0f, 0.75f);

            _titleText = CreateText(panelObject.transform, "TitleText");
            SetAnchors(_titleText.rectTransform, new Vector2(0.5f, 0.68f), new Vector2(0.5f, 0.68f));
            _titleText.rectTransform.sizeDelta = new Vector2(900f, 160f);
            _titleText.fontSize = 110f;
            _titleText.alignment = TextAlignmentOptions.Center;

            GameObject buttonObject = new GameObject("RestartButton", typeof(RectTransform));
            buttonObject.transform.SetParent(panelObject.transform, false);

            RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.5f, 0.3f);
            buttonRect.anchorMax = new Vector2(0.5f, 0.3f);
            buttonRect.pivot = new Vector2(0.5f, 0.5f);
            buttonRect.sizeDelta = new Vector2(420f, 140f);

            Image buttonImage = buttonObject.AddComponent<Image>();
            buttonImage.color = new Color(0.2f, 0.55f, 0.9f, 1f);

            Button restartButton = buttonObject.AddComponent<Button>();
            restartButton.onClick.AddListener(OnRestartButtonClicked);

            _buttonText = CreateText(buttonObject.transform, "ButtonText");
            SetAnchors(_buttonText.rectTransform, Vector2.zero, Vector2.one);
            _buttonText.rectTransform.offsetMin = Vector2.zero;
            _buttonText.rectTransform.offsetMax = Vector2.zero;
            _buttonText.fontSize = 60f;
            _buttonText.alignment = TextAlignmentOptions.Center;
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

        private void OnRestartButtonClicked()
        {
            _overlayRoot.SetActive(false);
            RestartDuelAsync().Forget();
        }

        private async UniTaskVoid RestartDuelAsync()
        {
            try
            {
                await _sceneLoader.Load(SceneNames.Game);
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }
    }
}
