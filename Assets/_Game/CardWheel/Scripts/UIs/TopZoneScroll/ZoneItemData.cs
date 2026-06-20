using UnityEngine;

namespace Vertigo.CardWheel.UIs.TopZoneScroll
{
    public class ZoneItemData
    {
        public int    ZoneNumber             { get; }
        public string Label                  { get; }
        public bool   IsSuperZone            { get; }
        public bool   IsSafeZone             { get; }
        public bool   IsPastZone             { get; }
        public Color  ZoneNumberSelectedColor { get; }
        public Color  ZoneNumberPastColor    { get; }
        public Color  ZoneNumberFutureColor  { get; }

        public ZoneItemData(int    zoneNumber,
                            string label,
                            bool   isSuperZone,
                            bool   isSafeZone,
                            bool   isPastZone,
                            Color  zoneNumberSelectedColor,
                            Color  zoneNumberPastColor,
                            Color  zoneNumberFutureColor)
        {
            ZoneNumber             = zoneNumber;
            Label                  = label;
            IsSuperZone            = isSuperZone;
            IsSafeZone             = isSafeZone;
            IsPastZone             = isPastZone;
            ZoneNumberSelectedColor = zoneNumberSelectedColor;
            ZoneNumberPastColor    = zoneNumberPastColor;
            ZoneNumberFutureColor  = zoneNumberFutureColor;
        }
    }
}