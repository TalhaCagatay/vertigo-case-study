using UnityEngine;
using Vertigo.CardWheel.Controller;
using Vertigo.CardWheel.UIs.Screens;

namespace Vertigo.CardWheel.Data
{
    public abstract class AWheelSliceDefinition : ScriptableObject
    {
        public string id;

        [SerializeField] private Sprite     icon;
        [SerializeField] private string     label;
        [SerializeField] private GameObject viewPrefab;

        [SerializeField, HideInInspector] private CardWheelSliceView view;

        public string             Id         => id;
        public Sprite             Icon       => icon;
        public string             Label      => label;
        public CardWheelSliceView ViewPrefab => view;

#if UNITY_EDITOR
        private void OnValidate() => view = viewPrefab.GetComponent<CardWheelSliceView>();
#endif
        
        public abstract void Apply(CardWheelController  cardWheelController);
    }
}