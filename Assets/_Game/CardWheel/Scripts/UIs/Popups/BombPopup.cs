using System;
using com.core.ui;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Vertigo.CardWheel.UIs.Popups
{
    public class BombPopup : BasePopup
    {
        [SerializeField] private Button giveUpButton;
        [SerializeField] private Button reviveButton;

        private event Action GiveUpClicked;
        private event Action ReviveClicked;

        private void Awake()
        {
            giveUpButton.onClick.AddListener(OnGiveUpClicked);
            reviveButton.onClick.AddListener(OnReviveClicked);
        }

        public void Setup(Action onGiveUp, Action onRevive)
        {
            GiveUpClicked = onGiveUp;
            ReviveClicked = onRevive;
        }

        private void OnGiveUpClicked() => GiveUpClicked?.Invoke();
        private void OnReviveClicked() => ReviveClicked?.Invoke();

        private void OnDestroy()
        {
            giveUpButton.onClick.RemoveListener(OnGiveUpClicked);
            reviveButton.onClick.RemoveListener(OnReviveClicked);
        }
    }
}