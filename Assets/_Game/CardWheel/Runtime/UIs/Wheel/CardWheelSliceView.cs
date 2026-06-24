using UnityEngine;
using Vertigo.CardWheel.Data;

namespace Vertigo.CardWheel.UIs.Screens
{
    public abstract class CardWheelSliceView : MonoBehaviour
    {
        public abstract void Setup(AWheelSliceDefinition wheelSliceDefinition);
    }
}