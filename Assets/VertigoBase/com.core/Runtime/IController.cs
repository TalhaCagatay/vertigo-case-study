using Cysharp.Threading.Tasks;

namespace com.core
{
    // todo: this interface is pretty useless right now with the use of Reflex DI, this can be easily removed
    public interface IController
    {
        int  Order         { get; }
        bool IsInitialized { get; }

        UniTask Initialize();
    }
}