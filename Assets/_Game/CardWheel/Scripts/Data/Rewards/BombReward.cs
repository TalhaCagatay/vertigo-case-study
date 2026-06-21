using Vertigo.CardWheel.Data;
using UnityEngine;
using Vertigo.Player.Data;

namespace Vertigo.CardWheel.Data.Rewards
{
    [CreateAssetMenu(menuName = "Rewards/Bomb Reward")]
    public class BombReward : ARewardDefinition
    {
        public override void Grant(PlayerData playerData, int amount)
        {
        }
    }
}
