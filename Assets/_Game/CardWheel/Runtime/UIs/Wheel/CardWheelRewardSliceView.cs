using Reflex.Attributes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vertigo.CardWheel.Controller;
using Vertigo.CardWheel.Data;
using Vertigo.CardWheel.Data.Rewards;

namespace Vertigo.CardWheel.UIs.Screens
{
    public class CardWheelRewardSliceView : CardWheelSliceView
    {
        [SerializeField] private Image    iconImage;
        [SerializeField] private TMP_Text amountText;

        [Inject] private CardWheelController _cardWheelController;

#if UNITY_EDITOR
        private void OnValidate()
        {
            iconImage  = GetComponentInChildren<Image>();
            amountText = GetComponentInChildren<TMP_Text>();
        }
#endif

        public override void Setup(AWheelSliceDefinition wheelSliceDefinition)
        {
            if (wheelSliceDefinition is not ARewardDefinition rewardDefinition)
            {
                Debug.LogError($"[CardWheelRewardSliceView] RewardSliceView only accepts ARewardDefinition");
                return;
            }

            var scaledAmount = _cardWheelController.CurrentTierConfig.GetScaledRewardAmount(_cardWheelController.CurrentZone, rewardDefinition.Amount);

            iconImage.sprite = rewardDefinition.Icon;
            amountText.SetText($"x{scaledAmount}");
            gameObject.name = $"Slice_{rewardDefinition.Label}";
        }
    }
}