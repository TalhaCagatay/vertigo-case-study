using System;
using UnityEngine;

namespace _Game.CardWheel.Data
{
    [Serializable]
    public class AccumulatedReward
    {
        public string RewardType { get; }
        public Sprite Icon       { get; }
        public string Id         { get; }
        public string Label      { get; }
        public int    Amount     { get; private set; }

        public AccumulatedReward(string rewardType, Sprite icon, string id, string label, int amount)
        {
            RewardType = rewardType;
            Icon       = icon;
            Id         = id;
            Label      = label;
            Amount     = amount;
        }

        public void Add(int amount) => Amount += amount;
    }
}