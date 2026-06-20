using UnityEditor;
using UnityEngine;
using Vertigo.CardWheel.Data;

namespace Vertigo.CardWheel.Editor
{
    public class ZoneWheelMappingEditorTab : EditorTabBase<ZoneWheelMapping>
    {
        protected override string AssetTypeLabel       => "Zone Wheel Mapping";
        protected override string CreateDialogTitle    => "Create Zone Wheel Mapping";
        protected override string CreateDialogPathRoot => "Assets/_Game/CardWheel/Configs";
        protected override string CreateDialogFileName => "ZoneWheelMapping.asset";

        protected override void DrawAssetFields()
        {
            base.DrawAssetFields();

            EditorGUILayout.Space(12);
            EditorGUILayout.LabelField("Zone Mapping Preview", EditorStyles.boldLabel);
            EditorGUILayout.LabelField
                (
                 "• Every 30th zone → Gold\n"   +
                 "• Every 5th zone  → Silver\n" +
                 "• All other zones → Bronze",
                 EditorStyles.wordWrappedLabel
                );
        }
    }
}