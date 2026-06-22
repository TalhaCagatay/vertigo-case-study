using UnityEngine;

namespace Vertigo.CardWheel.UIs.Rewards
{
    public class RewardItemData
    {
        public string Id     { get; }
        public Sprite Icon   { get; }
        public string Label  { get; }
        public int    Amount { get; set; }

        public RewardItemData(string id, Sprite icon, string label, int amount)
        {
            Id     = id;
            Icon   = icon;
            Label  = label;
            Amount = amount;
        }
    }
}