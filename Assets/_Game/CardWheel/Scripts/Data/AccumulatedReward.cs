using System;
using UnityEngine;

namespace _Game.CardWheel.Data
{
    /// <summary>
    /// Represents a stack of a single reward type collected during the wheel session.
    /// </summary>
    [Serializable]
    public class AccumulatedReward
    {
        public string RewardType { get; }
        public Sprite Icon       { get; }
        public string Label      { get; }
        public int    Amount     { get; private set; }

        public AccumulatedReward(string rewardType, Sprite icon, string label, int amount)
        {
            RewardType = rewardType;
            Icon       = icon;
            Label      = label;
            Amount     = amount;
        }

        public void Add(int amount)
        {
            Amount += amount;
        }
    }
}
