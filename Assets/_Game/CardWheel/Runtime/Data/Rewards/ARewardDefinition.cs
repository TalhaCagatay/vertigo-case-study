using UnityEngine;
using Vertigo.CardWheel.Controller;
using Vertigo.CardWheel.State;
using Vertigo.Player;

namespace Vertigo.CardWheel.Data.Rewards
{
    public abstract class ARewardDefinition : AWheelSliceDefinition
    {
        [SerializeField] private int amount;

        public int Amount => amount;

        public virtual void Grant(PlayerController playerController, int value)
        {
            playerController.AddReward(id, value);
        }

        public override void Apply(CardWheelController cardWheelController)
        {
            var scaledAmount = cardWheelController.CurrentTierConfig.GetScaledRewardAmount(cardWheelController.CurrentZone, Amount);

            cardWheelController.AddReward(this, scaledAmount);
            cardWheelController.SetState(WheelState.Result);
        }
    }
}