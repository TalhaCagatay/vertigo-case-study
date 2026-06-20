using System;
using Vertigo.CardWheel.Data;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Vertigo.CardWheel.UIs.Wheel
{
    public class CardWheelSpinner : MonoBehaviour
    {
        [SerializeField] private Image                spinnerImage;
        [SerializeField] private Transform            slicesContainer;
        [SerializeField] private CardWheelSliceView[] sliceViews;

        private Tweener _spinTween;

        private const float ANGLE_PER_SLICE = 360f / 8f;

        private void OnValidate()
        {
            spinnerImage    = GetComponent<Image>();
            slicesContainer = transform;
            sliceViews      = slicesContainer.GetComponentsInChildren<CardWheelSliceView>();
        }

        public void Setup(WheelTierConfig config, int currentZone)
        {
            spinnerImage.sprite = config.SpinnerSprite;

            var multiplier = config.GetRewardMultiplier(currentZone);

            for (int i = 0; i < Mathf.Min(sliceViews.Length, config.Slices.Length); i++)
            {
                var sliceData    = config.Slices[i];
                var scaledAmount = Mathf.RoundToInt(sliceData.Amount * multiplier);
                sliceViews[i].Setup(sliceData, scaledAmount);
            }
        }

        public Vector3 GetSliceIconWorldPosition(int sliceIndex) => sliceViews[sliceIndex].transform.position;

        public void SpinToIndex(int targetSliceIndex, float duration, Action onComplete = null)
        {
            _spinTween?.Kill();

            const int FULL_SPINS = 5;

            var sliceAngle  = targetSliceIndex * ANGLE_PER_SLICE;
            var totalSpins  = FULL_SPINS * 360f;
            var overshoot   = UnityEngine.Random.Range(5f, 22.5f);
            var targetAngle = sliceAngle - totalSpins - overshoot;

            _spinTween = transform.DORotate(new Vector3(0, 0, targetAngle), duration, RotateMode.FastBeyond360).SetEase(Ease.InOutCubic).OnComplete
                (
                 () =>
                 {
                     transform.DORotate(new Vector3(0, 0, sliceAngle), 0.2f).SetEase(Ease.OutBack).OnComplete(() => onComplete?.Invoke());
                 }
                );
        }

        public void ResetRotation()
        {
            _spinTween?.Kill();
            transform.localRotation = Quaternion.identity;
        }

        private void OnDestroy() => _spinTween?.Kill();
    }
}