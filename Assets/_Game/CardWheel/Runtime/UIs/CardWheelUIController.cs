using System;
using System.Collections.Generic;
using System.Text;
using Vertigo.CardWheel.Controller;
using Vertigo.CardWheel.Data;
using Vertigo.CardWheel.Data.Rewards;
using Vertigo.CardWheel.State;
using Vertigo.CardWheel.UIs.Popups;
using Vertigo.CardWheel.UIs.Rewards;
using Vertigo.CardWheel.UIs.ZoneScroll;
using Vertigo.CardWheel.UIs.Screens;
using com.core;
using com.core.ui;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Vertigo.Player;

namespace Vertigo.CardWheel.UIs
{
    public class CardWheelUIController : AController
    {
        private readonly UIController        _uiController;
        private readonly CardWheelController _cardWheelController;
        private readonly PlayerController    _playerController;
        private readonly ZoneWheelMapping    _zoneMapping;
        private readonly List<ZoneItemData>  _zoneItems = new();

        private const int VisibleAheadCount = 10;

        private CardWheelScreen _screen;
        private RewardScreen    _rewardScreen;

        public override bool IsInitialized { get; protected set; }

        public CardWheelUIController(UIController uiController, CardWheelController cardWheelController, PlayerController playerController, ZoneWheelMapping zoneMapping)
        {
            _uiController        = uiController;
            _cardWheelController = cardWheelController;
            _playerController    = playerController;
            _zoneMapping         = zoneMapping;
        }

        public override async UniTask Initialize()
        {
            _screen = await _uiController.ShowScreenAsync<CardWheelScreen>();

            _cardWheelController.StateChanged   += StateChanged;
            _cardWheelController.ZoneChanged    += ZoneChanged;
            _cardWheelController.RewardsUpdated += RewardsUpdated;
            _cardWheelController.BombDetonated  += BombDetonated;

            _screen.SpinClicked    += SpinRequested;
            _screen.LeaveClicked   += LeaveRequested;
            _screen.RewardsClicked += RewardsRequested;

            var config = _cardWheelController.CurrentTierConfig;
            var zone   = _cardWheelController.CurrentZone;
            _screen.SetupWheel(config, zone);

            PopulateZoneBar(zone);
            UpdateButtonStates();

            _screen.SetSpinCost(_cardWheelController.SpinCost);

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

        private void RewardsUpdated(List<RewardModel> rewards)
        {
            if (rewards.Count == 0) _screen.ClearRewardPanel();
        }

        private async void BombDetonated()
        {
            var bombPopup   = await _uiController.PushPopupAsync<BombPopup>();
            var coinBalance = _playerController.CoinBalance;
            bombPopup.Setup(OnGiveUpClicked, OnReviveClicked, _cardWheelController.ReviveCost, coinBalance);
        }

        private void OnGiveUpClicked()
        {
            _uiController.PopPopup();
            _cardWheelController.GiveUp();
            RefreshAfterReset();
        }

        private void OnReviveClicked()
        {
            if (_cardWheelController.Revive()) _uiController.PopPopup();
        }

        private void RefreshAfterReset()
        {
            var config = _cardWheelController.CurrentTierConfig;
            var zone   = _cardWheelController.CurrentZone; // should be 1

            _screen.SetupWheel(config, zone);
            _screen.ResetWheel();

            ResetZoneBar(zone);

            _screen.ClearRewardPanel();
            UpdateButtonStates();
        }

        private void ResetZoneBar(int currentZone)
        {
            _zoneItems.Clear();
            BuildInitialZoneRange(currentZone);

            _screen.SetupZoneBar(_zoneItems);
            _screen.CenterZoneOnIndex(0, 0f);
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

            var rewardDefinition = landedSlice as ARewardDefinition;
            var scaledAmount     = config.GetScaledRewardAmount(_cardWheelController.CurrentZone, rewardDefinition.Amount);

            var sliceWorldPos = _screen.GetSliceIconWorldPosition(preSelectedIndex);

            _screen.AddOrUpdateReward(rewardDefinition, 0);

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

        private void ShowLeaveSummaryPopup(List<RewardModel> rewards)
        {
            var sb = new StringBuilder();
            foreach (var reward in rewards)
            {
                sb.Append($"{reward.Label} x{reward.Amount}{Environment.NewLine}");
            }

            Debug.Log($"[CardWheelUIController] Player left with rewards:{Environment.NewLine}{sb}");

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

            int desiredCount = currentZone + VisibleAheadCount;

            while (_zoneItems.Count < desiredCount)
            {
                int zn = _zoneItems.Count + 1;
                AddZoneItem(zn, currentZone);
            }

            UpdatePastStates(currentZone);

            _screen.SetupZoneBar(_zoneItems);
            _screen.CenterZoneOnIndex(currentZone - 1, 0.35f);
        }

        private void UpdatePastStates(int currentZone)
        {
            for (int i = 0; i < _zoneItems.Count; i++)
            {
                var  item         = _zoneItems[i];
                bool shouldBePast = item.ZoneNumber < currentZone;

                if (item.IsPastZone != shouldBePast)
                {
                    var cfg = _cardWheelController.GetConfigForZone(item.ZoneNumber);
                    _zoneItems[i] = BuildZoneItemData(item.ZoneNumber, currentZone, cfg);
                }
            }
        }

        private void BuildInitialZoneRange(int currentZone)
        {
            _zoneItems.Clear();

            for (int zn = 1; zn <= currentZone + VisibleAheadCount; zn++)
            {
                AddZoneItem(zn, currentZone);
            }
        }

        private void AddZoneItem(int zone, int currentZone)
        {
            var cfg = _cardWheelController.GetConfigForZone(zone);
            _zoneItems.Add(BuildZoneItemData(zone, currentZone, cfg));
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

        private async void RewardsRequested()
        {
            _rewardScreen             =  await _uiController.ShowScreenAsync<RewardScreen>();
            _rewardScreen.BackClicked += OnRewardBackClicked;

            var rewardItems = BuildRewardItemsFromPlayerData(_playerController);
            _rewardScreen.DisplayRewards(rewardItems);
        }

        private void OnRewardBackClicked()
        {
            _rewardScreen.BackClicked -= OnRewardBackClicked;
            _uiController.ShowScreen<CardWheelScreen>();
        }

        private List<RewardModel> BuildRewardItemsFromPlayerData(PlayerController playerController)
        {
            var items     = new List<RewardModel>();
            var allSlices = GetAllSlices();

            if (playerController.CoinBalance > 0)
            {
                var coinSlice = FindCoinSlice(allSlices);
                items.Add(new RewardModel(coinSlice as ARewardDefinition, playerController.CoinBalance));
            }

            foreach (var kvp in playerController.Rewards)
            {
                var slice = FindSliceById(allSlices, kvp.Key);
                items.Add(new RewardModel(slice as ARewardDefinition, kvp.Value));
            }

            return items;
        }

        private static AWheelSliceDefinition FindCoinSlice(List<AWheelSliceDefinition> slices)
        {
            foreach (var slice in slices)
            {
                if (slice is CoinReward)
                    return slice;
            }
            return null;
        }

        private List<AWheelSliceDefinition> GetAllSlices()
        {
            var slices = new List<AWheelSliceDefinition>();
            slices.AddRange(_zoneMapping.BronzeConfig.Slices);
            slices.AddRange(_zoneMapping.SilverConfig.Slices);
            slices.AddRange(_zoneMapping.GoldConfig.Slices);
            return slices;
        }

        private static AWheelSliceDefinition FindSliceById(List<AWheelSliceDefinition> slices, string id) => slices.Find(slice => slice.id == id);
    }
}