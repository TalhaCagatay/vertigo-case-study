using _Game.CardWheel.Data;
using UnityEngine;
using UnityEngine.UI;

namespace _Game.CardWheel.UIs
{
    /// <summary>
    /// The static pointer/indicator at the top of the wheel.
    /// Updates its sprite based on the current tier.
    /// </summary>
    public class CardWheelIndicator : MonoBehaviour
    {
        [SerializeField] private Image indicatorImage;

        /// <summary>
        /// Updates the indicator sprite based on the current tier config.
        /// </summary>
        public void Setup(WheelTierConfig config)
        {
            if (indicatorImage != null && config.IndicatorSprite != null)
            {
                indicatorImage.sprite = config.IndicatorSprite;
            }
        }
    }
}
