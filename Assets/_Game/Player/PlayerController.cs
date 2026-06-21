using com.core;
using com.core.data;
using Cysharp.Threading.Tasks;
using Vertigo.Player.Data;

namespace Vertigo.Player
{
    public class PlayerController : IController
    {
        public const string PLAYER_DATA_SAVE_KEY = "player-data-key";

        private DataController _dataController;

        public PlayerData PlayerData { get; private set; }

        public bool IsInitialized { get; private set; }

        public PlayerController(DataController dataController)
        {
            _dataController = dataController;
            Initialize();
        }

        public UniTask Initialize()
        {
            PlayerData    = _dataController.Load(PLAYER_DATA_SAVE_KEY, new PlayerData());
            IsInitialized = true;
            return UniTask.CompletedTask;
        }

        public void       Save() => _dataController.Save(PLAYER_DATA_SAVE_KEY, PlayerData);
        public PlayerData Load() => _dataController.Load(PLAYER_DATA_SAVE_KEY, new PlayerData());
    }
}