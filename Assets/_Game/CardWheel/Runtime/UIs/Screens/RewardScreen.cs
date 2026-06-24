using System;
using System.Collections.Generic;
using Vertigo.CardWheel.UIs.Rewards;
using com.core.ui;
using UnityEngine;
using Vertigo.CardWheel.Component;
using Vertigo.CardWheel.Data;

namespace Vertigo.CardWheel.UIs.Screens
{
    public class RewardScreen : BaseScreen
    {
        [SerializeField] private RewardScrollView rewardScrollView;
        [SerializeField] private BackButton       backButton;

        public event Action BackClicked;

#if UNITY_EDITOR
        private void OnValidate()
        {
            rewardScrollView = GetComponentInChildren<RewardScrollView>();
            backButton       = GetComponentInChildren<BackButton>();
        }
#endif

        private void Awake()               => backButton.Clicked += OnBackButtonClicked;
        private void OnDestroy()           => backButton.Clicked -= OnBackButtonClicked;
        private void OnBackButtonClicked() => BackClicked?.Invoke();

        public void DisplayRewards(List<RewardModel> rewards)
        {
            rewardScrollView.ClearRewards();
            foreach (var reward in rewards)
            {
                rewardScrollView.AddOrUpdateReward(reward.Definition, reward.Amount);
            }
        }
    }
}