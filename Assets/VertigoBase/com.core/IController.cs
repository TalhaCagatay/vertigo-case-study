using Cysharp.Threading.Tasks;

namespace com.core
{
    public interface IController
    {
        bool IsInitialized { get; }

        UniTask Initialize();
    }
}