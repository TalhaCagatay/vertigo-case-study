using System.Collections.Generic;
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

        public int Order { get; }

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

        private void RewardsUpdated(List<AccumulatedReward> rewards)
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

            var scaledAmount = config.GetScaledRewardAmount(_cardWheelController.CurrentZone, landedSlice.Amount);

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
                    _zoneItems[i] =
                        BuildZoneItemData(item.ZoneNumber, currentZone, cfg);
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

        private void ClearZoneBar() => _zoneItems.Clear();

        private async void RewardsRequested()
        {
            _rewardScreen             =  await _uiController.ShowScreenAsync<RewardScreen>();
            _rewardScreen.BackClicked += OnRewardBackClicked;

            var rewardItems = BuildRewardItemsFromPlayerData(_playerController);
            _rewardScreen.DisplayRewards(rewardItems);
        }

        private void OnRewardBackClicked()
        {
            if (_rewardScreen != null)
                _rewardScreen.BackClicked -= OnRewardBackClicked;

            _uiController.ShowScreenAsync<CardWheelScreen>().Forget();
        }

        private List<RewardItemData> BuildRewardItemsFromPlayerData(PlayerController playerController)
        {
            var items     = new List<RewardItemData>();
            var allSlices = GetAllSlices();

            foreach (var kvp in playerController.Rewards)
            {
                var slice = FindSliceById(allSlices, kvp.Key);
                if (slice != null)
                {
                    items.Add(new RewardItemData(slice.id, slice.Icon, slice.Label, kvp.Value));
                }
                else
                {
                    items.Add(new RewardItemData(kvp.Key, null, kvp.Key, kvp.Value));
                }
            }

            if (playerController.CoinBalance > 0)
            {
                var coinSlice = FindCoinSlice(allSlices);
                if (coinSlice != null)
                {
                    items.Add(new RewardItemData(coinSlice.id, coinSlice.Icon, coinSlice.Label, playerController.CoinBalance));
                }
            }

            return items;
        }

        private static ARewardDefinition FindCoinSlice(List<ARewardDefinition> slices)
        {
            foreach (var slice in slices)
            {
                if (slice is CoinReward)
                    return slice;
            }
            return null;
        }

        private List<ARewardDefinition> GetAllSlices()
        {
            var slices = new List<ARewardDefinition>();
            if (_zoneMapping.BronzeConfig != null)
                slices.AddRange(_zoneMapping.BronzeConfig.Slices);
            if (_zoneMapping.SilverConfig != null)
                slices.AddRange(_zoneMapping.SilverConfig.Slices);
            if (_zoneMapping.GoldConfig != null)
                slices.AddRange(_zoneMapping.GoldConfig.Slices);
            return slices;
        }

        private static ARewardDefinition FindSliceById(List<ARewardDefinition> slices, string id)
        {
            foreach (var slice in slices)
            {
                if (slice.id == id)
                    return slice;
            }
            return null;
        }
    }
}