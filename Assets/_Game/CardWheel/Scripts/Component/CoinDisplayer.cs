using Reflex.Attributes;
using TMPro;
using UnityEngine;
using Vertigo.CardWheel.Data;

namespace Vertigo.CardWheel.Component
{
    [RequireComponent(typeof(TMP_Text))]
    public class CoinDisplayer : MonoBehaviour
    {
        [SerializeField] private TMP_Text coinText;

        [Inject] private PlayerData _playerData;

#if UNITY_EDITOR
        private void OnValidate()
        {
            coinText = GetComponent<TMP_Text>();
        }
#endif

        private void Update()
        {
            coinText.text = _playerData.CoinBalance.ToString();
        }
    }
}