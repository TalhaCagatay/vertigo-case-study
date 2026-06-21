using System.Collections.Generic;
using MemoryPack;

namespace Vertigo.Player.Data
{
    [MemoryPackable]
    public partial class PlayerData
    {
        public int                     CoinBalance = 1000;
        public Dictionary<string, int> Rewards     = new();
        
        public void AddReward(string id, int amount)
        {
            if (!Rewards.TryAdd(id, amount)) Rewards[id] += amount;
        }

        public int  GetRewardAmount(string id)     => Rewards.GetValueOrDefault(id, 0);
        public void AddCoins(int           amount) => CoinBalance += amount;
        public bool HasEnoughCoins(int     amount) => CoinBalance >= amount;

        public bool SpendCoins(int amount)
        {
            if (!HasEnoughCoins(amount)) return false;

            CoinBalance -= amount;
            return true;
        }
    }
}