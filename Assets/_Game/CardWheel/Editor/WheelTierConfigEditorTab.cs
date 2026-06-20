using UnityEditor;
using UnityEngine;
using Vertigo.CardWheel.Data;

namespace Vertigo.CardWheel.Editor
{
    public class WheelTierConfigEditorTab : EditorTabBase<WheelTierConfig>
    {
        protected override string AssetTypeLabel       => "Wheel Tier Config";
        protected override string CreateDialogTitle    => "Create Wheel Tier Config";
        protected override string CreateDialogPathRoot => "Assets/_Game/CardWheel/Configs";
        protected override string CreateDialogFileName => "WheelTierConfig.asset";

        private bool _slicesFoldout = true;

        protected override void DrawAssetFields()
        {
            var so = _buffer.SerializedWorking;
            so.Update();

            var spinnerProp   = so.FindProperty("spinnerSprite");
            var indicatorProp = so.FindProperty("indicatorSprite");
            EditorGUILayout.PropertyField(spinnerProp,   new GUIContent("Spinner Sprite"));
            EditorGUILayout.PropertyField(indicatorProp, new GUIContent("Indicator Sprite"));

            EditorGUILayout.Space(8);

            var selectedColorProp = so.FindProperty("zoneNumberSelectedColor");
            var pastColorProp     = so.FindProperty("zoneNumberPastColor");
            var futureColorProp   = so.FindProperty("zoneNumberFutureColor");
            EditorGUILayout.PropertyField(selectedColorProp, new GUIContent("Selected Color"));
            EditorGUILayout.PropertyField(pastColorProp,     new GUIContent("Past Color"));
            EditorGUILayout.PropertyField(futureColorProp,   new GUIContent("Future Color"));

            EditorGUILayout.Space(8);

            var spinDurationProp = so.FindProperty("spinDuration");
            EditorGUILayout.PropertyField(spinDurationProp, new GUIContent("Spin Duration"));

            EditorGUILayout.Space(8);

            var curveProp = so.FindProperty("rewardScaleCurve");
            EditorGUILayout.PropertyField(curveProp, new GUIContent("Reward Scale Curve"));

            EditorGUILayout.Space(8);

            _slicesFoldout = EditorGUILayout.Foldout(_slicesFoldout, "Slices (Rewards)", true);
            if (_slicesFoldout)
            {
                EditorGUI.indentLevel++;
                var slicesProp = so.FindProperty("slices");
                EditorGUILayout.PropertyField(slicesProp, new GUIContent("Reward Definitions"), true);
                EditorGUI.indentLevel--;
            }

            so.ApplyModifiedProperties();
        }
    }
}