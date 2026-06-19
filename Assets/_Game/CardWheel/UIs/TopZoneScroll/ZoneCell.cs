using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

namespace _Game.CardWheel.UIs.TopZoneScroll
{
    class ZoneCell : FancyCell<ZoneItemData, ZoneContext>
    {
        [SerializeField] private TMP_Text zoneText;
        [SerializeField] private Image    background;
        [SerializeField] private float    spacingPixels = 160f;
        [SerializeField] private float    normalizedInterval = 0.2f;
        
        private float _currentPosition = 0f;

        private void OnValidate()
        {
            zoneText   = GetComponentInChildren<TMP_Text>();
            background = GetComponentInChildren<Image>();
        }

        public override void Initialize()
        {
        }

        public override void UpdateContent(ZoneItemData itemData)
        {
            zoneText.text = itemData.ZoneNumber.ToString();

            if (itemData.IsSuperZone) background.color     = new Color32(255, 215, 0,   255);
            else if (itemData.IsSafeZone) background.color = new Color32(173, 216, 230, 255);
            else background.color                          = Color.white;

            var selected = Context != null && Context.SelectedIndex == Index;
            if (selected)
            {
                zoneText.color = Color.black;
                transform.localScale = Vector3.one * 1.12f;
            }
            else if (itemData.IsPastZone)
            {
                zoneText.color = Color.gray;
                transform.localScale = Vector3.one;
            }
            else
            {
                zoneText.color = Color.black;
                transform.localScale = Vector3.one;
            }
        }

        public override void UpdatePosition(float position)
        {
            _currentPosition = position;

            var rt = transform as RectTransform;
            if (rt == null) return;

            var x = (position - 0.5f) / Mathf.Max(1e-4f, normalizedInterval) * spacingPixels;
            rt.anchoredPosition = new Vector2(x, rt.anchoredPosition.y);
        }
        
        private void OnEnable() => UpdatePosition(_currentPosition);
    }
}