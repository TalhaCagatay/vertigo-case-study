using Vertigo.CardWheel.Data;

namespace Vertigo.CardWheel.Editor
{
    public class WheelTierConfigEditorTab : EditorTabBase<WheelTierConfig>
    {
        protected override string AssetTypeLabel       => "Wheel Tier Config";
        protected override string CreateDialogTitle    => "Create Wheel Tier Config";
        protected override string CreateDialogPathRoot => "Assets/_Game/CardWheel/Configs";
        protected override string CreateDialogFileName => "WheelTierConfig.asset";
    }
}