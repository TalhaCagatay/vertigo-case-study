using Vertigo.CardWheel.Data.Rewards;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Vertigo.CardWheel.UIs.Screens
{
    public class CardWheelSliceView : MonoBehaviour
    {
        [SerializeField] private Image    iconImage;
        [SerializeField] private TMP_Text amountText;

#if UNITY_EDITOR
        private void OnValidate()
        {
            iconImage  = GetComponentInChildren<Image>();
            amountText = GetComponentInChildren<TMP_Text>();
        }
#endif

        public void Setup(ARewardDefinition rewardDefinition, int scaledAmount)
        {
            // disabling tmp_text causes layout group to increase icon size so bomb icon looks bigger & better.
            amountText.gameObject.SetActive(rewardDefinition is not BombReward);

            iconImage.sprite  = rewardDefinition.Icon;
            iconImage.enabled = true;
            amountText.SetText($"x{scaledAmount}");
            gameObject.name = $"Slice_{rewardDefinition.Label}";
        }

        public void SetAmountText(string text) => amountText.SetText(text);
    }
}