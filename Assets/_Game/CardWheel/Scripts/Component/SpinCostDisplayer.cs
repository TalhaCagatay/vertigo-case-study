using TMPro;
using UnityEngine;

namespace Vertigo.CardWheel.Component
{
    [RequireComponent(typeof(TMP_Text))]
    public class SpinCostDisplayer : MonoBehaviour
    {
        [SerializeField] private TMP_Text spinCostText;

#if UNITY_EDITOR
        private void OnValidate()
        {
            spinCostText = GetComponent<TMP_Text>();
        }
#endif

        public void SetSpinCost(int cost)
        {
            spinCostText.text = $"x{cost}";
        }
    }
}