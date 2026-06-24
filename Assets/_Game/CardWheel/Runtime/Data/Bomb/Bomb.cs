using UnityEngine;
using Vertigo.CardWheel.Controller;

namespace Vertigo.CardWheel.Data.Rewards
{
    [CreateAssetMenu(menuName = "Wheel Slice/Bomb")]
    public class Bomb : AWheelSliceDefinition
    {
        public override void Apply(CardWheelController cardWheelController)
        {
            cardWheelController.SetGameOver();
        }
    }
}