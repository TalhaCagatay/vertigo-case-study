using Cysharp.Threading.Tasks;
using UnityEngine;

namespace com.core
{
    public class GameConfigController : IController
    {
        public int  Order         { get; }
        public bool IsInitialized { get; private set; }

        public GameConfigController()
        {
            Order = InitOrder.Number;
        }
        
        public UniTask Initialize()
        {
            Application.targetFrameRate  = 60;
            Debug.unityLogger.logEnabled = true;
            
            IsInitialized = true;
            return UniTask.CompletedTask;
        }
    }
}