using System;
using System.Collections.Generic;
using System;
using System.Collections.Generic;
using _Game.CardWheel.Data;
using com.core.ui;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using _Game.CardWheel.UIs.TopZoneScroll;

namespace _Game.CardWheel.UIs
{
    public class CardWheelScreen : BaseScreen
    {
        [SerializeField] private CardWheelSpinner   cardWheelSpinner;
        [SerializeField] private CardWheelIndicator cardWheelIndicator;
        [SerializeField] private Button             spinButton;
        [SerializeField] private Button             leaveButton;
        [SerializeField] private TMP_Text           zoneText;
        [SerializeField] private Transform          rewardPanelContainer;
        [SerializeField] private RewardEntry        rewardEntryPrefab;
        [SerializeField] private ScrollSnap         scrollSnap;
        [SerializeField] private ZoneScrollView     zoneScrollView;

        [SerializeField] private float flyDuration  = 0.6f;
        [SerializeField] private float spinDuration = 3f;

        public event Action SpinClicked;
        public event Action LeaveClicked;

        private readonly List<RewardEntry> _rewardEntries = new();

#if UNITY_EDITOR
        private void OnValidate()
        {
            cardWheelSpinner   = GetComponentInChildren<CardWheelSpinner>();
            cardWheelIndicator = GetComponentInChildren<CardWheelIndicator>();
            scrollSnap         = GetComponentInChildren<ScrollSnap>();
            zoneScrollView     = GetComponentInChildren<ZoneScrollView>();
        }
#endif

        private void Awake()
        {
            spinButton.onClick.AddListener(OnSpinButtonClicked);
            leaveButton.onClick.AddListener(OnLeaveButtonClicked);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.R)) scrollSnap.NextScreen();
            if (Input.GetKeyDown(KeyCode.L)) scrollSnap.PreviousScreen();
            if (Input.GetKeyDown(KeyCode.B))
            {
                scrollSnap.UpdateListItemPositions();
                scrollSnap.UpdateListItemsSize();
            }
        }

        private void OnSpinButtonClicked()  => SpinClicked?.Invoke();
        private void OnLeaveButtonClicked() => LeaveClicked?.Invoke();

        public void SetupWheel(WheelTierConfig config, int currentZone)
        {
            cardWheelSpinner.Setup(config, currentZone);
            cardWheelIndicator.Setup(config);
        }

        public void SpinToIndex(int sliceIndex, Action onComplete) => cardWheelSpinner.SpinToIndex(sliceIndex, spinDuration, onComplete);
        public void UpdateZoneDisplay(int zone) => zoneText.text = $"Zone {zone}";

        public void RebuildRewardPanel(IReadOnlyList<AccumulatedReward> rewards)
        {
            foreach (var entry in _rewardEntries) Destroy(entry.gameObject);
            _rewardEntries.Clear();

            foreach (var reward in rewards)
            {
                var entry = Instantiate(rewardEntryPrefab, rewardPanelContainer);
                entry.Setup(reward.Icon, reward.Amount, reward.Id, reward.Label);
                _rewardEntries.Add(entry);
            }
        }

        public void AddRewardEntry(Sprite icon, int amount, string id, string label)
        {
            var entry = Instantiate(rewardEntryPrefab, rewardPanelContainer);
            entry.Setup(icon, amount, id, label);
            _rewardEntries.Add(entry);
        }

        public void PlayRewardAnimation(Vector3 sliceWorldPosition, Sprite sliceIcon, string rewardId, int addedAmount, Action onComplete)
        {
            var animationIcon = new GameObject("AnimationRewardIcon", typeof(Image));
            animationIcon.transform.SetParent(transform, false);

            var flyingImage = animationIcon.GetComponent<Image>();
            flyingImage.sprite = sliceIcon;
            flyingImage.SetNativeSize();
            flyingImage.transform.localScale *= 0.5f;

            var canvas = GetComponentInParent<Canvas>();
            Camera canvasCamera = canvas?.worldCamera;

            var startScreenPos = RectTransformUtility.WorldToScreenPoint(canvasCamera, sliceWorldPosition);
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)transform, startScreenPos, canvasCamera, out var localStart))
            {
                animationIcon.transform.localPosition = localStart;
            }

            var targetEntry = _rewardEntries.Find(e => e.Id == rewardId);
            if (targetEntry == null)
            {
                onComplete?.Invoke();
                Destroy(animationIcon);
                return;
            }

            RectTransform targetIconRect = targetEntry.IconRectTransform;

            var iconCorners = new Vector3[4];
            targetIconRect.GetWorldCorners(iconCorners);
            var iconCenterWorld = (iconCorners[0] + iconCorners[2]) * 0.5f;

            var seq = DOTween.Sequence();
            seq.Append(animationIcon.transform.DOMove(iconCenterWorld, flyDuration).SetEase(Ease.InBack));
            seq.Join(animationIcon.transform.DOScale(Vector3.one * 0.5f, flyDuration).SetEase(Ease.InBack));
            seq.OnComplete(() =>
            {
                Destroy(animationIcon);
                targetEntry.PlayAddAnimation(addedAmount, onComplete);
            });
        }

        public void SetSpinButtonInteractable(bool interactable) => spinButton.interactable = interactable;
        public void SetLeaveButtonInteractable(bool interactable) => leaveButton.interactable = interactable;
        public void ResetWheel() => cardWheelSpinner.ResetRotation();

        public void SetupZoneBar(IList<ZoneItemData> items)
        {
            if (zoneScrollView != null) zoneScrollView.UpdateData(items);
        }

        public void CenterZoneOnIndex(int index, float duration = 0f)
        {
            if (zoneScrollView == null) return;
            zoneScrollView.SetSelectedIndex(index);
            zoneScrollView.CenterOnIndex(index, duration);
        }

        public Vector3 GetSliceIconWorldPosition(int sliceIndex) => cardWheelSpinner.GetSliceIconWorldPosition(sliceIndex);

        private void OnDestroy()
        {
            spinButton.onClick.RemoveListener(OnSpinButtonClicked);
            leaveButton.onClick.RemoveListener(OnLeaveButtonClicked);
        }
    }
}