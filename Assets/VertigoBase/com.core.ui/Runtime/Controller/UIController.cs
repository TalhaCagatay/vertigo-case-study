using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace com.core.ui
{
    public class UIController : IController
    {
        private const string UI_PARENT_PATH    = "UIPrefabs/UIParent";
        private const string POPUPS_RESOURCES  = "UIPrefabs/Popups";
        private const string SCREENS_RESOURCES = "UIPrefabs/Screens";

        private readonly Stack<BasePopup>             _popupStack  = new();
        private readonly Dictionary<Type, BasePopup>  _popupCache  = new();
        private readonly Dictionary<Type, BaseScreen> _ScreenCache = new();

        private BaseScreen _currentScreen;
        private UIParent   _uiParent;

        public bool IsInitialized { get; private set; }

        public UIController()
        {
            Debug.Log($"UIController ctor");
            Initialize();
        }
        
        public UniTask Initialize()
        {
            var uiParentPrefab = Resources.Load<UIParent>(UI_PARENT_PATH);
            if (uiParentPrefab == null)
            {
                Debug.LogError($"[UIController] UIParent prefab not found at {UI_PARENT_PATH}. Ensure it exists in a Resources folder.");
                return UniTask.CompletedTask;
            }

            _uiParent      = Object.Instantiate(uiParentPrefab);
            _uiParent.name = "UIParent";

            IsInitialized = true;
            return UniTask.CompletedTask;
        }

        public async UniTask<TPopup> PushPopupAsync<TPopup>() where TPopup : BasePopup
        {
            var popup = GetOrCreatePopup<TPopup>();
            _popupStack.Push(popup);
            var popupTask = popup.ShowAsync();
            UpdateBackgroundForTopPopup();
            await popupTask;
            return popup;
        }

        public void PushPopup<TPopup>() where TPopup : BasePopup => PushPopupAsync<TPopup>().Forget();

        public async UniTask PopPopupAsync()
        {
            if (_popupStack.Count == 0)
            {
                Debug.LogWarning("[UIController] PopPopup called but popup stack is empty.");
                return;
            }

            var top = _popupStack.Pop();
            await top.HideAsync();
            UpdateBackgroundForTopPopup();
        }

        public void PopPopup() => PopPopupAsync().Forget();

        public BasePopup PeekPopup() => _popupStack.Count > 0 ? _popupStack.Peek() : null;

        public async UniTask<TScreen> ShowScreenAsync<TScreen>() where TScreen : BaseScreen
        {
            var screen = GetOrCreateScreen<TScreen>();

            if (_currentScreen == screen && screen.gameObject.activeSelf) return screen;
            if (_currentScreen != null   && _currentScreen != screen && _currentScreen.gameObject.activeSelf) await _currentScreen.HideAsync();

            _currentScreen = screen;
            await screen.ShowAsync();
            return screen;
        }

        public async UniTask HideScreenAsync<TScreen>() where TScreen : BaseScreen
        {
            var type = typeof(TScreen);
            if (!_ScreenCache.TryGetValue(type, out var screen))
            {
                Debug.LogWarning($"[UIController] HideScreenAsync<{type.Name}> called but Screen was never shown (not in cache).");
                return;
            }

            await screen.HideAsync();

            if (_currentScreen == screen) _currentScreen = null;
        }

        public void ShowScreen<TScreen>() where TScreen : BaseScreen => ShowScreenAsync<TScreen>().Forget();
        public void HideScreen<TScreen>() where TScreen : BaseScreen => HideScreenAsync<TScreen>().Forget();

        private TPopup GetOrCreatePopup<TPopup>() where TPopup : BasePopup
        {
            var type = typeof(TPopup);
            if (_popupCache.TryGetValue(type, out var cached) && cached != null) return (TPopup)cached;

            var prefab = LoadPopupPrefab<TPopup>();
            if (prefab == null)
            {
                Debug.LogError($"[UIController] Popup prefab for {type.Name} not found at {POPUPS_RESOURCES}/{type.Name}. Ensure the prefab exists in a Resources folder.");
                return null;
            }

            var instance = UnityEngine.Object.Instantiate(prefab, _uiParent.PopupParent);
            instance.gameObject.SetActive(false);

            var popup = instance.GetComponent<TPopup>();
            _popupCache[type] = popup;

            return popup;
        }

        private TScreen GetOrCreateScreen<TScreen>() where TScreen : BaseScreen
        {
            var type = typeof(TScreen);
            if (_ScreenCache.TryGetValue(type, out var cached) && cached != null) return (TScreen)cached;

            var prefab = LoadScreenPrefab<TScreen>();
            if (prefab == null)
            {
                Debug.LogError($"[UIController] Screen prefab for {type.Name} not found at {SCREENS_RESOURCES}/{type.Name}. Ensure the prefab exists in a Resources folder.");
                return null;
            }

            var instance = UnityEngine.Object.Instantiate(prefab, _uiParent.ScreenParent);
            instance.gameObject.SetActive(false);

            var screen = instance.GetComponent<TScreen>();
            _ScreenCache[type] = screen;

            return screen;
        }

        private TPopup  LoadPopupPrefab<TPopup>() where TPopup : BasePopup     => LoadViewPrefab<TPopup>(POPUPS_RESOURCES);
        private TScreen LoadScreenPrefab<TScreen>() where TScreen : BaseScreen => LoadViewPrefab<TScreen>(SCREENS_RESOURCES);

        private static T LoadViewPrefab<T>(string resourcesPath) where T : IUI
        {
            var type = typeof(T);
            var path = $"{resourcesPath}/{type.Name}";
            var go   = Resources.Load<GameObject>(path);
            return go.GetComponent<T>();
        }

        private void UpdateBackgroundForTopPopup()
        {
            var backgroundGO = _uiParent.BackgroundGO;
            if (backgroundGO == null) return;

            if (_popupStack.Count > 0)
            {
                var topPopup = _popupStack.Peek();
                if (topPopup != null && topPopup.gameObject.activeSelf)
                {
                    // Position background one step behind the top popup
                    var topPopupIndex = topPopup.transform.GetSiblingIndex();
                    backgroundGO.SetActive(true);
                    backgroundGO.transform.SetSiblingIndex(Mathf.Max(0, topPopupIndex - 1));
                }
                else
                {
                    // Top popup is inactive, hide background
                    backgroundGO.SetActive(false);
                }
            }
            else
            {
                // No popups in stack, hide background
                backgroundGO.SetActive(false);
            }
        }
    }
}