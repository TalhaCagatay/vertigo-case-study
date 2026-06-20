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
            var so = _buffer.SerializedWorking;
            so.Update();

            var bronzeProp = so.FindProperty("bronzeConfig");
            var silverProp = so.FindProperty("silverConfig");
            var goldProp   = so.FindProperty("goldConfig");

            EditorGUILayout.PropertyField(bronzeProp, new GUIContent("Bronze Tier Config"));
            EditorGUILayout.PropertyField(silverProp, new GUIContent("Silver Tier Config"));
            EditorGUILayout.PropertyField(goldProp,   new GUIContent("Gold Tier Config"));

            EditorGUILayout.Space(12);
            EditorGUILayout.LabelField("Zone Mapping Preview", EditorStyles.boldLabel);
            EditorGUILayout.LabelField
                (
                 "• Every 30th zone → Gold\n"   +
                 "• Every 5th zone  → Silver\n" +
                 "• All other zones → Bronze",
                 EditorStyles.wordWrappedLabel
                );

            so.ApplyModifiedProperties();
        }
    }
}