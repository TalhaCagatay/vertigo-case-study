using System.Collections.Generic;
using Vertigo.CardWheel.Controller;
using Vertigo.CardWheel.Data;
using Vertigo.CardWheel.State;
using Vertigo.CardWheel.UIs.Popups;
using Vertigo.CardWheel.UIs.ZoneScroll;
using Vertigo.CardWheel.UIs.Wheel;
using com.core;
using com.core.ui;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Vertigo.CardWheel.UIs
{
    public class CardWheelUIController : IController
    {
        private readonly UIController        _uiController;
        private readonly CardWheelController _cardWheelController;

        private CardWheelScreen    _screen;
        private List<ZoneItemData> _zoneItems = new();
        private int                _zoneBarMaxZone;

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
            ClearZoneBar();
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

            _screen.SpinToIndex(preSelectedIndex, _cardWheelController.CurrentTierConfig.SpinDuration, OnSpinCompleted);
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
            if (_zoneItems.Count == 0)
            {
                BuildInitialZoneRange(currentZone);
            }

            while (currentZone > _zoneBarMaxZone)
            {
                int zn  = _zoneBarMaxZone + 1;
                var cfg = _cardWheelController.GetConfigForZone(zn);
                _zoneItems.Add(BuildZoneItemData(zn, currentZone, cfg));
                _zoneBarMaxZone = zn;
            }

            for (int i = 0; i < _zoneItems.Count; i++)
            {
                var  item   = _zoneItems[i];
                bool isPast = item.ZoneNumber < currentZone;
                if (item.IsPastZone != isPast)
                {
                    var cfg = _cardWheelController.GetConfigForZone(item.ZoneNumber);
                    _zoneItems[i] = BuildZoneItemData(item.ZoneNumber, currentZone, cfg);
                }
            }

            _screen.SetupZoneBar(_zoneItems);
            _screen.CenterZoneOnIndex(currentZone - 1, 0.35f);
        }

        private void BuildInitialZoneRange(int currentZone)
        {
            _zoneItems.Clear();
            _zoneBarMaxZone = 0;

            int start = 1;
            int end   = Mathf.Max(currentZone + 20, 50);

            for (int zn = start; zn <= end; zn++)
            {
                var cfg = _cardWheelController.GetConfigForZone(zn);
                _zoneItems.Add(BuildZoneItemData(zn, currentZone, cfg));
            }
            _zoneBarMaxZone = end;
        }

        private ZoneItemData BuildZoneItemData(int zn, int currentZone, WheelTierConfig cfg)
        {
            var isPast = zn < currentZone;
            return new ZoneItemData
                (
                 zn,
                 $"Zone {zn}",
                 _cardWheelController.IsSuperZone,
                 _cardWheelController.IsSafeZone,
                 isPast,
                 cfg.ZoneNumberSelectedColor,
                 cfg.ZoneNumberPastColor,
                 cfg.ZoneNumberFutureColor
                );
        }

        private void ClearZoneBar()
        {
            _zoneItems.Clear();
            _zoneBarMaxZone = 0;
        }
    }
}