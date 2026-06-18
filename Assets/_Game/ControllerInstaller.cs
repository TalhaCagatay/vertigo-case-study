using _Game.CardWheel.UIs;
using com.core.ui;
using Reflex.Core;
using Reflex.Enums;
using UnityEngine;
using Resolution = Reflex.Enums.Resolution;

namespace _Game
{
    public class ControllerInstaller : MonoBehaviour, IInstaller
    {
        public void InstallBindings(ContainerBuilder builder)
        {
            builder.RegisterType(typeof(CardWheelController),   Lifetime.Singleton, Resolution.Eager);
            builder.RegisterType(typeof(CardWheelUIController), Lifetime.Singleton, Resolution.Eager);
            builder.RegisterType(typeof(UIController),          Lifetime.Singleton, Resolution.Eager);
        }
    }
}