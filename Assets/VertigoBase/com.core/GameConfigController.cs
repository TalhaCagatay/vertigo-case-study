using Cysharp.Threading.Tasks;
using UnityEngine;

namespace com.core
{
    public class GameConfigController : IController
    {
        public bool IsInitialized { get; private set; }

        public GameConfigController() => Initialize();
        
        public UniTask Initialize()
        {
            Application.targetFrameRate = 60;
            
            IsInitialized = true;
            return UniTask.CompletedTask;
        }
    }
}