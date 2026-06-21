using UnityEngine;

namespace Vertigo.CardWheel.Data.Rewards
{
    [CreateAssetMenu(menuName = "Rewards/Coin Reward")]
    public class CoinReward : ARewardDefinition
    {
        public override void Grant(PlayerData playerData, int value)
        {
            playerData.AddCoins(value);
        }
    }
}
