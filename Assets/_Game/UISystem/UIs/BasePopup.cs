using System;
using core.com.ui;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace com.core.ui
{
    public abstract class BasePopup : MonoBehaviour, IUI
    {
        public event Action<IUI> StartedShowing;
        public event Action<IUI> Showed;
        public event Action<IUI> Hidden;

        [SerializeField] private float   openingTime  = 0.35f;
        [SerializeField] private float   closingTime  = 0.25f;
        [SerializeField] private Vector3 initialScale = new(0.75f, 0.75f, 0.75f);
        [SerializeField] private Ease    openingEase  = Ease.OutBack;
        [SerializeField] private Ease    closingEase  = Ease.InBack;

        protected virtual Tweener PlayShowAnimation(Action completedCallback) => transform.DOScale(Vector3.one, openingTime).SetEase(openingEase).SetUpdate(true).OnComplete(() => completedCallback?.Invoke());
        protected virtual Tweener PlayHideAnimation(Action completedCallback) => transform.DOScale(initialScale, closingTime).SetEase(closingEase).SetUpdate(true).OnComplete(() => completedCallback?.Invoke());

        public UniTask ShowAsync()
        {
            StartedShowing?.Invoke(this);
            transform.SetAsLastSibling();
            var utcs = new UniTaskCompletionSource();
            transform.localScale = initialScale;
            gameObject.SetActive(true);

            PlayShowAnimation(() => OnAnimationShowed(utcs));
            return utcs.Task;
        }

        public void Show() => ShowAsync().Forget();

        public UniTask HideAsync()
        {
            var utcs = new UniTaskCompletionSource();
            PlayHideAnimation(() => OnAnimationCompleted(utcs));
            return utcs.Task;
        }

        private void OnAnimationShowed(UniTaskCompletionSource utcs)
        {
            Showed?.Invoke(this);
            utcs.TrySetResult();
        }

        private void OnAnimationCompleted(UniTaskCompletionSource utcs)
        {
            transform.SetAsFirstSibling();
            gameObject.SetActive(false);
            Hidden?.Invoke(this);
            utcs.TrySetResult();
        }

        public void Hide() => HideAsync().Forget();
    }
}