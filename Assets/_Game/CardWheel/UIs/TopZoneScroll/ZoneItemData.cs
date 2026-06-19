namespace _Game.CardWheel.UIs.TopZoneScroll
{
    public class ZoneItemData
    {
        public int    ZoneNumber  { get; }
        public string Label       { get; }
        public bool   IsSuperZone { get; }
        public bool   IsSafeZone  { get; }
        public bool   IsPastZone  { get; }

        public ZoneItemData(int zoneNumber, string label, bool isSuperZone, bool isSafeZone, bool isPastZone)
        {
            ZoneNumber  = zoneNumber;
            Label       = label;
            IsSuperZone = isSuperZone;
            IsSafeZone  = isSafeZone;
            IsPastZone  = isPastZone;
        }
    }
}