using UnityEngine;

namespace _Game.CardWheel.Data
{
    [CreateAssetMenu(fileName = "ZoneWheelMapping", menuName = "CardWheel/Zone Wheel Mapping", order = 3)]
    public class ZoneWheelMapping : ScriptableObject
    {
        [SerializeField] private WheelTierConfig bronzeConfig;
        [SerializeField] private WheelTierConfig silverConfig; // Used every 5th zone (safe zone)
        [SerializeField] private WheelTierConfig goldConfig;   // Used every 30th zone (super zone)

        public WheelTierConfig BronzeConfig => bronzeConfig;
        public WheelTierConfig SilverConfig => silverConfig;
        public WheelTierConfig GoldConfig   => goldConfig;

        /// <summary>
        /// Determines the wheel tier config based on the current zone number.
        /// Zone numbers are 1-based (first spin = zone 1).
        /// Every 30th zone = Gold (super zone, no bomb)
        /// Every 5th zone = Silver (safe zone, no bomb)
        /// Otherwise = Bronze
        /// </summary>
        public WheelTierConfig GetConfigForZone(int zone)
        {
            if (zone % 30 == 0 && goldConfig != null)
                return goldConfig;

            if (zone % 5 == 0 && silverConfig != null)
                return silverConfig;

            return bronzeConfig;
        }
    }
}
