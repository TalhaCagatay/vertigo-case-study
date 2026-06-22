using UnityEngine;
using Vertigo.Player;

namespace Vertigo.CardWheel.Data.Rewards
{
    [CreateAssetMenu(menuName = "Rewards/Coin Reward")]
    public class CoinReward : ARewardDefinition
    {
        public override void Grant(PlayerController playerController, int value)
        {
            playerController.AddCoins(value);
        }
    }
}