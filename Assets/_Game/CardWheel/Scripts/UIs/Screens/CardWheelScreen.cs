using System;
using System.Collections.Generic;
using Vertigo.CardWheel.Data;
using Vertigo.CardWheel.UIs.Rewards;
using Vertigo.CardWheel.UIs.ZoneScroll;
using com.core.ui;
using UnityEngine;
using UnityEngine.UI;

namespace Vertigo.CardWheel.UIs.Screens
{
    public class CardWheelScreen : BaseScreen
    {
        [SerializeField] private CardWheelSpinner   cardWheelSpinner;
        [SerializeField] private CardWheelIndicator cardWheelIndicator;
        [SerializeField] private Button             spinButton;
        [SerializeField] private Button             leaveButton;
        [SerializeField] private RewardScrollView   rewardScrollView;
        [SerializeField] private ZoneScrollView     zoneScrollView;

        public event Action SpinClicked;
        public event Action LeaveClicked;

#if UNITY_EDITOR
        private void OnValidate()
        {
            cardWheelSpinner   = GetComponentInChildren<CardWheelSpinner>();
            cardWheelIndicator = GetComponentInChildren<CardWheelIndicator>();
            rewardScrollView   = GetComponentInChildren<RewardScrollView>();
            zoneScrollView     = GetComponentInChildren<ZoneScrollView>();
        }
#endif

        private void Awake()
        {
            spinButton.onClick.AddListener(OnSpinButtonClicked);
            leaveButton.onClick.AddListener(OnLeaveButtonClicked);
        }

        private void OnDestroy()
        {
            spinButton.onClick.RemoveListener(OnSpinButtonClicked);
            leaveButton.onClick.RemoveListener(OnLeaveButtonClicked);
        }

        private void OnSpinButtonClicked()  => SpinClicked?.Invoke();
        private void OnLeaveButtonClicked() => LeaveClicked?.Invoke();

        public void SetupWheel(WheelTierConfig config, int currentZone)
        {
            cardWheelSpinner.Setup(config, currentZone);
            cardWheelIndicator.Setup(config);
        }

        public void SpinToIndex(int sliceIndex, float spinDuration, Action onComplete) => cardWheelSpinner.SpinToIndex(sliceIndex, spinDuration, onComplete);

        public void ClearRewardPanel() => rewardScrollView.ClearRewards();

        public void AddOrUpdateReward(Sprite icon, int amount, string id, string label) => rewardScrollView.AddOrUpdateReward(icon, amount, id, label);

        public void PlayRewardAnimation(Vector3 sliceWorldPosition, Sprite sliceIcon, string rewardId, int addedAmount, Action onComplete) => rewardScrollView.PlayRewardAnimation
            (sliceWorldPosition, sliceIcon, rewardId, addedAmount, onComplete);

        public void    SetSpinButtonInteractable(bool  interactable) => spinButton.interactable = interactable;
        public void    SetLeaveButtonInteractable(bool interactable) => leaveButton.interactable = interactable;
        public void    ResetWheel()                                  => cardWheelSpinner.ResetRotation();
        public void    SetupZoneBar(IList<ZoneItemData> items)       => zoneScrollView.UpdateData(items);
        public Vector3 GetSliceIconWorldPosition(int    sliceIndex)  => cardWheelSpinner.GetSliceIconWorldPosition(sliceIndex);

        public void CenterZoneOnIndex(int index, float duration = 0f)
        {
            zoneScrollView.SetSelectedIndex(index);
            zoneScrollView.CenterOnIndex(index, duration);
        }
    }
}