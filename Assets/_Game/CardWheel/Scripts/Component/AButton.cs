using System;
using UnityEngine;
using UnityEngine.UI;

namespace Vertigo.CardWheel.Component
{
    [RequireComponent(typeof(Button))]
    public class AButton<T> : MonoBehaviour
    {
        public event Action Clicked;

        [SerializeField] private Graphic[] graphics;

        public Button button;

#if UNITY_EDITOR
        private void OnValidate()
        {
            button   = GetComponent<Button>();
            graphics = GetComponentsInChildren<Graphic>();
        }
#endif

        private void Start() => button.onClick.AddListener(() => Clicked?.Invoke());

        public void SetInteractable(bool interactable)
        {
            button.interactable = interactable;
            Array.ForEach(graphics, graphic => graphic.color = interactable ? button.colors.normalColor : button.colors.disabledColor);
        }
    }
}