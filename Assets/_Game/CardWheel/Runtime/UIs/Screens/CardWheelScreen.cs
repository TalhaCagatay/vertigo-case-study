using System;
using System.Collections.Generic;
using Vertigo.CardWheel.Data;
using Vertigo.CardWheel.UIs.Rewards;
using Vertigo.CardWheel.UIs.ZoneScroll;
using com.core.ui;
using UnityEngine;
using Vertigo.CardWheel.Component;
using Vertigo.CardWheel.Data.Rewards;

namespace Vertigo.CardWheel.UIs.Screens
{
    public class CardWheelScreen : BaseScreen
    {
        [SerializeField] private CardWheelSpinner   cardWheelSpinner;
        [SerializeField] private CardWheelIndicator cardWheelIndicator;
        [SerializeField] private SpinButton         spinButton;
        [SerializeField] private LeaveButton        leaveButton;
        [SerializeField] private RewardsButton      rewardsButton;
        [SerializeField] private RewardScrollView   rewardScrollView;
        [SerializeField] private ZoneScrollView     zoneScrollView;
        [SerializeField] private SpinCostDisplayer  spinCostDisplayer;

        public event Action SpinClicked;
        public event Action LeaveClicked;
        public event Action RewardsClicked;

#if UNITY_EDITOR
        private void OnValidate()
        {
            cardWheelSpinner   = GetComponentInChildren<CardWheelSpinner>();
            cardWheelIndicator = GetComponentInChildren<CardWheelIndicator>();
            rewardScrollView   = GetComponentInChildren<RewardScrollView>();
            zoneScrollView     = GetComponentInChildren<ZoneScrollView>();
            leaveButton        = GetComponentInChildren<LeaveButton>();
            rewardsButton      = GetComponentInChildren<RewardsButton>();
            spinButton         = GetComponentInChildren<SpinButton>();
            spinCostDisplayer  = GetComponentInChildren<SpinCostDisplayer>();
        }
#endif

        private void Awake()
        {
            spinButton.Clicked    += OnSpinButtonClicked;
            leaveButton.Clicked   += OnLeaveButtonClicked;
            rewardsButton.Clicked += OnRewardsButtonClicked;
        }

        private void OnDestroy()
        {
            spinButton.Clicked    -= OnSpinButtonClicked;
            leaveButton.Clicked   -= OnLeaveButtonClicked;
            rewardsButton.Clicked -= OnRewardsButtonClicked;
        }

        private void OnSpinButtonClicked()    => SpinClicked?.Invoke();
        private void OnLeaveButtonClicked()   => LeaveClicked?.Invoke();
        private void OnRewardsButtonClicked() => RewardsClicked?.Invoke();

        public void SetupWheel(WheelTierConfig config, int currentZone)
        {
            cardWheelSpinner.Setup(config, currentZone);
            cardWheelIndicator.Setup(config);
        }

        public void SetSpinCost(int cost) => spinCostDisplayer.SetSpinCost(cost);

        public void SpinToIndex(int sliceIndex, float spinDuration, Action onComplete) => cardWheelSpinner.SpinToIndex(sliceIndex, spinDuration, onComplete);

        public void ClearRewardPanel() => rewardScrollView.ClearRewards();

        public void AddOrUpdateReward(ARewardDefinition rewardDefinition, int amount) => rewardScrollView.AddOrUpdateReward(rewardDefinition, amount);

        public void PlayRewardAnimation(Vector3 sliceWorldPosition, Sprite sliceIcon, string rewardId, int addedAmount, Action onComplete) => rewardScrollView.PlayRewardAnimation
            (sliceWorldPosition, sliceIcon, rewardId, addedAmount, onComplete);

        public void    SetSpinButtonInteractable(bool  interactable) => spinButton.SetInteractable(interactable);
        public void    SetLeaveButtonInteractable(bool interactable) => leaveButton.SetInteractable(interactable);
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