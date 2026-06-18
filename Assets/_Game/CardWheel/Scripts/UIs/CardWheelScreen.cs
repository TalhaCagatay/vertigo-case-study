using System;
using System.Collections.Generic;
using _Game.CardWheel.Data;
using com.core.ui;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Game.CardWheel.UIs
{
    public class CardWheelScreen : BaseScreen
    {
        [SerializeField] private CardWheelSpinner   cardWheelSpinner;
        [SerializeField] private CardWheelIndicator cardWheelIndicator;

        [SerializeField] private Button      spinButton;
        [SerializeField] private Button      leaveButton;
        [SerializeField] private TMP_Text    zoneText;
        [SerializeField] private Transform   rewardPanelContainer;
        [SerializeField] private RewardEntry rewardEntryPrefab;

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
        }
#endif

        private void Awake()
        {
            spinButton.onClick.AddListener(OnSpinButtonClicked);
            leaveButton.onClick.AddListener(OnLeaveButtonClicked);
        }

        private void OnSpinButtonClicked()  => SpinClicked?.Invoke();
        private void OnLeaveButtonClicked() => LeaveClicked?.Invoke();

        public void SetupWheel(WheelTierConfig config, int currentZone)
        {
            cardWheelSpinner.Setup(config, currentZone);
            cardWheelIndicator.Setup(config);
        }

        public void SpinToIndex(int       sliceIndex, Action onComplete) => cardWheelSpinner.SpinToIndex(sliceIndex, spinDuration, onComplete);
        public void UpdateZoneDisplay(int zone) => zoneText.text = $"Zone {zone}";

        public void RebuildRewardPanel(IReadOnlyList<AccumulatedReward> rewards)
        {
            foreach (var entry in _rewardEntries) Destroy(entry.gameObject);
            _rewardEntries.Clear();

            foreach (var reward in rewards)
            {
                var entry = Instantiate(rewardEntryPrefab, rewardPanelContainer);
                entry.Setup(reward.Icon, reward.Amount, reward.Label);
                _rewardEntries.Add(entry);
            }
        }

        public void AddRewardEntry(Sprite icon, int amount, string label)
        {
            var entry = Instantiate(rewardEntryPrefab, rewardPanelContainer);
            entry.Setup(icon, amount, label);
            _rewardEntries.Add(entry);
        }

        public void PlayRewardAnimation(Vector3 sliceWorldPosition, Sprite sliceIcon, string rewardType, int addedAmount, Action onComplete)
        {
            var flyingIcon = new GameObject("FlyingRewardIcon", typeof(Image));
            flyingIcon.transform.SetParent(transform, false);

            var flyingImage = flyingIcon.GetComponent<Image>();
            flyingImage.sprite = sliceIcon;
            flyingImage.SetNativeSize();

            var    canvas       = GetComponentInParent<Canvas>();
            Camera canvasCamera = canvas?.worldCamera ?? null;

            var startScreenPos = RectTransformUtility.WorldToScreenPoint(canvasCamera, sliceWorldPosition);
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle
                    (
                     (RectTransform)transform,
                     startScreenPos,
                     canvasCamera,
                     out var localStart
                    ))
            {
                flyingIcon.transform.localPosition = localStart;
            }

            var           targetEntry = _rewardEntries.Find(e => e.Label.Contains(rewardType));
            RectTransform targetIconRect;
            if (targetEntry != null)
            {
                targetIconRect = targetEntry.IconRectTransform;
            }
            else if (_rewardEntries.Count > 0)
            {
                targetIconRect = _rewardEntries[^1].IconRectTransform;
            }
            else
            {
                targetIconRect = (RectTransform)rewardPanelContainer;
            }

            var iconCorners = new Vector3[4];
            targetIconRect.GetWorldCorners(iconCorners);
            var iconCenterWorld = (iconCorners[0] + iconCorners[2]) * 0.5f;

            var seq = DOTween.Sequence();
            seq.Append(flyingIcon.transform.DOMove(iconCenterWorld, flyDuration).SetEase(Ease.InBack));
            seq.Join(flyingIcon.transform.DOScale(Vector3.one * 0.5f, flyDuration).SetEase(Ease.InBack));
            seq.OnComplete
                (
                 () =>
                 {
                     Destroy(flyingIcon);
                     targetEntry.PlayAddAnimation(addedAmount, onComplete);
                 }
                );
        }

        public void SetSpinButtonInteractable(bool  interactable) => spinButton.interactable = interactable;
        public void SetLeaveButtonInteractable(bool interactable) => leaveButton.interactable = interactable;
        public void ResetWheel()                                  => cardWheelSpinner.ResetRotation();

        public Vector3 GetSliceIconWorldPosition(int sliceIndex) => cardWheelSpinner.GetSliceIconWorldPosition(sliceIndex);

        private void OnDestroy()
        {
            spinButton.onClick.RemoveListener(OnSpinButtonClicked);
            leaveButton.onClick.RemoveListener(OnLeaveButtonClicked);
        }
    }
}