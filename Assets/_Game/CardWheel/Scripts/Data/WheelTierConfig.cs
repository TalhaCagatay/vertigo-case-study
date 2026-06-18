using System.Collections.Generic;
using UnityEngine;

namespace _Game.CardWheel.Data
{
    [CreateAssetMenu(fileName = "WheelTierConfig", menuName = "CardWheel/Wheel Tier Config", order = 2)]
    public class WheelTierConfig : ScriptableObject
    {
        [SerializeField] private WheelTierType tierType;
        [SerializeField] private Sprite        spinnerSprite;
        [SerializeField] private Sprite        indicatorSprite;
        [SerializeField] private WheelSliceData[] slices = new WheelSliceData[8];
        [SerializeField] private int           bombIndex = -1; // -1 means no bomb (safe zone)
        [SerializeField] private float         spinDuration = 3f;
        [SerializeField] private AnimationCurve rewardScaleCurve = AnimationCurve.Linear(0, 1, 100, 10);

        public WheelTierType TierType         => tierType;
        public Sprite        SpinnerSprite    => spinnerSprite;
        public Sprite        IndicatorSprite  => indicatorSprite;
        public WheelSliceData[] Slices        => slices;
        public int           BombIndex        => bombIndex;
        public bool          HasBomb          => bombIndex >= 0 && bombIndex < slices.Length;
        public float         SpinDuration     => spinDuration;
        public AnimationCurve RewardScaleCurve => rewardScaleCurve;

        /// <summary>
        /// Returns the reward multiplier for a given zone number based on the configured curve.
        /// </summary>
        public float GetRewardMultiplier(int zone)
        {
            return rewardScaleCurve.Evaluate(zone);
        }
    }
}
