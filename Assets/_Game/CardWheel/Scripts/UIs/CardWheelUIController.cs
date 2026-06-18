using com.core;
using com.core.ui;
using Cysharp.Threading.Tasks;

namespace _Game.CardWheel.UIs
{
    public class CardWheelUIController : IController
    {
        private readonly UIController _uiController;

        public bool IsInitialized { get; private set; }

        public CardWheelUIController(UIController uiController)
        {
            _uiController = uiController;
            Initialize();
        }

        public UniTask Initialize()
        {
            _uiController.ShowScreen<CardWheelScreen>();
            IsInitialized = true;
            return UniTask.CompletedTask;
        }
    }
}