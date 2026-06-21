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
        private void OnValidate()
        {
            coinText = GetComponent<TMP_Text>();
        }
#endif

        private void Awake()
        {
            Debug.Log($"_playerData.GetHashCode():{_playerController.GetHashCode()}");
        }

        private void Update()
        {
            coinText.SetText(_playerController.PlayerData.CoinBalance.ToString());
        }
    }
}