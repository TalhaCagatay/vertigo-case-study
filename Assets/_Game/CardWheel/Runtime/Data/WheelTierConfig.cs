using System.Linq;
using Vertigo.CardWheel.Data.Rewards;
using UnityEngine;

namespace Vertigo.CardWheel.Data
{
    [CreateAssetMenu(fileName = "WheelTierConfig", menuName = "CardWheel/Wheel Tier Config", order = 2)]
    public class WheelTierConfig : ScriptableObject
    {
        [SerializeField] private Sprite              spinnerSprite;
        [SerializeField] private Sprite              indicatorSprite;
        [SerializeField] private ARewardDefinition[] slices                  = new ARewardDefinition[8];
        [SerializeField] private Color               zoneNumberSelectedColor = Color.black;
        [SerializeField] private Color               zoneNumberPastColor     = Color.black;
        [SerializeField] private Color               zoneNumberFutureColor   = Color.black;
        [SerializeField] private float               spinDuration            = 3f;
        [SerializeField] private float               rewardMultiplier        = 10;
        [SerializeField] private AnimationCurve      rewardScaleCurve        = AnimationCurve.Linear(0, 1, 100, 10);

        public Sprite              SpinnerSprite           => spinnerSprite;
        public Sprite              IndicatorSprite         => indicatorSprite;
        public ARewardDefinition[] Slices                  => slices;
        public Color               ZoneNumberSelectedColor => zoneNumberSelectedColor;
        public Color               ZoneNumberPastColor     => zoneNumberPastColor;
        public Color               ZoneNumberFutureColor   => zoneNumberFutureColor;
        public bool                HasBomb                 => slices.Any(r => r is BombReward);
        public float               SpinDuration            => spinDuration;
        public AnimationCurve      RewardScaleCurve        => rewardScaleCurve;

        public int GetScaledRewardAmount(int zone, int amount)
        {
            float t          = 1f - Mathf.Exp(-zone / 100f);
            float multiplier = rewardScaleCurve.Evaluate(t);
            return amount + Mathf.RoundToInt(multiplier * amount * rewardMultiplier);
        }
    }
}