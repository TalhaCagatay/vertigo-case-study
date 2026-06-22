using System;
using System.Collections.Generic;
using com.core;
using com.core.data;
using Cysharp.Threading.Tasks;
using Vertigo.Player.Data;

namespace Vertigo.Player
{
    public class PlayerController : IController
    {
        public event Action<int> BalanceUpdated;

        private const string PLAYER_DATA_SAVE_KEY = "player-data-key";

        private readonly DataController _dataController;

        private PlayerData _playerData;

        public IReadOnlyDictionary<string, int> Rewards       => _playerData.Rewards;
        public int                              CoinBalance   => _playerData.CoinBalance;
        public bool                             IsInitialized { get; private set; }

        public PlayerController(DataController dataController)
        {
            _dataController = dataController;
            Initialize();
        }

        public UniTask Initialize()
        {
            _playerData   = _dataController.Load(PLAYER_DATA_SAVE_KEY, new PlayerData());
            IsInitialized = true;
            return UniTask.CompletedTask;
        }

        public void AddCoins(int amount)
        {
            _playerData.AddCoins(amount);
            Save();
            BalanceUpdated?.Invoke(_playerData.CoinBalance);
        }

        public void AddReward(string id, int amount)
        {
            _playerData.AddReward(id, amount);
            Save();
        }

        public bool HasEnoughCoins(int amount) => _playerData.HasEnoughCoins(amount);

        public bool SpendCoins(int amount)
        {
            if (!_playerData.SpendCoins(amount)) return false;

            Save();
            BalanceUpdated?.Invoke(_playerData.CoinBalance);
            return true;
        }

        private void Save() => _dataController.Save(PLAYER_DATA_SAVE_KEY, _playerData);

        public async UniTask DeletePlayerData()
        {
            await _dataController.Delete(PLAYER_DATA_SAVE_KEY);
            _playerData = new();
            BalanceUpdated?.Invoke(_playerData.CoinBalance);
        }
    }
}