using com.core.data;
using Vertigo.CardWheel.Controller;
using Vertigo.CardWheel.Data;
using Vertigo.CardWheel.UIs;
using com.core;
using com.core.ui;
using Reflex.Core;
using Reflex.Enums;
using UnityEngine;
using Resolution = Reflex.Enums.Resolution;

namespace _Game
{
    public class ControllerInstaller : MonoBehaviour, IInstaller
    {
        [SerializeField] private ZoneWheelMapping zoneWheelMapping;

        public void InstallBindings(ContainerBuilder builder)
        {
            Debug.Log($"[ControllerInstaller] InstallBindings");
            builder.RegisterValue(zoneWheelMapping, new[] { typeof(ZoneWheelMapping) });

            builder.RegisterType(typeof(CardWheelController),   Lifetime.Singleton, Resolution.Eager);
            builder.RegisterType(typeof(CardWheelUIController), Lifetime.Singleton, Resolution.Eager);
            builder.RegisterType(typeof(UIController),          Lifetime.Singleton, Resolution.Eager);
            builder.RegisterType(typeof(GameConfigController),  Lifetime.Singleton, Resolution.Eager);
            builder.RegisterType(typeof(DataController),        Lifetime.Singleton, Resolution.Eager);
            builder.RegisterType(typeof(PlayerData),            Lifetime.Singleton, Resolution.Eager);
        }
    }
}