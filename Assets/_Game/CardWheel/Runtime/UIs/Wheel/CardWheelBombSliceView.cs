using UnityEngine;
using UnityEngine.UI;
using Vertigo.CardWheel.Data;
using Vertigo.CardWheel.Data.Rewards;

namespace Vertigo.CardWheel.UIs.Screens
{
    public class CardWheelBombSliceView : CardWheelSliceView
    {
        [SerializeField] private Image iconImage;

#if UNITY_EDITOR
        private void OnValidate() => iconImage = GetComponentInChildren<Image>();
#endif

        public override void Setup(AWheelSliceDefinition wheelSliceDefinition)
        {
            if (wheelSliceDefinition is not Bomb)
            {
                Debug.LogError("[CardWheelBombSliceView] BombSliceView only accepts BombReward");
                return;
            }

            iconImage.sprite = wheelSliceDefinition.Icon;
            gameObject.name  = $"Slice_{wheelSliceDefinition.Label}";
        }
    }
}