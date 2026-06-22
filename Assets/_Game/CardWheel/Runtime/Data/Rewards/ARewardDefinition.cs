using UnityEngine;
using Vertigo.Player;

namespace Vertigo.CardWheel.Data.Rewards
{
    public abstract class ARewardDefinition : ScriptableObject
    {
        public string id;

        [SerializeField] private Sprite icon;
        [SerializeField] private int    amount;
        [SerializeField] private string label;

        public string Id     => id;
        public Sprite Icon   => icon;
        public int    Amount => amount;
        public string Label  => label;

        public virtual void Grant(PlayerController playerController, int value)
        {
            playerController.AddReward(id, value);
        }
    }
}