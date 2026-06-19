using UnityEngine;

namespace _Game.CardWheel.Data.Rewards
{
    public abstract class ARewardDefinition : ScriptableObject
    {
        public string id;

        [SerializeField] private Sprite icon;
        [SerializeField] private int    amount;
        [SerializeField] private string label;

        public string RewardType => id;
        public Sprite Icon       => icon;
        public int    Amount     => amount;
        public string Label      => label;

        public virtual void Grant(PlayerData playerData, int value)
        {
            playerData.Save(id, value);
        }
    }
}