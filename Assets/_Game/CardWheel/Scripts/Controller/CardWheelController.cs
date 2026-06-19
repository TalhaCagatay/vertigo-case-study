using System;
using System.Collections.Generic;
using _Game.CardWheel.Data;
using _Game.CardWheel.State;
using com.core;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Game.CardWheel.Controller
{
    public class CardWheelController : IController
    {
        public event Action<int>                     ZoneChanged;
        public event Action<int>                     SpinStarted;
        public event Action<WheelSliceData>          SpinCompleted;
        public event Action                          BombDetonated;
        public event Action<WheelSliceData>          RewardCollected;
        public event Action<WheelState>              StateChanged;
        public event Action                          RewardsReset;
        public event Action<List<AccumulatedReward>> RewardsUpdated;

        public WheelState      CurrentState          { get; private set; } = WheelState.Idle;
        public int             CurrentZone           { get; private set; } = 1;
        public WheelTierConfig CurrentTierConfig     { get; private set; }
        public int             PreSelectedSliceIndex { get; private set; } = -1;
        public bool            CanLeave              => !IsSpinning && CurrentTierConfig != null && !CurrentTierConfig.HasBomb;
        public bool            IsSpinning            => CurrentState == WheelState.Spinning;

        public bool IsInitialized { get; private set; }

        private readonly ZoneWheelMapping        _zoneMapping;
        private readonly List<AccumulatedReward> _accumulatedRewards = new();

        public CardWheelController(ZoneWheelMapping zoneMapping)
        {
            _zoneMapping = zoneMapping;
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
            RewardsReset?.Invoke();
            RewardsUpdated?.Invoke(new List<AccumulatedReward>(_accumulatedRewards));
        }

        private void ResolveTierConfig() => CurrentTierConfig = _zoneMapping.GetConfigForZone(CurrentZone);

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
            if (CurrentState != WheelState.Spinning)
            {
                Debug.LogWarning("[CardWheelController] CompleteSpin called but not in Spinning state");
                return;
            }

            var landedSlice = CurrentTierConfig.Slices[PreSelectedSliceIndex];

            if (landedSlice.IsBomb)
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

        private void AddReward(WheelSliceData slice, int scaledAmount)
        {
            var existing = _accumulatedRewards.Find(r => r.RewardType == slice.RewardType);
            if (existing != null)
            {
                existing.Add(scaledAmount);
            }
            else
            {
                _accumulatedRewards.Add(new AccumulatedReward(slice.RewardType, slice.Icon, slice.Label, scaledAmount));
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

            var rewards = new List<AccumulatedReward>(_accumulatedRewards);
            ResetState();
            return rewards;
        }

        public void Revive()
        {
            if (CurrentState != WheelState.GameOver)
            {
                Debug.LogWarning("[CardWheelController] Revive called but not in GameOver state");
                return;
            }

            // _accumulatedRewards.Clear();
            PreSelectedSliceIndex = -1;
            SetState(WheelState.Idle);
            // RewardsReset?.Invoke();
            // RewardsUpdated?.Invoke(new List<AccumulatedReward>(_accumulatedRewards));
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