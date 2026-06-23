using com.core.data;
using Vertigo.CardWheel.Controller;
using Vertigo.CardWheel.Data;
using Vertigo.CardWheel.UIs;
using com.core;
using com.core.ui;
using Reflex.Core;
using Reflex.Enums;
using UnityEngine;
using Vertigo.Player;
using Resolution = Reflex.Enums.Resolution;

namespace _Game
{
    public class ControllerInstaller : MonoBehaviour, IInstaller
    {
        [SerializeField] private ZoneWheelMapping zoneWheelMapping;

        public static Container Container { get; private set; }

        public void InstallBindings(ContainerBuilder builder)
        {
            ContainerScope.OnRootContainerBuilding += containerBuilder =>
            {
                containerBuilder.OnContainerBuilt += container =>
                {
                    Container = container;
                };
            };

            Debug.Log($"[ControllerInstaller] InstallBindings");
            builder.RegisterValue(zoneWheelMapping, new[] { typeof(ZoneWheelMapping) });

            builder.RegisterType(typeof(CardWheelController),   new[] { typeof(CardWheelController), typeof(IController) },   Lifetime.Singleton, Resolution.Eager);
            builder.RegisterType(typeof(CardWheelUIController), new[] { typeof(CardWheelUIController), typeof(IController) }, Lifetime.Singleton, Resolution.Eager);
            builder.RegisterType(typeof(UIController),          new[] { typeof(UIController), typeof(IController) },          Lifetime.Singleton, Resolution.Eager);
            builder.RegisterType(typeof(GameConfigController),  new[] { typeof(GameConfigController), typeof(IController) },  Lifetime.Singleton, Resolution.Eager);
            builder.RegisterType(typeof(DataController),        new[] { typeof(DataController), typeof(IController) },        Lifetime.Singleton, Resolution.Eager);
            builder.RegisterType(typeof(PlayerController),      new[] { typeof(PlayerController), typeof(IController) },      Lifetime.Singleton, Resolution.Eager);
        }
    }
}