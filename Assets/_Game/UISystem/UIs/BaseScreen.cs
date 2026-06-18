using System;
using core.com.ui;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace com.core.ui
{
    public abstract class BaseScreen : MonoBehaviour, IUI
    {
        public event Action<IUI> Showed;
        public event Action<IUI> Hidden;

        public virtual UniTask ShowAsync()
        {
            gameObject.SetActive(true);
            Showed?.Invoke(this);
            return UniTask.CompletedTask;
        }

        public UniTask HideAsync()
        {
            gameObject.SetActive(false);
            Hidden?.Invoke(this);
            return UniTask.CompletedTask;
        }

        private void OnDestroy() => Hidden?.Invoke(this); // workaround for instantiated and destroyed UIs
    }
}