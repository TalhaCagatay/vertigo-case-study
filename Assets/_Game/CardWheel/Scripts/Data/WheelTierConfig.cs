using System.Linq;
using _Game.CardWheel.Data.Rewards;
using UnityEngine;

namespace _Game.CardWheel.Data
{
    [CreateAssetMenu(fileName = "WheelTierConfig", menuName = "CardWheel/Wheel Tier Config", order = 2)]
    public class WheelTierConfig : ScriptableObject
    {
        [SerializeField] private WheelTierType       tierType;
        [SerializeField] private Sprite              spinnerSprite;
        [SerializeField] private Sprite              indicatorSprite;
        [SerializeField] private ARewardDefinition[] slices                  = new ARewardDefinition[8];
        [SerializeField] private Color               zoneNumberSelectedColor = Color.black;
        [SerializeField] private Color               zoneNumberPastColor     = Color.black;
        [SerializeField] private Color               zoneNumberFutureColor   = Color.black;
        [SerializeField] private float               spinDuration            = 3f;
        [SerializeField] private AnimationCurve      rewardScaleCurve        = AnimationCurve.Linear(0, 1, 100, 10);

        public WheelTierType       TierType                => tierType;
        public Sprite              SpinnerSprite           => spinnerSprite;
        public Sprite              IndicatorSprite         => indicatorSprite;
        public ARewardDefinition[] Slices                  => slices;
        public Color               ZoneNumberSelectedColor => zoneNumberSelectedColor;
        public Color               ZoneNumberPastColor     => zoneNumberPastColor;
        public Color               ZoneNumberFutureColor   => zoneNumberFutureColor;
        public bool                HasBomb                 => slices.Any(r => r is BombReward);
        public float               SpinDuration            => spinDuration;
        public AnimationCurve      RewardScaleCurve        => rewardScaleCurve;

        public float GetRewardMultiplier(int zone)
        {
            return rewardScaleCurve.Evaluate(zone);
        }
    }
}