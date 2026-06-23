using TMPro;
using UnityEngine;
using UnityEngine.UI.Extensions;

namespace Vertigo.CardWheel.UIs.ZoneScroll
{
    class ZoneCell : FancyCell<ZoneItemData, ZoneContext>
    {
        [SerializeField] private TMP_Text zoneText;
        [SerializeField] private float    spacingPixels      = 160f;
        [SerializeField] private float    normalizedInterval = 0.2f;

        private float _currentPosition = 0f;

#if UNITY_EDITOR
        private void OnValidate()
        {
            zoneText = GetComponentInChildren<TMP_Text>();
        }
#endif

        public override void Initialize()
        {
        }

        public override void UpdateContent(ZoneItemData itemData)
        {
            zoneText.SetText(itemData.ZoneNumber.ToString());
            var selected = Context != null && Context.SelectedIndex == Index;
            if (selected)
            {
                zoneText.color       = itemData.ZoneNumberSelectedColor;
                transform.localScale = Vector3.one * 1.12f;
            }
            else if (itemData.IsPastZone)
            {
                zoneText.color       = itemData.ZoneNumberPastColor;
                transform.localScale = Vector3.one;
            }
            else
            {
                zoneText.color       = itemData.ZoneNumberFutureColor;
                transform.localScale = Vector3.one;
            }
        }

        public override void UpdatePosition(float position)
        {
            _currentPosition = position;

            var rt = transform as RectTransform;
            var x  = (position - 0.5f) / Mathf.Max(1e-4f, normalizedInterval) * spacingPixels;
            rt.anchoredPosition = new Vector2(x, rt.anchoredPosition.y);
        }

        private void OnEnable() => UpdatePosition(_currentPosition);
    }
}