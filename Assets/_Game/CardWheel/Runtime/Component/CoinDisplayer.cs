using Reflex.Attributes;
using TMPro;
using UnityEngine;
using Vertigo.Player;

namespace Vertigo.CardWheel.Component
{
    [RequireComponent(typeof(TMP_Text))]
    public class CoinDisplayer : MonoBehaviour
    {
        [SerializeField] private TMP_Text coinText;

        [Inject] private PlayerController _playerController;

#if UNITY_EDITOR
        private void OnValidate() => coinText = GetComponent<TMP_Text>();
#endif

        private void Awake()
        {
            _playerController.BalanceUpdated += OnBalanceUpdated;
            OnBalanceUpdated(_playerController.CoinBalance);
        }

        private void OnDestroy() => _playerController.BalanceUpdated -= OnBalanceUpdated;

        private void OnBalanceUpdated(int coins) => coinText.SetText(coins.ToString());
    }
}