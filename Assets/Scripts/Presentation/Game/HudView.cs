using System.Collections.Generic;
using PistolPanic.Core;
using PistolPanic.Meta;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace PistolPanic.Presentation
{
    public sealed class HudView : MonoBehaviour
    {
        private const int CanvasSortingOrder = 100;

        private const int MaxAmmoPips = 10;

        private const float AmmoPipWidth = 20f;

        private const float AmmoPipHeight = 40f;

        private const float AmmoPipSpacing = 28f;

        private const float StagePipSize = 24f;

        private const float StagePipSpacing = 32f;

        private readonly Color _ammoFullColor = new Color(1f, 0.85f, 0.3f, 1f);

        private readonly Color _stagePipFallbackColor = new Color(0.9f, 0.9f, 0.9f, 1f);

        [Inject]
        private readonly FireSystem _fireSystem = null;

        [Inject]
        private readonly DuelFlow _duelFlow = null;

        [Inject]
        private readonly ProgressService _progressService = null;

        [Inject]
        private readonly EconomyService _economyService = null;

        [Inject]
        private readonly PlayerWeaponProvider _playerWeaponProvider = null;

        [Inject]
        private readonly EnemyTypeProvider _enemyTypeProvider = null;

        [Inject]
        private readonly EnemyConfig _enemyConfig = null;

        [Inject]
        private readonly AudioService _audioService = null;

        private TMP_Text _levelText;

        private TMP_Text _moneyText;

        private TMP_Text _weaponNameText;

        private TMP_Text _reloadText;

        private TMP_Text _phaseMessageText;

        private readonly Image[] _ammoPips = new Image[MaxAmmoPips];

        private Image[] _stagePips;

        private bool _wasFullReloading;

        private void Start()
        {
            CreateHUD();
            PullInitialState();

            _fireSystem.PlayerAmmoChanged += OnAmmoChanged;
            _duelFlow.PhaseChanged += OnPhaseChanged;
            _progressService.LevelChanged += OnLevelChanged;
            _economyService.MoneyChanged += OnMoneyChanged;
        }

        private void OnDestroy()
        {
            _fireSystem.PlayerAmmoChanged -= OnAmmoChanged;
            _duelFlow.PhaseChanged -= OnPhaseChanged;
            _progressService.LevelChanged -= OnLevelChanged;
            _economyService.MoneyChanged -= OnMoneyChanged;
        }

        private void CreateHUD()
        {
            GameObject canvasObject = new GameObject("HudCanvas");
            canvasObject.transform.SetParent(transform, false);

            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = CanvasSortingOrder;

            canvasObject.AddComponent<GraphicRaycaster>();

            _levelText = CreateText(canvasObject.transform, "LevelText", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));
            _levelText.rectTransform.anchoredPosition = new Vector2(40f, -40f);
            _levelText.rectTransform.sizeDelta = new Vector2(560f, 70f);
            _levelText.fontSize = 46f;
            _levelText.alignment = TextAlignmentOptions.Left;
            _levelText.text = "УРОВЕНЬ " + _progressService.CurrentLevel;

            _moneyText = CreateText(canvasObject.transform, "MoneyText", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));
            _moneyText.rectTransform.anchoredPosition = new Vector2(40f, -120f);
            _moneyText.rectTransform.sizeDelta = new Vector2(560f, 70f);
            _moneyText.fontSize = 46f;
            _moneyText.alignment = TextAlignmentOptions.Left;
            _moneyText.color = new Color(1f, 0.85f, 0.3f, 1f);
            _moneyText.text = "МОНЕТЫ " + _economyService.Money;

            CreateStagePips(canvasObject.transform);
            CreateWeaponPanel(canvasObject.transform);

            _phaseMessageText = CreateText(canvasObject.transform, "PhaseMessageText", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            _phaseMessageText.rectTransform.anchoredPosition = Vector2.zero;
            _phaseMessageText.rectTransform.sizeDelta = new Vector2(900f, 140f);
            _phaseMessageText.fontSize = 84f;
            _phaseMessageText.alignment = TextAlignmentOptions.Center;
        }

        private void CreateStagePips(Transform parentTransform)
        {
            int stageCount = _enemyConfig.StageHealths.Count;
            _stagePips = new Image[stageCount];

            for (int i = 0; i < stageCount; i++)
            {
                Image stagePip = CreatePipImage(parentTransform, "StagePip" + i, new Vector2(1f, 1f), new Vector2(1f, 1f));
                stagePip.rectTransform.sizeDelta = new Vector2(StagePipSize, StagePipSize);
                stagePip.rectTransform.anchoredPosition = new Vector2(-40f - i * StagePipSpacing, -48f);
                stagePip.color = ResolveStagePipColor(i);
                _stagePips[i] = stagePip;
            }
        }

        private void CreateWeaponPanel(Transform parentTransform)
        {
            _weaponNameText = CreateText(parentTransform, "WeaponNameText", new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f));
            _weaponNameText.rectTransform.anchoredPosition = new Vector2(40f, 210f);
            _weaponNameText.rectTransform.sizeDelta = new Vector2(700f, 60f);
            _weaponNameText.fontSize = 44f;
            _weaponNameText.alignment = TextAlignmentOptions.Left;
            _weaponNameText.text = _playerWeaponProvider.CurrentWeaponConfig.DisplayName;

            for (int i = 0; i < _ammoPips.Length; i++)
            {
                Image ammoPip = CreatePipImage(parentTransform, "AmmoPip" + i, new Vector2(0f, 0f), new Vector2(0f, 0f));
                ammoPip.rectTransform.sizeDelta = new Vector2(AmmoPipWidth, AmmoPipHeight);
                ammoPip.rectTransform.anchoredPosition = new Vector2(40f + i * AmmoPipSpacing, 140f);
                ammoPip.color = _ammoFullColor;
                ammoPip.gameObject.SetActive(false);
                _ammoPips[i] = ammoPip;
            }

            _reloadText = CreateText(parentTransform, "ReloadText", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f));
            _reloadText.rectTransform.anchoredPosition = new Vector2(0f, 260f);
            _reloadText.rectTransform.sizeDelta = new Vector2(700f, 80f);
            _reloadText.fontSize = 54f;
            _reloadText.alignment = TextAlignmentOptions.Center;
            _reloadText.color = new Color(1f, 0.85f, 0.3f, 1f);
            _reloadText.text = "ПЕРЕЗАРЯДКА";
            _reloadText.gameObject.SetActive(false);
        }

        private void PullInitialState()
        {
            int magazineSize = _playerWeaponProvider.CurrentWeaponConfig.MagazineSize;

            for (int i = 0; i < _ammoPips.Length; i++)
            {
                if (i < magazineSize)
                {
                    _ammoPips[i].gameObject.SetActive(true);
                    _ammoPips[i].color = _ammoFullColor;
                }
            }

            ApplyPhaseMessage(_duelFlow.Phase);
        }

        private void OnAmmoChanged(AmmoSnapshot snapshot)
        {
            UpdateAmmoPips(snapshot);
            UpdateReloadText(snapshot);

            if (snapshot.IsFullReloading && _wasFullReloading == false)
            {
                _audioService.PlaySfx(SfxType.Reload);
            }

            _wasFullReloading = snapshot.IsFullReloading;
        }

        private void OnPhaseChanged(DuelPhase phase)
        {
            ApplyPhaseMessage(phase);
        }

        private void OnLevelChanged(int currentLevel)
        {
            _levelText.text = "УРОВЕНЬ " + currentLevel;
        }

        private void OnMoneyChanged(int money)
        {
            _moneyText.text = "МОНЕТЫ " + money;
        }

        private void ApplyPhaseMessage(DuelPhase phase)
        {
            if (phase == DuelPhase.Armed || phase == DuelPhase.Grace)
            {
                _phaseMessageText.gameObject.SetActive(true);
                _phaseMessageText.text = "ПРИГОТОВЬСЯ";

                return;
            }

            _phaseMessageText.gameObject.SetActive(false);
        }

        private void UpdateAmmoPips(AmmoSnapshot snapshot)
        {
            for (int i = 0; i < _ammoPips.Length; i++)
            {
                Image ammoPip = _ammoPips[i];

                if (i < snapshot.Magazine)
                {
                    ammoPip.gameObject.SetActive(true);
                    ammoPip.color = _ammoFullColor;

                    continue;
                }

                if (snapshot.IsFullReloading == false && i == snapshot.Magazine)
                {
                    Color regenColor = _ammoFullColor;
                    regenColor.a = Mathf.Clamp01(snapshot.RegenProgress01);

                    ammoPip.gameObject.SetActive(true);
                    ammoPip.color = regenColor;

                    continue;
                }

                ammoPip.gameObject.SetActive(false);
            }
        }

        private void UpdateReloadText(AmmoSnapshot snapshot)
        {
            if (snapshot.IsFullReloading == false)
            {
                _reloadText.gameObject.SetActive(false);

                return;
            }

            Color reloadColor = _reloadText.color;
            reloadColor.a = Mathf.Clamp01(snapshot.ReloadProgress01);

            _reloadText.gameObject.SetActive(true);
            _reloadText.color = reloadColor;
        }

        private Color ResolveStagePipColor(int stageIndex)
        {
            IReadOnlyList<Color> stageColors = _enemyTypeProvider.Current.StageColors;

            if (stageIndex < 0 || stageIndex >= stageColors.Count)
            {
                return _stagePipFallbackColor;
            }

            return stageColors[stageIndex];
        }

        private Image CreatePipImage(Transform parentTransform, string objectName, Vector2 anchor, Vector2 pivot)
        {
            GameObject imageObject = new GameObject(objectName, typeof(RectTransform));
            imageObject.transform.SetParent(parentTransform, false);

            RectTransform rectTransform = imageObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = anchor;
            rectTransform.anchorMax = anchor;
            rectTransform.pivot = pivot;

            Image image = imageObject.AddComponent<Image>();
            image.raycastTarget = false;

            return image;
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
    }
}
