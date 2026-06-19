using _Game.CardWheel.Data;
using _Game.CardWheel.Data.Rewards;
using UnityEngine;

[CreateAssetMenu(menuName = "Rewards/Bomb Reward")]
public class BombReward : ARewardDefinition
{
    public override void Grant(PlayerData playerData, int amount)
    {
    }
}