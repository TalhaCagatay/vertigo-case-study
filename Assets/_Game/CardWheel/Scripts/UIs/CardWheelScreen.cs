using com.core.ui;
using DG.Tweening;
using UnityEngine;
#if UNITY_EDITOR
using UnityEngine.Assertions;
#endif

namespace _Game.CardWheel.UIs
{
    public class CardWheelScreen : BaseScreen
    {
        [SerializeField, HideInInspector] private CardWheelSpinner   cardWheelSpinner;
        [SerializeField, HideInInspector] private CardWheelIndicator cardWheelIndicator;
        [SerializeField]                  private float              spinDuration = 3f;

#if UNITY_EDITOR
        /// <summary>
        /// This is not how I usually handle references but the case study documents suggest doing it this way,
        /// I usually use OnValidate to validate if the references are assigned through editor.
        /// </summary>
        private void OnValidate()
        {
            cardWheelSpinner   = GetComponentInChildren<CardWheelSpinner>();
            cardWheelIndicator = GetComponentInChildren<CardWheelIndicator>();

            Assert.IsNotNull(cardWheelSpinner,   "CardWheelSpinner component not found in children");
            Assert.IsNotNull(cardWheelIndicator, "CardWheelIndicator component not found in children");
        }
#endif

        private void Awake()
        {
            cardWheelSpinner.transform.DORotate(Vector3.forward * -360f, spinDuration, RotateMode.FastBeyond360).SetEase(Ease.Linear).SetLoops(-1);
        }
    }
}