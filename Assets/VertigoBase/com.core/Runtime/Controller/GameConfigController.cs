using Cysharp.Threading.Tasks;
using UnityEngine;

namespace com.core
{
    public class GameConfigController : AController
    {
        public override bool IsInitialized { get; protected set; }

        public override UniTask Initialize()
        {
            Application.targetFrameRate  = 60;
            Debug.unityLogger.logEnabled = true;

            IsInitialized = true;
            return UniTask.CompletedTask;
        }
    }
}