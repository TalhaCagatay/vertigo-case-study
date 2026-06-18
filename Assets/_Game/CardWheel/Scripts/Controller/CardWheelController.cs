using com.core;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class CardWheelController : IController
{
    public bool IsInitialized { get; private set; }

    public CardWheelController()
    {
        Debug.Log($"ctor CardWheelController");
        Initialize();
    }
    
    public UniTask Initialize()
    {
        Debug.Log($"CardWheelController initialized");
        IsInitialized = true;
        return UniTask.CompletedTask;
    }
}