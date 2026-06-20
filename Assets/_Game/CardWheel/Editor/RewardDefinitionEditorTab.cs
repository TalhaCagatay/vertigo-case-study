using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Vertigo.CardWheel.Data.Rewards;

namespace Vertigo.CardWheel.Editor
{
    public class RewardDefinitionEditorTab : EditorTabBase<ARewardDefinition>
    {
        private static readonly (Type Type, string Label)[] _rewardTypes =
        {
            (typeof(CoinReward), "Coin Reward"),
            (typeof(BombReward), "Bomb Reward"),
            (typeof(ChestReward), "Chest Reward"),
            (typeof(CosmeticReward), "Cosmetic Reward"),
            (typeof(SkillPointReward), "Skill Point Reward"),
        };

        private int                     _selectedRewardTypeIndex = 0;
        private int                     _typeFilterIndex         = 0;
        private List<ARewardDefinition> _allAssets               = new List<ARewardDefinition>();
        private bool                    _showCreatePopup         = false;

        protected override string AssetTypeLabel       => "Reward";
        protected override string CreateDialogTitle    => "Create Reward";
        protected override string CreateDialogPathRoot => "Assets/_Game/CardWheel/Configs";
        protected override string CreateDialogFileName => "NewReward.asset";

        private static string[] TypeFilterOptions => new[] { "All" }.Concat(_rewardTypes.Select(r => r.Label)).ToArray();

        protected override void RefreshAssetList()
        {
            var allGuids = AssetDatabase.FindAssets("t:ARewardDefinition");
            _allAssets = allGuids.Select(g => AssetDatabase.LoadAssetAtPath<ARewardDefinition>(AssetDatabase.GUIDToAssetPath(g))).Where(a => a != null).OrderBy(a => a.name).ToList();

            ApplyTypeFilter();
        }

        private void ApplyTypeFilter()
        {
            if (_typeFilterIndex == 0)
            {
                _assetList = _allAssets.ToList();
            }
            else
            {
                var selectedType = _rewardTypes[_typeFilterIndex - 1].Type;
                _assetList = _allAssets.Where(a => a.GetType() == selectedType).ToList();
            }

            _assetNames = _assetList.Select(a => $"[{a.GetType().Name}] {a.name}").ToArray();
        }

        protected override void DrawAssetListPanel()
        {
            EditorGUILayout.LabelField($"{AssetTypeLabel}s", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            _typeFilterIndex = EditorGUILayout.Popup("Filter", _typeFilterIndex, TypeFilterOptions);
            if (EditorGUI.EndChangeCheck())
            {
                ApplyTypeFilter();
                if (_selectedIndex >= _assetList.Count)
                {
                    _buffer?.Dispose();
                    _buffer        = null;
                    _selectedIndex = -1;
                }
            }

            EditorGUILayout.Space(4);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("New", GUILayout.Width(50)))
            {
                if (HandleUnsavedBeforeSwitch())
                {
                    _showCreatePopup = true;
                }
            }

            if (GUILayout.Button("Duplicate", GUILayout.Width(65)))
            {
                if (HandleUnsavedBeforeSwitch())
                {
                    DuplicateSelectedAsset();
                }
            }

            if (GUILayout.Button("Delete", GUILayout.Width(55)))
            {
                DeleteSelectedAsset();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(4);

            _selectionScrollPos = EditorGUILayout.BeginScrollView(_selectionScrollPos);
            var newIndex = GUILayout.SelectionGrid(_selectedIndex, _assetNames, 1);
            if (newIndex != _selectedIndex)
            {
                if (HandleUnsavedBeforeSwitch())
                {
                    SelectAsset(newIndex);
                }
            }

            EditorGUILayout.EndScrollView();

            if (_showCreatePopup)
            {
                DrawCreatePopup();
            }
        }

        private void DrawCreatePopup()
        {
            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Create New Reward", EditorStyles.boldLabel);

            _selectedRewardTypeIndex = EditorGUILayout.Popup
                (
                 "Reward Type",
                 _selectedRewardTypeIndex,
                 _rewardTypes.Select(r => r.Label).ToArray()
                );

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Create", GUILayout.Width(80)))
            {
                CreateRewardOfType(_rewardTypes[_selectedRewardTypeIndex].Type);
                _showCreatePopup = false;
            }

            if (GUILayout.Button("Cancel", GUILayout.Width(80)))
            {
                _showCreatePopup = false;
            }

            EditorGUILayout.EndHorizontal();
        }

        private void CreateRewardOfType(Type rewardType)
        {
            var path = EditorUtility.SaveFilePanelInProject
                (
                 CreateDialogTitle,
                 $"{rewardType.Name}.asset",
                 "asset",
                 $"Choose a location for the new {rewardType.Name}",
                 CreateDialogPathRoot
                );

            if (string.IsNullOrEmpty(path)) return;

            var template = (ARewardDefinition)ScriptableObject.CreateInstance(rewardType);
            template.name = System.IO.Path.GetFileNameWithoutExtension(path);

            _buffer?.Dispose();
            _buffer = new ConfigBuffer<ARewardDefinition>(template);
            _buffer.SetAssetPath(path);
            _selectedIndex = -1;
        }

        protected override void DrawAssetFields()
        {
            var so = _buffer.SerializedWorking;
            so.Update();

            EditorGUILayout.LabelField("Type", _buffer.Working.GetType().Name);

            EditorGUILayout.Space(4);

            var idProp     = so.FindProperty("id");
            var iconProp   = so.FindProperty("icon");
            var amountProp = so.FindProperty("amount");
            var labelProp  = so.FindProperty("label");

            EditorGUILayout.PropertyField(idProp,     new GUIContent("ID"));
            EditorGUILayout.PropertyField(labelProp,  new GUIContent("Label"));
            EditorGUILayout.PropertyField(amountProp, new GUIContent("Amount"));

            EditorGUILayout.Space(4);
            EditorGUILayout.PropertyField(iconProp, new GUIContent("Icon"));

            so.ApplyModifiedProperties();
        }
    }
}