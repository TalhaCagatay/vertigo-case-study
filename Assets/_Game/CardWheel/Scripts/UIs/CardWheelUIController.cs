using System.Collections.Generic;
using System.Linq;
using _Game.CardWheel.Controller;
using _Game.CardWheel.Data;
using _Game.CardWheel.State;
using _Game.CardWheel.UIs.Popups;
using _Game.CardWheel.UIs.TopZoneScroll;
using com.core;
using com.core.ui;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Game.CardWheel.UIs
{
    public class CardWheelUIController : IController
    {
        private readonly UIController        _uiController;
        private readonly CardWheelController _cardWheelController;

        private CardWheelScreen _screen;

        private const int   ZONE_BAR_ITEM_COUNT    = 60;
        private const float ZONE_AUTO_SCROLL_SPEED = 0.6f;

        public bool IsInitialized { get; private set; }

        public CardWheelUIController(UIController uiController, CardWheelController cardWheelController)
        {
            _uiController        = uiController;
            _cardWheelController = cardWheelController;
            Initialize().Forget();
        }

        public async UniTask Initialize()
        {
            _screen = await _uiController.ShowScreenAsync<CardWheelScreen>();

            _cardWheelController.StateChanged   += StateChanged;
            _cardWheelController.ZoneChanged    += ZoneChanged;
            _cardWheelController.RewardsUpdated += RewardsUpdated;
            _cardWheelController.BombDetonated  += BombDetonated;

            _screen.SpinClicked  += SpinRequested;
            _screen.LeaveClicked += LeaveRequested;

            var config = _cardWheelController.CurrentTierConfig;
            var zone   = _cardWheelController.CurrentZone;
            _screen.SetupWheel(config, zone);

            PopulateZoneBar(zone);

            _screen.UpdateZoneDisplay(_cardWheelController.CurrentZone);
            _screen.RebuildRewardPanel(_cardWheelController.GetAccumulatedRewards());
            PopulateZoneBar(zone);
            UpdateButtonStates();

            IsInitialized = true;
            Debug.Log("[CardWheelUIController] Initialized");
        }

        private void StateChanged(WheelState state) => UpdateButtonStates();

        private void ZoneChanged(int zone)
        {
            _screen.UpdateZoneDisplay(zone);

            var config = _cardWheelController.CurrentTierConfig;
            _screen.SetupWheel(config, zone);

            PopulateZoneBar(zone);

            UpdateButtonStates();
        }

        private void RewardsUpdated(List<AccumulatedReward> rewards)
        {
            if (_cardWheelController.CurrentState == WheelState.Idle ||
                _cardWheelController.CurrentState == WheelState.GameOver)
            {
                _screen.RebuildRewardPanel(rewards);
            }
        }

        private async void BombDetonated()
        {
            var bombPopup = await _uiController.PushPopupAsync<BombPopup>();
            bombPopup.Setup(OnGiveUpClicked, OnReviveClicked);
        }

        private async void OnGiveUpClicked()
        {
            await _uiController.PopPopupAsync();
            _cardWheelController.GiveUp();
            RefreshAfterReset();
        }

        private void OnReviveClicked()
        {
            _uiController.PopPopup();
            _cardWheelController.Revive();
            RefreshAfterReset();
        }

        private void RefreshAfterReset()
        {
            var config = _cardWheelController.CurrentTierConfig;
            var zone   = _cardWheelController.CurrentZone;
            _screen.SetupWheel(config, zone);

            _screen.ResetWheel();
            _screen.UpdateZoneDisplay(_cardWheelController.CurrentZone);
            _screen.RebuildRewardPanel(_cardWheelController.GetAccumulatedRewards());
            UpdateButtonStates();
        }

        private void SpinRequested()
        {
            if (_cardWheelController.CurrentState != WheelState.Idle)
                return;

            _cardWheelController.PrepareSpin();

            _screen.SetSpinButtonInteractable(false);

            _cardWheelController.OnSpinStarted();

            var preSelectedIndex = _cardWheelController.PreSelectedSliceIndex;
            var config           = _cardWheelController.CurrentTierConfig;
            var landedSlice      = config.Slices[preSelectedIndex];
            var existingRewards  = _cardWheelController.GetAccumulatedRewards();

            _screen.SpinToIndex
                (
                 preSelectedIndex,
                 () =>
                 {
                     var alreadyExists = existingRewards.Any(r => r.RewardType == landedSlice.RewardType);
                     _cardWheelController.CompleteSpin();

                     if (_cardWheelController.CurrentState == WheelState.GameOver) return;

                     var multiplier   = config.GetRewardMultiplier(_cardWheelController.CurrentZone);
                     var scaledAmount = Mathf.RoundToInt(landedSlice.Amount * multiplier);

                     var sliceWorldPos = _screen.GetSliceIconWorldPosition(preSelectedIndex);

                     if (!alreadyExists) _screen.AddRewardEntry(landedSlice.Icon, 0, landedSlice.id, landedSlice.Label);

                     _screen.PlayRewardAnimation
                         (
                          sliceWorldPos,
                          landedSlice.Icon,
                          landedSlice.RewardType,
                          scaledAmount,
                          () =>
                          {
                              _cardWheelController.AdvanceZone();
                              _screen.SetSpinButtonInteractable(true);
                          }
                         );
                 }
                );
        }

        private void LeaveRequested()
        {
            if (!_cardWheelController.CanLeave)
            {
                Debug.LogWarning($"[CardWheelUIController] Can not leave in this state.");
                return;
            }

            var rewards = _cardWheelController.CollectRewardsAndLeave();

            ShowLeaveSummaryPopup(rewards);
        }

        private void ShowLeaveSummaryPopup(List<AccumulatedReward> rewards)
        {
            Debug.Log("[CardWheelUIController] Player left with rewards:");
            foreach (var reward in rewards)
            {
                Debug.Log($"  - {reward.Label} x{reward.Amount}");
            }

            RefreshAfterReset();
        }

        private void UpdateButtonStates()
        {
            var state = _cardWheelController.CurrentState;
            _screen.SetSpinButtonInteractable(state == WheelState.Idle);
            _screen.SetLeaveButtonInteractable(_cardWheelController.CanLeave);
        }

        private void PopulateZoneBar(int currentZone)
        {
            var items = new List<ZoneItemData>(ZONE_BAR_ITEM_COUNT);

            var centerDesired = ZONE_BAR_ITEM_COUNT / 2;
            var start = Mathf.Max(1, currentZone - centerDesired);
            var centerIndex = currentZone - start;

            for (int i = 0; i < ZONE_BAR_ITEM_COUNT; i++)
            {
                var zn = start + i;
                var cfg = _cardWheelController.GetConfigForZone(zn);
                var isSuper = zn > 0 && zn % 30 == 0;
                var isSafe = cfg != null && !cfg.HasBomb;
                var isPast = zn < currentZone;
                items.Add(new ZoneItemData(zn, $"Zone {zn}", isSuper, isSafe, isPast));
            }

            _screen.SetupZoneBar(items);
            _screen.CenterZoneOnIndex(centerIndex, 0.35f);
        }
    }
}