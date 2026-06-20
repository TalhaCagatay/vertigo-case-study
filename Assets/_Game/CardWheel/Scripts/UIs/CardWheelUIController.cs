using System.Collections.Generic;
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

        private const int ZONE_BAR_ITEM_COUNT = 60;

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
            UpdateButtonStates();

            IsInitialized = true;
            Debug.Log("[CardWheelUIController] Initialized");
        }

        private void StateChanged(WheelState state)
        {
            if (state == WheelState.Result) return;
            UpdateButtonStates();
        }

        private void ZoneChanged(int zone)
        {
            var config = _cardWheelController.CurrentTierConfig;
            _screen.SetupWheel(config, zone);

            PopulateZoneBar(zone);
            UpdateButtonStates();
        }

        private void RewardsUpdated(List<AccumulatedReward> rewards)
        {
            if (rewards.Count == 0) _screen.ClearRewardPanel();
        }

        private async void BombDetonated()
        {
            var bombPopup = await _uiController.PushPopupAsync<BombPopup>();
            bombPopup.Setup(OnGiveUpClicked, OnReviveClicked);
        }

        private void OnGiveUpClicked()
        {
            _uiController.PopPopup();
            _cardWheelController.GiveUp();
            RefreshAfterReset();
        }

        private void OnReviveClicked()
        {
            _uiController.PopPopup();
            _cardWheelController.Revive();
        }

        private void RefreshAfterReset()
        {
            var config = _cardWheelController.CurrentTierConfig;
            var zone   = _cardWheelController.CurrentZone;
            _screen.SetupWheel(config, zone);

            _screen.ResetWheel();
            PopulateZoneBar(zone);
            _screen.ClearRewardPanel();
            UpdateButtonStates();
        }

        private void SpinRequested()
        {
            _cardWheelController.PrepareSpin();

            _screen.SetSpinButtonInteractable(false);

            _cardWheelController.OnSpinStarted();

            var preSelectedIndex = _cardWheelController.PreSelectedSliceIndex;

            _screen.SpinToIndex(preSelectedIndex, OnSpinCompleted);
        }

        private void OnSpinCompleted()
        {
            var preSelectedIndex = _cardWheelController.PreSelectedSliceIndex;
            var config           = _cardWheelController.CurrentTierConfig;
            var landedSlice      = config.Slices[preSelectedIndex];
            _cardWheelController.CompleteSpin();

            if (_cardWheelController.CurrentState == WheelState.GameOver) return;

            var multiplier   = config.GetRewardMultiplier(_cardWheelController.CurrentZone);
            var scaledAmount = Mathf.RoundToInt(landedSlice.Amount * multiplier);

            var sliceWorldPos = _screen.GetSliceIconWorldPosition(preSelectedIndex);

            _screen.AddOrUpdateReward(landedSlice.Icon, 0, landedSlice.id, landedSlice.Label);

            _screen.PlayRewardAnimation
                (
                 sliceWorldPos,
                 landedSlice.Icon,
                 landedSlice.id,
                 scaledAmount,
                 () =>
                 {
                     _cardWheelController.AdvanceZone();
                     _screen.SetSpinButtonInteractable(true);
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
            var start         = Mathf.Max(1, currentZone - centerDesired);
            var centerIndex   = currentZone - start;

            for (int i = 0; i < ZONE_BAR_ITEM_COUNT; i++)
            {
                var zn                     = start + i;
                var cfg                    = _cardWheelController.GetConfigForZone(zn);
                var isSuper                = zn > 0 && zn % 30 == 0;
                var isSafe                 = !cfg.HasBomb;
                var isPast                 = zn < currentZone;
                var zoneNumberCurrentColor = cfg.ZoneNumberSelectedColor;
                var zoneNumberPastColor    = cfg.ZoneNumberPastColor;
                var zoneNumberFutureColor  = cfg.ZoneNumberFutureColor;
                items.Add(new ZoneItemData(zn, $"Zone {zn}", isSuper, isSafe, isPast, zoneNumberCurrentColor, zoneNumberPastColor, zoneNumberFutureColor));
            }

            _screen.SetupZoneBar(items);
            _screen.CenterZoneOnIndex(centerIndex, 0.35f);
        }
    }
}