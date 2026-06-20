using Vertigo.CardWheel.Data;
using UnityEngine;
using UnityEngine.UI;

namespace Vertigo.CardWheel.UIs.Wheel
{
    public class CardWheelIndicator : MonoBehaviour
    {
        [SerializeField, HideInInspector] private Image indicatorImage;

#if UNITY_EDITOR
        private void OnValidate() => indicatorImage = GetComponent<Image>();
#endif
        public void Setup(WheelTierConfig config) => indicatorImage.sprite = config.IndicatorSprite;
    }
}