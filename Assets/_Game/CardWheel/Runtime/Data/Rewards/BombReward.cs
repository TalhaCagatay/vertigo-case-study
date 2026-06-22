using UnityEngine;
using Vertigo.Player;

namespace Vertigo.CardWheel.Data.Rewards
{
    [CreateAssetMenu(menuName = "Rewards/Bomb Reward")]
    public class BombReward : ARewardDefinition
    {
        public override void Grant(PlayerController _, int __)
        {
        }
    }
}
