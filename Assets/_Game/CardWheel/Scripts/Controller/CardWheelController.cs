using System;
using System.Collections.Generic;
using Vertigo.CardWheel.Data;
using Vertigo.CardWheel.Data.Rewards;
using Vertigo.CardWheel.State;
using com.core;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Vertigo.CardWheel.Controller
{
    public class CardWheelController : IController
    {
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

        public bool IsInitialized { get; private set; }

        private readonly ZoneWheelMapping        _zoneMapping;
        private readonly List<AccumulatedReward> _accumulatedRewards = new();
        private readonly PlayerData              _playerData;

        public CardWheelController(ZoneWheelMapping zoneMapping, PlayerData playerData)
        {
            _zoneMapping = zoneMapping;
            _playerData  = playerData;
            Initialize();
        }

        public UniTask Initialize()
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

            ResolveTierConfig();
            var config     = CurrentTierConfig;
            var sliceCount = config.Slices.Length;

            PreSelectedSliceIndex = Random.Range(0, sliceCount);

            Debug.Log($"[CardWheelController] Pre-selected slice index: {PreSelectedSliceIndex} ({(config.Slices[PreSelectedSliceIndex]?.Label ?? "null")})");
        }

        public void OnSpinStarted()
        {
            SetState(WheelState.Spinning);
            SpinStarted?.Invoke(PreSelectedSliceIndex);
        }

        public void CompleteSpin()
        {
            var landedSlice = CurrentTierConfig.Slices[PreSelectedSliceIndex];

            if (landedSlice is BombReward)
            {
                SetState(WheelState.GameOver);
                BombDetonated?.Invoke();
            }
            else
            {
                var multiplier   = CurrentTierConfig.GetRewardMultiplier(CurrentZone);
                var scaledAmount = Mathf.RoundToInt(landedSlice.Amount * multiplier);

                AddReward(landedSlice, scaledAmount);
                SetState(WheelState.Result);
            }
            SpinCompleted?.Invoke(landedSlice);
        }

        private void AddReward(ARewardDefinition slice, int scaledAmount)
        {
            var existing = _accumulatedRewards.Find(r => r.RewardType == slice.RewardType);
            if (existing != null)
            {
                existing.Add(scaledAmount);
            }
            else
            {
                _accumulatedRewards.Add(new AccumulatedReward(slice.RewardType, slice.Icon, slice.id, slice.Label, scaledAmount));
            }

            RewardCollected?.Invoke(slice);
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
                var slice = FindSliceByRewardType(reward.RewardType);
                if (slice != null)
                {
                    slice.Grant(_playerData, reward.Amount);
                }
            }

            var rewards = new List<AccumulatedReward>(_accumulatedRewards);
            ResetState();
            return rewards;
        }

        private ARewardDefinition FindSliceByRewardType(string rewardType)
        {
            foreach (var slice in CurrentTierConfig.Slices)
            {
                if (slice.RewardType == rewardType)
                    return slice;
            }
            return null;
        }

        public void Revive()
        {
            if (CurrentState != WheelState.GameOver)
            {
                Debug.LogWarning("[CardWheelController] Revive called but not in GameOver state");
                return;
            }

            PreSelectedSliceIndex = -1;
            SetState(WheelState.Idle);
            Debug.Log("[CardWheelController] Player revived, continuing from same zone");
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

        public IReadOnlyList<AccumulatedReward> GetAccumulatedRewards() => _accumulatedRewards.AsReadOnly();
    }
}