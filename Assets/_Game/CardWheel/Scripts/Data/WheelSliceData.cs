using UnityEngine;

namespace _Game.CardWheel.Data
{
    [CreateAssetMenu(fileName = "WheelSliceData", menuName = "CardWheel/Wheel Slice Data", order = 1)]
    public class WheelSliceData : ScriptableObject
    {
        [SerializeField] private string rewardType; // Flexible: "Coin", "BronzeCrate", "GoldCrate", "BaseballHat", "Sunglasses", "BayonetteKnife", "PistolPoints", "RiflePoints", "Bomb", etc.
        [SerializeField] private Sprite icon;
        [SerializeField] private int    amount;
        [SerializeField] private string label;
        [SerializeField] private bool   isBomb;

        public string RewardType => rewardType;
        public Sprite Icon       => icon;
        public int    Amount     => amount;
        public string Label      => label;
        public bool   IsBomb     => isBomb;
    }
}
