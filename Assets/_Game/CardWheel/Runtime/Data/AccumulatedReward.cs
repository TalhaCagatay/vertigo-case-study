using System;
using UnityEngine;
using Vertigo.CardWheel.Data.Rewards;

namespace Vertigo.CardWheel.Data
{
    [Serializable]
    public class AccumulatedReward
    {
        public ARewardDefinition Definition { get; }
        public Sprite            Icon       { get; }
        public string            Id         { get; }
        public string            Label      { get; }
        public int               Amount     { get; private set; }

        public AccumulatedReward(ARewardDefinition definition, int amount)
        {
            Definition = definition;
            Id         = definition.id;
            Icon       = definition.Icon;
            Label      = definition.Label;
            Amount     = amount;
        }

        public void Add(int amount) => Amount += amount;
    }
}
