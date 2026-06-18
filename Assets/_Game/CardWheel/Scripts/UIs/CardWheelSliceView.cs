using _Game.CardWheel.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Game.CardWheel.UIs
{
    public class CardWheelSliceView : MonoBehaviour
    {
        [SerializeField] private Image    iconImage;
        [SerializeField] private TMP_Text amountText;

        public void Setup(WheelSliceData sliceData, int scaledAmount)
        {
            iconImage.sprite  = sliceData.Icon;
            iconImage.enabled = true;
            amountText.text   = $"x{scaledAmount}";
            gameObject.name   = $"Slice_{sliceData.Label}";
        }

        public void SetAmountText(string text) => amountText.text = text;
    }
}
