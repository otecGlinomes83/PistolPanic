using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using PistolPanic.Core;
using PistolPanic.Meta;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VContainer;

namespace PistolPanic.Presentation
{
    public sealed class ShopWeaponsView : MonoBehaviour
    {
        private const int CanvasSortingOrder = 200;

        private const float CardWidth = 600f;

        private const float CardHeight = 180f;

        private const float CardSpacing = 200f;

        private const float CardBorderThickness = 8f;

        private const float CardTopOffset = 200f;

        private readonly Color _cardBackgroundColor = new Color(0.08f, 0.1f, 0.16f, 1f);

        private readonly Color _borderEquippedColor = new Color(0.95f, 0.65f, 0.15f, 1f);

        private readonly Color _borderDimColor = new Color(0.25f, 0.28f, 0.36f, 1f);

        [Inject]
        private readonly WeaponCatalog _weaponCatalog = null;

        [Inject]
        private readonly WeaponLoadoutService _weaponLoadoutService = null;

        [Inject]
        private readonly ISceneLoader _sceneLoader = null;

        [Inject]
        private readonly AudioService _audioService = null;

        [Inject]
        private readonly EconomyService _economyService = null;

        [Inject]
        private readonly SpriteFactory _spriteFactory = null;

        private GameObject[] _cardObjects;

        private Image[] _cardBorderImages;

        private TMP_Text _moneyText;

        private void Start()
        {
            CreateView();

            _economyService.MoneyChanged += OnMoneyChanged;
            _weaponLoadoutService.EquippedChanged += OnEquippedChanged;
        }

        private void OnDestroy()
        {
            _economyService.MoneyChanged -= OnMoneyChanged;
            _weaponLoadoutService.EquippedChanged -= OnEquippedChanged;
        }

        private void CreateView()
        {
            GameObject canvasObject = new GameObject("ShopWeaponsCanvas");
            canvasObject.transform.SetParent(transform, false);

            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = CanvasSortingOrder;

            canvasObject.AddComponent<GraphicRaycaster>();

            TMP_Text titleText = CreateText(canvasObject.transform, "TitleText", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
            titleText.rectTransform.anchoredPosition = new Vector2(0f, -50f);
            titleText.rectTransform.sizeDelta = new Vector2(800f, 110f);
            titleText.fontSize = 84f;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.text = "АРСЕНАЛ";

            _moneyText = CreateText(canvasObject.transform, "MoneyText", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
            _moneyText.rectTransform.anchoredPosition = new Vector2(0f, -160f);
            _moneyText.rectTransform.sizeDelta = new Vector2(600f, 70f);
            _moneyText.fontSize = 48f;
            _moneyText.alignment = TextAlignmentOptions.Center;
            _moneyText.color = new Color(1f, 0.85f, 0.3f, 1f);
            _moneyText.text = "МОНЕТЫ " + _economyService.Money;

            IReadOnlyList<WeaponConfig> weapons = _weaponCatalog.Weapons;
            _cardObjects = new GameObject[weapons.Count];
            _cardBorderImages = new Image[weapons.Count];

            for (int i = 0; i < weapons.Count; i++)
            {
                CreateCard(canvasObject.transform, i, weapons[i]);
            }

            CreateBattleButton(canvasObject.transform);
            RefreshHighlights();
        }

        private void CreateCard(Transform parentTransform, int cardIndex, WeaponConfig weaponConfig)
        {
            GameObject cardObject = new GameObject("WeaponCard" + cardIndex, typeof(RectTransform));
            cardObject.transform.SetParent(parentTransform, false);

            RectTransform cardRect = cardObject.GetComponent<RectTransform>();
            cardRect.anchorMin = new Vector2(0.5f, 1f);
            cardRect.anchorMax = new Vector2(0.5f, 1f);
            cardRect.pivot = new Vector2(0.5f, 1f);
            cardRect.sizeDelta = new Vector2(CardWidth, CardHeight);
            cardRect.anchoredPosition = new Vector2(0f, -CardTopOffset - cardIndex * CardSpacing);

            Image borderImage = cardObject.AddComponent<Image>();
            borderImage.sprite = _spriteFactory.GetPanelSprite();
            borderImage.type = Image.Type.Sliced;
            borderImage.color = _borderDimColor;

            Button cardButton = cardObject.AddComponent<Button>();
            cardButton.onClick.AddListener(OnAnyCardClicked);

            _cardObjects[cardIndex] = cardObject;
            _cardBorderImages[cardIndex] = borderImage;

            CreateCardBackground(cardObject.transform);
            CreateCardContent(cardObject.transform, weaponConfig);
        }

        private void CreateCardBackground(Transform cardTransform)
        {
            GameObject backgroundObject = new GameObject("CardBackground", typeof(RectTransform));
            backgroundObject.transform.SetParent(cardTransform, false);

            RectTransform backgroundRect = backgroundObject.GetComponent<RectTransform>();
            backgroundRect.anchorMin = Vector2.zero;
            backgroundRect.anchorMax = Vector2.one;
            backgroundRect.pivot = new Vector2(0.5f, 0.5f);
            backgroundRect.offsetMin = new Vector2(CardBorderThickness, CardBorderThickness);
            backgroundRect.offsetMax = new Vector2(-CardBorderThickness, -CardBorderThickness);

            Image backgroundImage = backgroundObject.AddComponent<Image>();
            backgroundImage.color = _cardBackgroundColor;
            backgroundImage.raycastTarget = false;
        }

        private void CreateCardContent(Transform cardTransform, WeaponConfig weaponConfig)
        {
            TMP_Text nameText = CreateText(cardTransform, "WeaponNameText", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));
            nameText.rectTransform.anchoredPosition = new Vector2(30f, -24f);
            nameText.rectTransform.sizeDelta = new Vector2(330f, 70f);
            nameText.fontSize = 56f;
            nameText.alignment = TextAlignmentOptions.Left;
            nameText.text = weaponConfig.DisplayName;

            TMP_Text descriptionText = CreateText(cardTransform, "WeaponDescriptionText", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));
            descriptionText.rectTransform.anchoredPosition = new Vector2(30f, -100f);
            descriptionText.rectTransform.sizeDelta = new Vector2(330f, 50f);
            descriptionText.fontSize = 34f;
            descriptionText.alignment = TextAlignmentOptions.Left;
            descriptionText.color = new Color(0.75f, 0.78f, 0.85f, 1f);
            descriptionText.text = BuildWeaponDescription(weaponConfig);

            GameObject previewObject = new GameObject("WeaponPreview", typeof(RectTransform));
            previewObject.transform.SetParent(cardTransform, false);

            RectTransform previewRect = previewObject.GetComponent<RectTransform>();
            previewRect.anchorMin = new Vector2(1f, 0.5f);
            previewRect.anchorMax = new Vector2(1f, 0.5f);
            previewRect.pivot = new Vector2(1f, 0.5f);
            previewRect.sizeDelta = new Vector2(220f, 110f);
            previewRect.anchoredPosition = new Vector2(-30f, 0f);

            Image previewImage = previewObject.AddComponent<Image>();
            previewImage.sprite = _spriteFactory.GetGunSprite(weaponConfig.FirePattern, GunSkin.PlayerSteel);
            previewImage.preserveAspect = true;
            previewImage.raycastTarget = false;
        }

        private void CreateBattleButton(Transform parentTransform)
        {
            GameObject buttonObject = new GameObject("BattleButton", typeof(RectTransform));
            buttonObject.transform.SetParent(parentTransform, false);

            RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.5f, 0f);
            buttonRect.anchorMax = new Vector2(0.5f, 0f);
            buttonRect.pivot = new Vector2(0.5f, 0f);
            buttonRect.sizeDelta = new Vector2(600f, 160f);
            buttonRect.anchoredPosition = new Vector2(0f, 120f);

            Image buttonImage = buttonObject.AddComponent<Image>();
            buttonImage.color = new Color(0.95f, 0.65f, 0.15f, 1f);

            Button battleButton = buttonObject.AddComponent<Button>();
            battleButton.onClick.AddListener(OnBattleButtonClicked);

            TMP_Text buttonText = CreateText(buttonObject.transform, "ButtonText", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
            buttonText.rectTransform.offsetMin = Vector2.zero;
            buttonText.rectTransform.offsetMax = Vector2.zero;
            buttonText.fontSize = 64f;
            buttonText.alignment = TextAlignmentOptions.Center;
            buttonText.color = Color.black;
            buttonText.fontStyle = FontStyles.Bold;
            buttonText.text = "В БОЙ";
        }

        private string BuildWeaponDescription(WeaponConfig weaponConfig)
        {
            int patternValue = (int)weaponConfig.FirePattern;

            if (patternValue == (int)FirePattern.Burst)
            {
                return "ОЧЕРЕДЬ " + weaponConfig.BurstCount;
            }

            if (patternValue == (int)FirePattern.Shotgun)
            {
                return "ВЕЕР " + weaponConfig.PelletCount + " ДРОБИН";
            }

            return "ОДИН ВЫСТРЕЛ";
        }

        private void OnAnyCardClicked()
        {
            GameObject selectedObject = EventSystem.current.currentSelectedGameObject;

            if (selectedObject == null)
            {
                return;
            }

            for (int i = 0; i < _cardObjects.Length; i++)
            {
                if (_cardObjects[i] == selectedObject)
                {
                    _weaponLoadoutService.Equip(i);
                    _audioService.PlaySfx(SfxType.UiClick);
                    RefreshHighlights();

                    return;
                }
            }
        }

        private void OnBattleButtonClicked()
        {
            _audioService.PlaySfx(SfxType.UiClick);
            LoadBattleSceneAsync().Forget();
        }

        private void OnEquippedChanged(int equippedIndex)
        {
            RefreshHighlights();
        }

        private void OnMoneyChanged(int money)
        {
            _moneyText.text = "МОНЕТЫ " + money;
        }

        private void RefreshHighlights()
        {
            for (int i = 0; i < _cardBorderImages.Length; i++)
            {
                if (_weaponLoadoutService.HasSelection && _weaponLoadoutService.EquippedIndex == i)
                {
                    _cardBorderImages[i].color = _borderEquippedColor;
                }
                else
                {
                    _cardBorderImages[i].color = _borderDimColor;
                }
            }
        }

        private TMP_Text CreateText(Transform parentTransform, string textObjectName, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot)
        {
            GameObject textObject = new GameObject(textObjectName, typeof(RectTransform));
            textObject.transform.SetParent(parentTransform, false);

            RectTransform rectTransform = textObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.pivot = pivot;

            TMP_Text text = textObject.AddComponent<TextMeshProUGUI>();
            text.raycastTarget = false;
            text.color = Color.white;

            return text;
        }

        private async UniTaskVoid LoadBattleSceneAsync()
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
