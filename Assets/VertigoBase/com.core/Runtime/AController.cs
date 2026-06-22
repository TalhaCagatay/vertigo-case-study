using Cysharp.Threading.Tasks;

namespace com.core
{
    public abstract class AController : IController
    {
        public int Order { get; }

        public abstract bool IsInitialized { get; protected set; }

        protected AController() => Order = InitOrder.Number;

        public abstract UniTask Initialize();
    }
}