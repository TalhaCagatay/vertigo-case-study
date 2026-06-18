using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Game.CardWheel.UIs
{
    public class RewardEntry : MonoBehaviour
    {
        [SerializeField] private Image    iconImage;
        [SerializeField] private TMP_Text amountText;

        private int _currentAmount;

        public string Label { get; private set; }

        public RectTransform IconRectTransform => (RectTransform)iconImage.transform;

        public void Setup(Sprite icon, int amount, string label)
        {
            iconImage.sprite = icon;
            _currentAmount   = amount;
            amountText.text  = $"x{amount}";
            Label            = label;
        }

        public void PlayAddAnimation(int addedAmount, Action onComplete)
        {
            var startAmount = _currentAmount;
            var endAmount   = _currentAmount + addedAmount;

            var seq = DOTween.Sequence();

            // Count-up animation
            seq.Join
                (
                 DOTween.To
                     (
                      value =>
                      {
                          _currentAmount  = Mathf.RoundToInt(value);
                          amountText.text = $"x{_currentAmount}";
                      },
                      startAmount,
                      endAmount,
                      0.5f
                     ).SetEase(Ease.OutCubic)
                );

            seq.Join(iconImage.transform.DOScale(1.3f, 0.2f).SetEase(Ease.OutBack));
            seq.Append(iconImage.transform.DOScale(1f, 0.3f).SetEase(Ease.InBack));

            seq.OnComplete(() => onComplete?.Invoke());
        }
    }
}
