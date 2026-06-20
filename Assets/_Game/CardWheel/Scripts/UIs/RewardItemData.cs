using UnityEngine;

namespace _Game.CardWheel.UIs
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