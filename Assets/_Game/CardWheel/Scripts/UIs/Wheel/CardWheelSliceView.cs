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

        public void Setup(ARewardDefinition sliceData, int scaledAmount)
        {
            iconImage.sprite  = sliceData.Icon;
            iconImage.enabled = true;
            amountText.text   = $"x{scaledAmount}";
            gameObject.name   = $"Slice_{sliceData.Label}";
        }

        public void SetAmountText(string text) => amountText.text = text;
    }
}