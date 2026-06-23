using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

namespace Vertigo.CardWheel.UIs.Rewards
{
    public class RewardCell : FancyScrollRectCell<RewardItemData, RewardContext>
    {
        [SerializeField] private Image                 iconImage;
        [SerializeField] private TMP_Text              amountText;
        [SerializeField] private HorizontalLayoutGroup horizontalLayoutGroup;
        [SerializeField] private ContentSizeFitter     contentSizeFitter;

        private int _currentAmount;

        public string Id { get; private set; }

        public RectTransform IconRectTransform => (RectTransform)iconImage.transform;

#if UNITY_EDITOR
        private void OnValidate()
        {
            iconImage             = GetComponentInChildren<Image>();
            amountText            = GetComponentInChildren<TMP_Text>();
            horizontalLayoutGroup = GetComponent<HorizontalLayoutGroup>();
            contentSizeFitter     = GetComponent<ContentSizeFitter>();
        }
#endif

        public override void UpdateContent(RewardItemData itemData)
        {
            Id               = itemData.Id;
            iconImage.sprite = itemData.Icon;
            _currentAmount   = itemData.Amount;
            amountText.SetText($"x{_currentAmount}");
        }

        public void Clear() => Id = string.Empty;

        public void PlayAddAnimation(int addedAmount, Action onComplete)
        {
            var startAmount = _currentAmount;
            var endAmount   = _currentAmount + addedAmount;

            var seq = DOTween.Sequence();

            seq.Join
                (
                 DOTween.To
                     (
                      value =>
                      {
                          _currentAmount = Mathf.RoundToInt(value);
                          amountText.SetText($"x{_currentAmount}");
                      },
                      startAmount,
                      endAmount,
                      0.5f
                     ).SetEase(Ease.OutCubic)
                );

            horizontalLayoutGroup.enabled = false; // a small hack to disable layout to play the animation properly and enable it later
            contentSizeFitter.enabled     = false;
            iconImage.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            seq.Join(iconImage.rectTransform.DOScale(1.3f, 0.2f).SetEase(Ease.OutBack));
            seq.Append(iconImage.rectTransform.DOScale(1f, 0.3f).SetEase(Ease.InBack));

            seq.OnComplete
                (
                 () =>
                 {
                     onComplete?.Invoke();
                     horizontalLayoutGroup.enabled = true;
                     contentSizeFitter.enabled     = true;
                 }
                );
        }
    }
}