using Cysharp.Threading.Tasks;

namespace com.core
{
    public interface IController
    {
        int  Order         { get; }
        bool IsInitialized { get; }

        UniTask Initialize();
    }
}