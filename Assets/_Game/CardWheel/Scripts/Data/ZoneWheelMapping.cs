using UnityEngine;

namespace _Game.CardWheel.Data
{
    [CreateAssetMenu(fileName = "ZoneWheelMapping", menuName = "CardWheel/Zone Wheel Mapping", order = 3)]
    public class ZoneWheelMapping : ScriptableObject
    {
        [SerializeField] private WheelTierConfig bronzeConfig;
        [SerializeField] private WheelTierConfig silverConfig;
        [SerializeField] private WheelTierConfig goldConfig;

        public WheelTierConfig BronzeConfig => bronzeConfig;
        public WheelTierConfig SilverConfig => silverConfig;
        public WheelTierConfig GoldConfig   => goldConfig;

        public WheelTierConfig GetConfigForZone(int zone)
        {
            if (zone % 30 == 0) return goldConfig;
            if (zone % 5  == 0) return silverConfig;
            return bronzeConfig;
        }
    }
}