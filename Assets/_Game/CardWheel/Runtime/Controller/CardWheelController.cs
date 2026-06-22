using System;
using System.Collections.Generic;
using Vertigo.CardWheel.Data;
using Vertigo.CardWheel.Data.Rewards;
using Vertigo.CardWheel.State;
using com.core;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Vertigo.Player;
using Random = UnityEngine.Random;

namespace Vertigo.CardWheel.Controller
{
    public class CardWheelController : AController
    {
        // todo: get these from a config asset
        public int ReviveCost => 50;
        public int SpinCost   => 10;

        public event Action<List<AccumulatedReward>> RewardsUpdated;
        public event Action<ARewardDefinition>       SpinCompleted;
        public event Action<ARewardDefinition>       RewardCollected;
        public event Action<WheelState>              StateChanged;
        public event Action<int>                     ZoneChanged;
        public event Action<int>                     SpinStarted;
        public event Action                          BombDetonated;
        public event Action                          RewardsReset;

        public WheelState      CurrentState          { get; private set; } = WheelState.Idle;
        public int             CurrentZone           { get; private set; } = 1;
        public WheelTierConfig CurrentTierConfig     { get; private set; }
        public int             PreSelectedSliceIndex { get; private set; } = -1;
        public bool            IsSpinning            => CurrentState == WheelState.Spinning;
        public bool            IsSuperZone           => CurrentZone > 0 && CurrentZone % 30 == 0;
        public bool            IsSafeZone            => !CurrentTierConfig.HasBomb;
        public bool            CanLeave              => !IsSpinning && (IsSafeZone || IsSuperZone);

        public int Order { get; }

        public override bool IsInitialized { get; protected set; }

        private readonly ZoneWheelMapping        _zoneMapping;
        private readonly List<AccumulatedReward> _accumulatedRewards = new();
        private readonly PlayerController        _playerController;

        public CardWheelController(ZoneWheelMapping zoneMapping, PlayerController playerController)
        {
            _zoneMapping      = zoneMapping;
            _playerController = playerController;
        }

        public override UniTask Initialize()
        {
            ResetState();
            IsInitialized = true;
            Debug.Log("[CardWheelController] Initialized");
            return UniTask.CompletedTask;
        }

        private void ResetState()
        {
            CurrentZone = 1;
            _accumulatedRewards.Clear();
            PreSelectedSliceIndex = -1;
            SetState(WheelState.Idle);
            ResolveTierConfig();
            ZoneChanged?.Invoke(CurrentZone);
            RewardsReset?.Invoke();
            RewardsUpdated?.Invoke(new List<AccumulatedReward>(_accumulatedRewards));
        }

        private void ResolveTierConfig() => CurrentTierConfig = _zoneMapping.GetConfigForZone(CurrentZone);

        public WheelTierConfig GetConfigForZone(int zone) => _zoneMapping.GetConfigForZone(zone);

        private void SetState(WheelState newState)
        {
            CurrentState = newState;
            StateChanged?.Invoke(newState);
        }

        public void PrepareSpin()
        {
            if (CurrentState != WheelState.Idle)
            {
                Debug.LogWarning("[CardWheelController] Cannot prepare spin: not in Idle state");
                return;
            }

            if (!_playerController.SpendCoins(SpinCost))
            {
                Debug.LogWarning($"[CardWheelController] Not enough coins to spin, requires:{SpinCost}, have:{_playerController.CoinBalance}");
                return;
            }

            ResolveTierConfig();
            var config     = CurrentTierConfig;
            var sliceCount = config.Slices.Length;

            PreSelectedSliceIndex = Random.Range(0, sliceCount);

            Debug.Log($"[CardWheelController] Pre-selected slice index: {PreSelectedSliceIndex} ({config.Slices[PreSelectedSliceIndex].Label})");
        }

        public void OnSpinStarted()
        {
            SetState(WheelState.Spinning);
            SpinStarted?.Invoke(PreSelectedSliceIndex);
        }

        public void CompleteSpin()
        {
            if (PreSelectedSliceIndex < 0)
            {
                Debug.LogError("[CardWheelController] No slice selected.");
                return;
            }

            var rewardDefinition = CurrentTierConfig.Slices[PreSelectedSliceIndex];

            if (rewardDefinition is BombReward) // it is fine to handle bomb like this since it is the only special case.
            {
                SetState(WheelState.GameOver);
                BombDetonated?.Invoke();
            }
            else
            {
                var multiplier   = CurrentTierConfig.GetRewardMultiplier(CurrentZone);
                var scaledAmount = Mathf.RoundToInt(rewardDefinition.Amount * multiplier);

                AddReward(rewardDefinition, scaledAmount);
                SetState(WheelState.Result);
            }
            SpinCompleted?.Invoke(rewardDefinition);
        }

        private void AddReward(ARewardDefinition rewardDefinition, int scaledAmount)
        {
            var existing = _accumulatedRewards.Find(r => r.Id == rewardDefinition.id);
            if (existing != null)
            {
                existing.Add(scaledAmount);
            }
            else
            {
                _accumulatedRewards.Add(new AccumulatedReward(rewardDefinition, scaledAmount));
            }

            RewardCollected?.Invoke(rewardDefinition);
            RewardsUpdated?.Invoke(new List<AccumulatedReward>(_accumulatedRewards));
        }

        public void AdvanceZone()
        {
            CurrentZone++;
            ResolveTierConfig();
            PreSelectedSliceIndex = -1;
            SetState(WheelState.Idle);
            ZoneChanged?.Invoke(CurrentZone);
        }

        public List<AccumulatedReward> CollectRewardsAndLeave()
        {
            if (!CanLeave)
            {
                Debug.LogWarning("[CardWheelController] Cannot leave right now");
                return new List<AccumulatedReward>(_accumulatedRewards);
            }

            foreach (var reward in _accumulatedRewards)
            {
                reward.Definition.Grant(_playerController, reward.Amount);
            }

            var rewards = new List<AccumulatedReward>(_accumulatedRewards);
            ResetState();
            return rewards;
        }

        public bool Revive()
        {
            if (CurrentState != WheelState.GameOver)
            {
                Debug.LogWarning("[CardWheelController] Revive called but not in GameOver state");
                return false;
            }

            if (!_playerController.SpendCoins(ReviveCost))
            {
                Debug.LogWarning($"[CardWheelController] Not enough coins to revive. Required: {ReviveCost}, Balance: {_playerController.CoinBalance}");
                return false;
            }

            PreSelectedSliceIndex = -1;
            SetState(WheelState.Idle);
            Debug.Log($"[CardWheelController] Player revived for {ReviveCost} coins, continuing from same zone");
            return true;
        }

        public void GiveUp()
        {
            if (CurrentState != WheelState.GameOver)
            {
                Debug.LogWarning("[CardWheelController] GiveUp called but not in GameOver state");
                return;
            }

            ResetState();
            Debug.Log("[CardWheelController] Player gave up, full reset to zone 1");
        }
    }
}