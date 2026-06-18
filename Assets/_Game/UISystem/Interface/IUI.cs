using System;
using Cysharp.Threading.Tasks;

namespace core.com.ui
{
    public interface IUI
    {
        event Action<IUI> Showed;
        event Action<IUI> Hidden;

        UniTask ShowAsync();
        UniTask HideAsync();
    }
}